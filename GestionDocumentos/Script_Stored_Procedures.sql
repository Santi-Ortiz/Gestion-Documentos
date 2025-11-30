
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------------

-- Procedimiento Almacenado para el procesamiento de acciones de validación de documentos

CREATE PROCEDURE sp_ProcesarAccionValidacion
	@documentoId uniqueidentifier, 
	@actorUserId uniqueidentifier,
	@accion nvarchar(10),
	@razon nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @actorOrder int;

	-- Se hace la validación si el actorUserId pertenece a la validación del documento
	SELECT @actorOrder = JSON_VALUE(step.value, '$.order')
	FROM DOCUMENTO doc
	CROSS APPLY OPENJSON(doc.flujoValidacionJson, '$.steps') AS step
	WHERE doc.documentoId = @documentoId
		AND TRY_CAST(JSON_VALUE(step.value, '$.userId') AS uniqueidentifier) = @actorUserId;

	-- Si es nulo entonces se retorna un error
	IF @actorOrder IS NULL
	BEGIN
		RAISERROR('El actor no hace parte del flujo de validación del documento',16, 1);
		RETURN;
	END

	-- Se comprueba el siguiente paso de validación pendiente

	DECLARE @nextPendingOrder int;

	SELECT @nextPendingOrder = MIN(CAST(JSON_VALUE(step.value, '$.order') AS INT ))
	FROM DOCUMENTO doc
	CROSS APPLY OPENJSON(doc.flujoValidacionJson, '$.steps') AS step
	WHERE doc.documentoId = @documentoId
		AND NOT EXISTS
		(
			SELECT 1
			FROM INSTANCIA_VALIDACION iv
			WHERE iv.documentoId = doc.documentoId
				AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
				AND UPPER(iv.accion) = 'APROBAR'
		);

    IF EXISTS (
    SELECT 1
    FROM DOCUMENTO doc
    CROSS APPLY OPENJSON(doc.flujoValidacionJson, '$.steps') step
    WHERE doc.documentoId = @documentoId
        AND CAST(JSON_VALUE(step.value, '$.order') AS INT) < @actorOrder
        AND NOT EXISTS (
            SELECT 1
            FROM INSTANCIA_VALIDACION iv
            WHERE iv.documentoId = doc.documentoId
                AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
                AND UPPER(iv.accion) = 'APROBAR'
        )
    )
    BEGIN
        RAISERROR('El actor no puede aprobar: existen pasos previos pendientes.',16,1);
        RETURN;
    END

	BEGIN TRY

	    BEGIN TRAN;
        
        -- Si la acción es rechazar entonces
        IF @accion = 'RECHAZAR'
        BEGIN
            DECLARE @estadoAnteriorR CHAR(1);

            -- Se busca el documento a revisar
            SELECT @estadoAnteriorR = estado
            FROM DOCUMENTO doc
            WHERE doc.documentoId = @documentoId;

            -- Se registra el estado del documento en InstanciaValidacion
            INSERT INTO INSTANCIA_VALIDACION (
                validacionId, userId, ordenPaso, accion, fechaRevision, documentoId
            )
            VALUES (
                NEWID(), @actorUserId, ISNULL(@nextPendingOrder, @actorOrder), 'Rechazar', SYSDATETIME(), @documentoId
            )

            -- Se actualiza el estado del documento en la tabla Documento
            UPDATE DOCUMENTO
            SET estado = 'R'
            WHERE documentoId = @documentoId

            COMMIT;
            RETURN;
        END

        IF @accion = 'APROBAR'
        BEGIN

            -- Se valida que el actor unicamente pueda validar si existe una orden posterior y si es el turno de él
            IF @nextPendingOrder IS NOT NULL AND @actorOrder < @nextPendingOrder
            BEGIN
                RAISERROR('El actor no puede aprobar: no es su turno.', 16, 1);
                ROLLBACK;
                RETURN;
            END

            
            -- Se insertan las aprobaciones para todas las etapas 
            INSERT INTO INSTANCIA_VALIDACION(
                validacionId, userId, ordenPaso, accion, fechaRevision, documentoId
            )
            SELECT NEWID(), @actorUserId, CAST(JSON_VALUE(step.value, '$.order') AS INT), 'Aprobar', SYSDATETIME(), doc.documentoId
            FROM DOCUMENTO doc
            CROSS APPLY OPENJSON(doc.flujoValidacionJson, '$.steps') AS step
            WHERE doc.documentoId = @documentoId
              AND CAST(JSON_VALUE(step.value, '$.order') AS INT) <= @actorOrder
              AND NOT EXISTS (
                    SELECT 1
                    FROM INSTANCIA_VALIDACION iv
                    WHERE iv.documentoId = doc.documentoId
                      AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
                      AND UPPER(iv.accion) = 'APROBAR'
              );

            -- Se evalua si restan pasos pendientes de aprobación para el documento 

            DECLARE @remaining INT;

            -- Devuelve la cantidad de pasos pendientes
            SELECT @remaining = COUNT(*)
            FROM DOCUMENTO doc
            CROSS APPLY OPENJSON(doc.flujoValidacionJson, '$.steps') AS step
            WHERE doc.documentoId = @documentoId
              AND NOT EXISTS(
                    SELECT 1 FROM INSTANCIA_VALIDACION iv
                    WHERE iv.documentoId = doc.documentoId
                      AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
                      AND UPPER(iv.accion) = 'APROBAR'
              );

            DECLARE @estadoActual CHAR(1);
            DECLARE @estadoNuevo CHAR(1);

            SELECT @estadoActual = estado
            FROM DOCUMENTO doc
            WHERE doc.documentoId = @documentoId;

            SET @estadoNuevo = CASE WHEN @remaining = 0 THEN 'A' ELSE 'P' END;

            -- Para los casos donde estadoActual != estadoNuevo
            IF @estadoActual <> @estadoNuevo
            BEGIN
                -- Se actualiza el documento con el estado nuevo
                UPDATE DOCUMENTO
                SET estado = @estadoNuevo
                WHERE documentoId = @documentoId;

            END

            COMMIT;
            RETURN;

        END

    END TRY
    
    BEGIN CATCH
        -- Se cancela alguna transacción que esté en curso
        IF XACT_STATE() <> 0 ROLLBACK;

        DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @sev INT = ERROR_SEVERITY();
        DECLARE @sta INT = ERROR_STATE();

        -- Se notifica si existe un error con el mensaje, severidad y el estado
        RAISERROR(@msg, @sev, @sta);
    END CATCH
        

