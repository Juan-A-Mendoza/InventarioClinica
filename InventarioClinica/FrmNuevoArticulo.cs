using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace InventarioClinica
{
    public partial class FrmNuevoArticulo : Form
    {
        private string cadenaConexion = "Data Source=InventarioClinica.db";

        public FrmNuevoArticulo()
        {
            InitializeComponent();
        }

        // Carga los estantes apenas se abre la ventanita
        private void FrmNuevoArticulo_Load(object sender, EventArgs e)
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, Nombre FROM Estantes";
                using (var comando = new SqliteCommand(query, conexion))
                {
                    using (var reader = comando.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        cmbEstante.DataSource = dt;
                        cmbEstante.DisplayMember = "Nombre";
                        cmbEstante.ValueMember = "Id";
                    }
                }
            }
        }

        // Guarda el artículo cuando el usuario hace clic
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Pequeña validación para que no dejen campos vacíos importantes
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El Código y el Nombre son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                // Instrucción SQL para insertar los datos en la tabla Articulos
                string query = @"INSERT INTO Articulos (Codigo, Nombre, Presentacion, IdEstante) 
                                 VALUES (@Codigo, @Nombre, @Presentacion, @IdEstante)";

                using (var comando = new SqliteCommand(query, conexion))
                {
                    // Pasamos los valores de las cajas de texto de forma segura
                    comando.Parameters.AddWithValue("@Codigo", txtCodigo.Text);
                    comando.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                    comando.Parameters.AddWithValue("@Presentacion", txtPresentacion.Text);
                    comando.Parameters.AddWithValue("@IdEstante", cmbEstante.SelectedValue);

                    try
                    {
                        comando.ExecuteNonQuery(); // Ejecuta el guardado
                        MessageBox.Show("Artículo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // Cierra esta ventanita automáticamente
                    }
                    catch (SqliteException ex)
                    {
                        // Si el código ya existe, SQLite nos avisará porque lo pusimos como PRIMARY KEY
                        if (ex.SqliteErrorCode == 19) // 19 es el código de error para restricción UNIQUE (duplicado)
                        {
                            MessageBox.Show("Ya existe un artículo con ese código.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Error al guardar: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}