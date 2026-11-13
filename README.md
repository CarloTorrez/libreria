# PrototipoCRUD - Librería "Páginas del Sur"

Prototipo CRUD en C# (.NET MVC) con PostgreSQL y Npgsql. Capas: Models, Views, Controllers y Data (DAO).

## Cómo ejecutarlo
1. Instalar .NET SDK 8 y PostgreSQL.
2. En PostgreSQL: `CREATE DATABASE libreria;` y luego ejecutar `database/libreria.sql` dentro de esa base.
3. Editar la contraseña en `Data/ConexionBD.cs` (o definir la variable de entorno `LIBRERIA_DB`).
4. En la terminal, dentro de la carpeta del proyecto: `dotnet restore` y `dotnet run`.
5. Abrir la URL que muestra la terminal (por ejemplo https://localhost:5001).

## Funcionalidad
- Libros: listar, buscar por título o autor, crear, editar y eliminar.
- Proveedores: listar, crear, editar y eliminar (no deja eliminar uno con libros asociados).
