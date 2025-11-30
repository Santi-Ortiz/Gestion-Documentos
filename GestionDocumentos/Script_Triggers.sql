
--------------------------------------------------------
-- Trigger para detectar cada vez que se inserte un nuevo registro en Documento
CREATE TRIGGER tr_Documento_Insert_InstanciaInicial
ON DOCUMENTO
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Por cada documento insertado, 
    -- se busca el primer paso del flujo por el orden más bajo, se obtiene el userId y se registra en InstanciaValidacion
    

    INSERT INTO INSTANCIA_VALIDACION (
        validacionId, userId, ordenPaso, accion, fechaRevision, documentoId
    )
    SELECT 
        NEWID(),
        TRY_CAST(JSON_VALUE(step.value, '$.userId') AS UNIQUEIDENTIFIER),
        CAST(JSON_VALUE(step.value, '$.order') AS INT),
        'Pendiente', -- Se inserta el estado 'Pendiente' ya que no ha sido revisado por ninguna persona                       
        SYSDATETIME(), 
        i.documentoId
    FROM INSERTED i
    CROSS APPLY OPENJSON(i.flujoValidacionJson, '$.steps') AS step
    WHERE CAST(JSON_VALUE(step.value, '$.order') AS INT) =
    (
        SELECT MIN(CAST(JSON_VALUE(step2.value, '$.order') AS INT))
        FROM OPENJSON(i.flujoValidacionJson, '$.steps') AS step2
    );
END
GO

--------------------------------------------------------
-- Trigger para detectar los cambios en el campo estado del documento

CREATE TRIGGER tr_Documento_Update_AuditoriaEstado
ON DOCUMENTO
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DOCUMENTO_AUDITORIA (
        estadoAnterior,
        estadoNuevo,
        fechaCambio,
        userId,
        documentoId
    )
    SELECT
        d.estado,
        i.estado,
        SYSDATETIME(),
        NULL,       
        i.documentoId
    FROM DELETED d
    INNER JOIN INSERTED i ON d.documentoId = i.documentoId
    WHERE RTRIM(ISNULL(d.estado,'')) <> RTRIM(ISNULL(i.estado,''));
END
GO


