
-- Script de DDL (Data Definition Language)

DROP TABLE IF EXISTS DOCUMENTO_AUDITORIA;
DROP TABLE IF EXISTS INSTANCIA_VALIDACION;
DROP TABLE IF EXISTS DOCUMENTO;
DROP TABLE IF EXISTS EMPRESA;

-- CREACIÓN DE TABLAS

-- Tabla Empresa
CREATE TABLE EMPRESA (
	empresaId uniqueidentifier UNIQUE NOT NULL DEFAULT NEWSEQUENTIALID(),
	NIT int UNIQUE NOT NULL,
	razonSocial nvarchar(100) NOT NULL,
	ubicacion nvarchar(100) NOT NULL,
	numeroEmpleados int NOT NULL

	PRIMARY KEY(empresaId)
);


-- Tabla Documento
CREATE TABLE DOCUMENTO (
	documentoId uniqueidentifier UNIQUE NOT NULL DEFAULT NEWSEQUENTIALID(),
	urlArchivo nvarchar(500) NOT NULL,
	estado varchar(1) NOT NULL,
	fechaCreacion datetime2 NOT NULL,
	flujoValidacionJson nvarchar(MAX) NOT NULL,
	empresaId uniqueidentifier NOT NULL,

	PRIMARY KEY(documentoId),
	FOREIGN KEY(empresaId) REFERENCES EMPRESA(empresaId)
);

CREATE INDEX idxDocumentoEstado ON DOCUMENTO(estado);


CREATE TABLE INSTANCIA_VALIDACION(
	validacionId uniqueidentifier UNIQUE NOT NULL DEFAULT NEWSEQUENTIALID(),
	userId uniqueidentifier NOT NULL,
	ordenPaso int NOT NULL,
	accion nvarchar(10) NOT NULL,
	fechaRevision datetime2 NOT NULL,
	documentoId uniqueidentifier NOT NULL,

	PRIMARY KEY (validacionId),
	FOREIGN KEY (documentoId) REFERENCES DOCUMENTO(documentoId)

);

-- Índices para tabla InstanciaValidacion
CREATE INDEX idxUserId ON INSTANCIA_VALIDACION(userId);
CREATE INDEX idxDocumentoOrden ON INSTANCIA_VALIDACION(documentoId, ordenPaso);


CREATE TABLE DOCUMENTO_AUDITORIA(
	auditoriaId uniqueidentifier UNIQUE NOT NULL DEFAULT NEWSEQUENTIALID(),
	estadoAnterior varchar(1) NOT NULL,
	estadoNuevo varchar(1) NOT NULL,
	fechaCambio datetime2 NOT NULL,
	userId uniqueidentifier NULL,
	documentoId uniqueidentifier NOT NULL,

	PRIMARY KEY (auditoriaId),
	FOREIGN KEY (documentoId) REFERENCES DOCUMENTO(documentoId)

);

-- Índices para tabla DocumentoAuditoria
CREATE INDEX idxUserIdAuditoria ON DOCUMENTO_AUDITORIA(userId);
CREATE INDEX idxDocumentoAuditoria ON DOCUMENTO_AUDITORIA(documentoId);