END
GO


--------------------------------------------------------
-- Procedimiento Almacenado para el listado de los documentos pendientes

CREATE PROCEDURE sp_ListarDocumentosPendientes 
	@actorUserId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    -- Un documento se encuentra pendiente cuando:
    -- El estado es 'P'
    -- Tiene al menos un paso del flujo pendiente sin aprobar

    SELECT
        d.documentoId,
        d.urlArchivo,
        d.estado,
        d.fechaCreacion,
        e.razonSocial AS empresaNombre,
        e.NIT,

        -- Se busca un próximo paso pendiente
        (
            SELECT MIN(CAST(JSON_VALUE(step.value, '$.order') AS INT))
            FROM OPENJSON(d.flujoValidacionJson, '$.steps') AS step
            WHERE NOT EXISTS (
                SELECT 1
                FROM INSTANCIA_VALIDACION iv
                WHERE iv.documentoId = d.documentoId
                  AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
                  AND UPPER(iv.accion) = 'APROBAR'
            )
        ) AS siguientePasoPendiente,

        -- Se buscan los aprobadores del siguiente paso
        (
            SELECT STRING_AGG(
                JSON_VALUE(step2.value, '$.userId'),
                ','
            )
            FROM OPENJSON(d.flujoValidacionJson, '$.steps') AS step2
            WHERE CAST(JSON_VALUE(step2.value, '$.order') AS INT) =
            (
                SELECT MIN(CAST(JSON_VALUE(step.value, '$.order') AS INT))
                FROM OPENJSON(d.flujoValidacionJson, '$.steps') AS step
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM INSTANCIA_VALIDACION iv
                    WHERE iv.documentoId = d.documentoId
                      AND iv.ordenPaso = CAST(JSON_VALUE(step.value, '$.order') AS INT)
                      AND UPPER(iv.accion) = 'APROBAR'
                )
            )
        ) AS aprobadoresResponsables

        FROM DOCUMENTO d JOIN EMPRESA e ON e.empresaId = d.empresaId
        WHERE d.estado = 'P';

END
GO

--------------------------------------------------------
-- Procedimiento Almacenado para observar el historial de las validaciones de los documentos
CREATE PROCEDURE sp_HistorialValidacionesDocumentos 
	@documentoId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
        iv.validacionId,
        iv.userId,
        iv.ordenPaso,
        iv.accion,
        iv.fechaRevision
    FROM INSTANCIA_VALIDACION iv
    WHERE iv.documentoId = @documentoId
    ORDER BY iv.ordenPaso, iv.fechaRevision;

END
GO

--------------------------------------------------------
-- Procedimiento Almacenado para observar los documentos donde se haya realizado un cambio de estado

CREATE PROCEDURE sp_HistorialDocumentoAuditoria
    @documentoId uniqueidentifier
AS
BEGIN

    SELECT
        da.auditoriaId,
        da.estadoAnterior,
        da.estadoNuevo,
        da.fechaCambio,
        da.userId
    FROM DOCUMENTO_AUDITORIA da
    WHERE da.documentoId = @documentoId
    ORDER BY da.fechaCambio;

END
GO