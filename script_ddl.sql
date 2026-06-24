-- ============================================================
-- PARTE A: Script DDL - Base de datos del Simulador de Dron
-- ============================================================

-- Tabla maestra: almacena la cabecera de cada ejecución exitosa
CREATE TABLE IF NOT EXISTS tb_master_control (
    id          SERIAL      PRIMARY KEY,
    fecha       TIMESTAMP   NOT NULL DEFAULT NOW(),
    terreno_n   INTEGER     NOT NULL,
    coord_x     INTEGER     NOT NULL,
    coord_y     INTEGER     NOT NULL
);

-- Tabla detalle: almacena el rastro de movimientos de cada ejecución
CREATE TABLE IF NOT EXISTS tb_det_log (
    id              SERIAL      PRIMARY KEY,
    master_id       INTEGER     NOT NULL REFERENCES tb_master_control(id),
    paso_ofuscado   INTEGER     NOT NULL,
    pos_x           INTEGER     NOT NULL,
    pos_y           INTEGER     NOT NULL
);
