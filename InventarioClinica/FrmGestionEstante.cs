using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventarioClinica
{
    public partial class FrmGestionEstante : Form
    {
        private string cadenaConexion = "Data Source=InventarioClinica.db";
        public long? IdEstante { get; set; } // Si es null, es Agregar. Si tiene valor, es Editar.

        public FrmGestionEstante() { InitializeComponent(); }

        private void FrmGestionEstante_Load(object sender, EventArgs e)
        {
            if (IdEstante.HasValue) // Si vamos a editar, cargamos el nombre actual
            {
                this.Text = "Editar Estante";
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var cmd = new SqliteCommand("SELECT Nombre FROM Estantes WHERE Id = @id", conexion);
                    cmd.Parameters.AddWithValue("@id", IdEstante.Value);
                    txtNombreEstante.Text = cmd.ExecuteScalar()?.ToString();
                }
            }
            else { this.Text = "Nuevo Estante"; }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // 1. Validación para que no guarden un estante sin nombre
            if (string.IsNullOrWhiteSpace(txtNombreEstante.Text))
            {
                MessageBox.Show("El nombre del estante no puede estar vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                string query = IdEstante.HasValue
                    ? "UPDATE Estantes SET Nombre = @nombre WHERE Id = @id"
                    : "INSERT INTO Estantes (Nombre) VALUES (@nombre)";

                using (var cmd = new SqliteCommand(query, conexion))
                {
                    // Usamos Trim() para evitar que el usuario deje espacios en blanco por accidente al final
                    cmd.Parameters.AddWithValue("@nombre", txtNombreEstante.Text.Trim());
                    if (IdEstante.HasValue) cmd.Parameters.AddWithValue("@id", IdEstante.Value);

                    try
                    {
                        cmd.ExecuteNonQuery(); // Intentamos guardar

                        // Si todo sale bien, cerramos la ventana
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (SqliteException ex)
                    {
                        // Si el error es 19 (violación de restricción UNIQUE en SQLite)
                        if (ex.SqliteErrorCode == 19)
                        {
                            MessageBox.Show("Ya existe un estante con este nombre. Por favor, elige un nombre diferente.", "Nombre Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // Por si ocurre cualquier otro error imprevisto
                            MessageBox.Show("Ocurrió un error en la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
