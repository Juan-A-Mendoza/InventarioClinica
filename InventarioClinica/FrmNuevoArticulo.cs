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

        public string CodigoParaEditar { get; set; } // Si está vacío es agregar, si tiene texto es editar

        // Carga los estantes apenas se abre la ventanita
        private void FrmNuevoArticulo_Load(object sender, EventArgs e)
        {
            // 1. Primero cargamos la lista de estantes en el ComboBox
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

            // 2. Si nos enviaron un código para editar, cambiamos el modo de la ventana
            if (!string.IsNullOrEmpty(CodigoParaEditar))
            {
                this.Text = "Editar Artículo";
                btnGuardar.Text = "Actualizar";
                txtCodigo.ReadOnly = true; // Bloqueamos el código para que no lo dañen
                CargarDatosArticulo();
            }
        }

        // Guarda o Actualiza el artículo cuando el usuario hace clic
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
                string query;

                if (string.IsNullOrEmpty(CodigoParaEditar))
                {
                    query = @"INSERT INTO Articulos (Codigo, Nombre, Presentacion, Concentracion, MaximaCantidad, PideMasVencera, MinimaCantidad, IdEstante) 
                              VALUES (@Codigo, @Nombre, @Presentacion, @Concentracion, @MaximaCantidad, @PideMasVencera, @MinimaCantidad, @IdEstante)";
                }
                else
                {
                    query = @"UPDATE Articulos 
                              SET Nombre = @Nombre, Presentacion = @Presentacion, Concentracion = @Concentracion, 
                                  MaximaCantidad = @MaximaCantidad, PideMasVencera = @PideMasVencera, MinimaCantidad = @MinimaCantidad, IdEstante = @IdEstante 
                              WHERE Codigo = @Codigo";
                }

                using (var comando = new SqliteCommand(query, conexion))
                {
                    // Parámetros antiguos
                    comando.Parameters.AddWithValue("@Codigo", txtCodigo.Text);
                    comando.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                    comando.Parameters.AddWithValue("@Presentacion", txtPresentacion.Text);
                    comando.Parameters.AddWithValue("@IdEstante", cmbEstante.SelectedValue);

                    // NUEVOS PARÁMETROS
                    comando.Parameters.AddWithValue("@Concentracion", txtConcentracion.Text);
                    comando.Parameters.AddWithValue("@MaximaCantidad", numMaximaCantidad.Value);
                    comando.Parameters.AddWithValue("@PideMasVencera", txtPideMasVencera.Text);
                    comando.Parameters.AddWithValue("@MinimaCantidad", numMinimaCantidad.Value);

                    try
                    {
                        comando.ExecuteNonQuery(); // Ejecuta el guardado o actualización
                        MessageBox.Show("Artículo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Le decimos a la ventana principal que todo salió bien para que recargue las listas
                        this.DialogResult = DialogResult.OK;
                        this.Close(); // Cierra esta ventanita automáticamente
                    }
                    catch (SqliteException ex)
                    {
                        // Si el código ya existe al intentar crear uno nuevo
                        if (ex.SqliteErrorCode == 19)
                        {
                            MessageBox.Show("Ya existe un artículo con ese código", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Error al guardar: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void CargarDatosArticulo()
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                // Agregamos los nuevos campos al SELECT
                string query = "SELECT * FROM Articulos WHERE Codigo = @codigo";
                using (var comando = new SqliteCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@codigo", CodigoParaEditar);
                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtCodigo.Text = reader["Codigo"].ToString();
                            txtNombre.Text = reader["Nombre"].ToString();
                            txtPresentacion.Text = reader["Presentacion"].ToString();
                            cmbEstante.SelectedValue = reader["IdEstante"];

                            // Cargamos los nuevos campos a la interfaz
                            txtConcentracion.Text = reader["Concentracion"] != DBNull.Value ? reader["Concentracion"].ToString() : "";
                            txtPideMasVencera.Text = reader["PideMasVencera"] != DBNull.Value ? reader["PideMasVencera"].ToString() : "";

                            if (reader["MaximaCantidad"] != DBNull.Value)
                                numMaximaCantidad.Value = Convert.ToDecimal(reader["MaximaCantidad"]);

                            if (reader["MinimaCantidad"] != DBNull.Value)
                                numMinimaCantidad.Value = Convert.ToDecimal(reader["MinimaCantidad"]);
                        }
                    }
                }
            }
        }
    }
}