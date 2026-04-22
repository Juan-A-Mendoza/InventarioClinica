using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace InventarioClinica
{
    public class BaseDatos
    {
        private string cadenaConexion = "Data Source=InventarioClinica.db";

        public void InicializarBaseDeDatos()
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                // Usamos IF NOT EXISTS para que SQLite se encargue de crearlas si faltan,
                // sin importar si el archivo .db ya estaba creado a medias.
                string crearTablas = @"
                    CREATE TABLE IF NOT EXISTS Estantes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS Articulos (
                        Codigo TEXT PRIMARY KEY,
                        Nombre TEXT NOT NULL,
                        Presentacion TEXT,
                        Concentracion TEXT,
                        MaximaCantidad INTEGER,
                        PideMasVencera TEXT,
                        MinimaCantidad INTEGER,
                        IdEstante INTEGER,
                        FOREIGN KEY(IdEstante) REFERENCES Estantes(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Movimientos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CodigoArticulo TEXT,
                        Fecha TEXT NOT NULL,
                        Documento TEXT,
                        Entrada INTEGER,
                        Salida INTEGER,
                        Existencia INTEGER NOT NULL,
                        Observaciones TEXT,
                        Lote TEXT,
                        FechaCompra TEXT,
                        FechaVencimiento TEXT,                      
                        FOREIGN KEY(CodigoArticulo) REFERENCES Articulos(Codigo)
                    );";

                // Ejecutamos la creación de tablas
                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = crearTablas;
                    comando.ExecuteNonQuery();
                }

                // Verificamos si la tabla de estantes está vacía antes de insertar los 18 por defecto
                using (var comandoCheck = conexion.CreateCommand())
                {
                    comandoCheck.CommandText = "SELECT COUNT(*) FROM Estantes";
                    long cantidadEstantes = (long)comandoCheck.ExecuteScalar();

                    if (cantidadEstantes == 0)
                    {
                        CrearEstantesPorDefecto(conexion);
                    }
                }
            }
        }

        private void CrearEstantesPorDefecto(SqliteConnection conexion)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "BEGIN TRANSACTION;";
                for (int i = 1; i <= 18; i++)
                {
                    comando.CommandText += $"INSERT INTO Estantes (Nombre) VALUES ('Estante {i}');";
                }
                comando.CommandText += "COMMIT;";
                comando.ExecuteNonQuery();
            }
        }
    }
}