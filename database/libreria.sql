-- ============================================================
-- Script de base de datos: Librería "Páginas del Sur" (Tarija)
-- Ejecutar en PostgreSQL local.
-- Paso 1: crear la base de datos (conectado a la BD "postgres"):
--     CREATE DATABASE libreria;
-- Paso 2: conectarse a "libreria" (\c libreria) y ejecutar el resto.
-- ============================================================

DROP TABLE IF EXISTS libros;
DROP TABLE IF EXISTS proveedores;

CREATE TABLE proveedores (
    id        SERIAL        PRIMARY KEY,
    nombre    VARCHAR(100)  NOT NULL,
    telefono  VARCHAR(20),
    correo    VARCHAR(100)
);

CREATE TABLE libros (
    id            SERIAL         PRIMARY KEY,
    titulo        VARCHAR(150)   NOT NULL,
    autor         VARCHAR(100)   NOT NULL,
    isbn          VARCHAR(20)    NOT NULL UNIQUE,
    precio        NUMERIC(10,2)  NOT NULL CHECK (precio >= 0),
    stock         INT            NOT NULL DEFAULT 0 CHECK (stock >= 0),
    proveedor_id  INT            NOT NULL,
    CONSTRAINT fk_libros_proveedor
        FOREIGN KEY (proveedor_id) REFERENCES proveedores (id)
        ON DELETE RESTRICT
);

-- Datos de ejemplo
INSERT INTO proveedores (nombre, telefono, correo) VALUES
    ('Distribuidora Los Andes', '4-6641234', 'ventas@losandes.bo'),
    ('Editorial Plural',        '2-2411111', 'contacto@plural.bo');

INSERT INTO libros (titulo, autor, isbn, precio, stock, proveedor_id) VALUES
    ('Cien años de soledad', 'Gabriel García Márquez', '978-0307474728', 120.00, 15, 1),
    ('El principito',        'Antoine de Saint-Exupéry', '978-0156012195',  80.50, 30, 1),
    ('Metal y melancolía',   'Alcides Arguedas',         '978-9990512345',  95.00,  8, 2);
