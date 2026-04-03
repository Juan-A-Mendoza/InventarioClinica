using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ClosedXML.Excel;
using System.IO;

namespace InventarioClinica
{
    public partial class Form1 : Form
    {
        private string cadenaConexion = "Data Source=InventarioClinica.db";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarEstantes();
        }
        
        private void CargarEstantes()
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                // Pedimos el ID (para uso interno) y el Nombre (para mostrarlo)
                string query = "SELECT Id, Nombre FROM Estantes";

                using (var comando = new SqliteCommand(query, conexion))
                {
                    using (var reader = comando.ExecuteReader())
                    {
                        // Creamos una tabla virtual en memoria para guardar los resultados
                        DataTable dtEstantes = new DataTable();
                        dtEstantes.Load(reader);

                        // Conectamos esa tabla a tu ComboBox
                        cmbEstantes.DataSource = dtEstantes;

                        // "DisplayMember" es lo que el usuario lee (Ej: "Estante 1")
                        cmbEstantes.DisplayMember = "Nombre";

                        // "ValueMember" es el dato oculto que usa el programa (Ej: ID 1)
                        cmbEstantes.ValueMember = "Id";
                    }
                }
            }
        }

        private void cmbEstantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Verificamos que realmente haya un estante seleccionado
            if (cmbEstantes.SelectedValue != null && cmbEstantes.SelectedValue is long idEstante)
            {
                CargarArticulosPorEstante(idEstante);
            }
        }

        private void CargarArticulosPorEstante(long idEstante)
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Codigo, Nombre FROM Articulos WHERE IdEstante = @IdEstante";

                using (var comando = new SqliteCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@IdEstante", idEstante);

                    using (var reader = comando.ExecuteReader())
                    {
                        DataTable dtArticulos = new DataTable();
                        dtArticulos.Load(reader);

                        // Verificamos si este estante tiene artículos guardados
                        if (dtArticulos.Rows.Count > 0)
                        {
                            // Sí hay artículos: Cargamos el ComboBox normalmente
                            cmbArticulos.DataSource = dtArticulos;
                            cmbArticulos.DisplayMember = "Nombre";
                            cmbArticulos.ValueMember = "Codigo";
                        }
                        else
                        {
                            // El estante está vacío: Limpiamos absolutamente todo
                            cmbArticulos.DataSource = null; // Desconectamos los datos viejos
                            cmbArticulos.Items.Clear();     // Vaciamos la lista
                            cmbArticulos.Text = "";         // Borramos el texto visual fantasma

                            // También limpiamos los controles dependientes para evitar errores
                            txtCodigoSeleccionado.Text = "";
                            dgvKardex.Rows.Clear();
                        }
                    }
                }
            }
        }

        private void btnNuevoArticulo_Click(object sender, EventArgs e)
        {
            FrmNuevoArticulo frm = new FrmNuevoArticulo();
            frm.ShowDialog(); // ShowDialog hace que no puedas tocar el Kardex hasta que cierres esta ventanita
    
            // Aquí (luego de cerrar la ventanita) podríamos forzar a que el ComboBox de artículos se recargue.

        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            // 1. Pequeñas validaciones de seguridad
            if (cmbArticulos.SelectedValue == null)
            {
                MessageBox.Show("Por favor, selecciona un artículo primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numCantidad.Value <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!rbEntrada.Checked && !rbSalida.Checked)
            {
                MessageBox.Show("Selecciona si es una Entrada o una Salida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string codigoArticulo = cmbArticulos.SelectedValue.ToString();
            string fecha = dtpFecha.Value.ToString("dd/MM/yyyy");
            string documento = txtDocumento.Text;
            int cantidad = (int)numCantidad.Value;
            string observaciones = txtObservaciones.Text;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                // 2. Buscar cuántos artículos había la última vez
                int existenciaAnterior = 0;
                string queryStock = "SELECT Existencia FROM Movimientos WHERE CodigoArticulo = @Codigo ORDER BY Id DESC LIMIT 1";

                using (var comandoStock = new SqliteCommand(queryStock, conexion))
                {
                    comandoStock.Parameters.AddWithValue("@Codigo", codigoArticulo);
                    var resultado = comandoStock.ExecuteScalar();
                    if (resultado != null && resultado != DBNull.Value)
                    {
                        existenciaAnterior = Convert.ToInt32(resultado);
                    }
                }

                // 3. Calcular la nueva existencia (Matemática del Kardex)
                int nuevaExistencia = existenciaAnterior;
                if (rbEntrada.Checked) nuevaExistencia += cantidad;
                if (rbSalida.Checked) nuevaExistencia -= cantidad;

                // Evitar que queden números negativos en el inventario
                if (nuevaExistencia < 0)
                {
                    MessageBox.Show("Stock insuficiente. No puedes registrar esta salida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 4. Guardar el movimiento en la Base de Datos
                string queryInsert = @"INSERT INTO Movimientos
                             (CodigoArticulo, Fecha, Documento, Entrada, Salida, Existencia, Observaciones)
                             VALUES (@Codigo, @Fecha, @Documento, @Entrada, @Salida, @Existencia, @Observaciones)";

                using (var comandoInsert = new SqliteCommand(queryInsert, conexion))
                {
                    comandoInsert.Parameters.AddWithValue("@Codigo", codigoArticulo);
                    comandoInsert.Parameters.AddWithValue("@Fecha", fecha);
                    comandoInsert.Parameters.AddWithValue("@Documento", string.IsNullOrWhiteSpace(documento) ? (object)DBNull.Value : documento);

                    // Si es entrada, guardamos el número. Si no, guardamos "Nulo" para que luego se dibuje como un guion (-)
                    comandoInsert.Parameters.AddWithValue("@Entrada", rbEntrada.Checked ? cantidad : (object)DBNull.Value);
                    comandoInsert.Parameters.AddWithValue("@Salida", rbSalida.Checked ? cantidad : (object)DBNull.Value);
                    comandoInsert.Parameters.AddWithValue("@Existencia", nuevaExistencia);
                    comandoInsert.Parameters.AddWithValue("@Observaciones", string.IsNullOrWhiteSpace(observaciones) ? (object)DBNull.Value : observaciones);

                    comandoInsert.ExecuteNonQuery();
                }
            }

            // 5. Limpiar las cajas de texto para el próximo registro
            numCantidad.Value = 0;
            txtDocumento.Clear();
            txtObservaciones.Clear();
            rbEntrada.Checked = false;
            rbSalida.Checked = false;

            // 6. Actualizar la tabla visual
            ActualizarTablaKardex(codigoArticulo);
        }

        private void ActualizarTablaKardex(string codigoArticulo)
        {
            // Limpiamos las filas anteriores de la tabla
            dgvKardex.Rows.Clear();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                // Unimos las tablas Movimientos y Articulos para tener todos los datos completos
                string query = @"
            SELECT 
                m.Fecha, 
                a.Nombre, 
                m.CodigoArticulo, 
                a.Presentacion, 
                m.Documento, 
                m.Entrada, 
                m.Salida, 
                m.Existencia, 
                m.Observaciones 
            FROM Movimientos m
            INNER JOIN Articulos a ON m.CodigoArticulo = a.Codigo
            WHERE m.CodigoArticulo = @Codigo
            ORDER BY m.Id ASC"; // ASC ordena del más antiguo al más nuevo

                using (var comando = new SqliteCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@Codigo", codigoArticulo);
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Si el dato es nulo en la BD, lo convertimos a un guion o lo dejamos en blanco
                            string entradaFormato = reader["Entrada"] != DBNull.Value ? reader["Entrada"].ToString() : "-";
                            string salidaFormato = reader["Salida"] != DBNull.Value ? reader["Salida"].ToString() : "-";
                            string observacionesFormato = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : "";
                            string presentacionFormato = reader["Presentacion"] != DBNull.Value ? reader["Presentacion"].ToString() : "";
                            string documentoFormato = reader["Documento"] != DBNull.Value ? reader["Documento"].ToString() : "";

                            // Agregamos la fila a tu DataGridView exactamente en el orden de tus columnas
                            dgvKardex.Rows.Add(
                                reader["Fecha"].ToString(),
                                reader["Nombre"].ToString(),
                                reader["CodigoArticulo"].ToString(),
                                presentacionFormato,
                                documentoFormato,
                                entradaFormato,
                                salidaFormato,
                                reader["Existencia"].ToString(),
                                observacionesFormato
                            );
                        }
                    }
                }
            }
        }

        private void cmbArticulos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbArticulos.SelectedValue != null && cmbArticulos.SelectedValue is string codigo)
            {
                txtCodigoSeleccionado.Text = codigo; // <--- AGREGAMOS ESTA LÍNEA
                ActualizarTablaKardex(codigo);
            }
            // Controlamos el caso en el que la primera carga viene como DataRowView:
            else if (cmbArticulos.SelectedValue != null && cmbArticulos.SelectedValue is System.Data.DataRowView drv)
            {
                string codigoDrv = drv["Codigo"].ToString();
                txtCodigoSeleccionado.Text = codigoDrv; // <--- AGREGAMOS ESTA LÍNEA
                ActualizarTablaKardex(codigoDrv);
            }
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            // 1. Verificamos que haya un estante seleccionado
            if (cmbEstantes.SelectedValue == null || !(cmbEstantes.SelectedValue is long idEstante))
            {
                MessageBox.Show("Por favor, selecciona un estante primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombreEstante = cmbEstantes.Text;

            // 2. Le preguntamos al usuario dónde quiere guardar el archivo
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Archivo de Excel|*.xlsx";
            dialog.Title = "Guardar inventario de " + nombreEstante;
            dialog.FileName = nombreEstante + ".xlsx"; // Nombre por defecto (Ej: "Estante 1.xlsx")

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        using (var conexion = new SqliteConnection(cadenaConexion))
                        {
                            conexion.Open();

                            // 3. Buscamos TODOS los artículos de este estante
                            string queryArticulos = "SELECT Codigo, Nombre, Presentacion FROM Articulos WHERE IdEstante = @IdEstante";
                            using (var cmdArticulos = new SqliteCommand(queryArticulos, conexion))
                            {
                                cmdArticulos.Parameters.AddWithValue("@IdEstante", idEstante);
                                using (var readerArticulos = cmdArticulos.ExecuteReader())
                                {
                                    bool tieneArticulos = false;

                                    while (readerArticulos.Read())
                                    {
                                        tieneArticulos = true;
                                        string codigo = readerArticulos["Codigo"].ToString();
                                        string nombreArticulo = readerArticulos["Nombre"].ToString();
                                        string presentacion = readerArticulos["Presentacion"].ToString();

                                        // 4. Creamos una hoja por cada artículo (El nombre de la hoja será el nombre del medicamento)
                                        // Nota: Excel no permite nombres de hojas de más de 31 caracteres ni ciertos símbolos.
                                        string nombreHoja = nombreArticulo.Length > 30 ? nombreArticulo.Substring(0, 30) : nombreArticulo;
                                        var worksheet = workbook.Worksheets.Add(nombreHoja);

                                        // 5. Ponemos los encabezados igual a tu imagen
                                        worksheet.Cell(1, 1).Value = "Fecha";
                                        worksheet.Cell(1, 2).Value = "Nombre";
                                        worksheet.Cell(1, 3).Value = "Codigo";
                                        worksheet.Cell(1, 4).Value = "Presentacion";
                                        worksheet.Cell(1, 5).Value = "Documento";
                                        worksheet.Cell(1, 6).Value = "Entrada";
                                        worksheet.Cell(1, 7).Value = "Salida";
                                        worksheet.Cell(1, 8).Value = "Existencia";
                                        worksheet.Cell(1, 9).Value = "Observaciones";

                                        // Le damos un poco de formato a los encabezados (Negrita)
                                        worksheet.Range("A1:I1").Style.Font.Bold = true;

                                        // 6. Buscamos los movimientos de ESTE artículo
                                        string queryMov = "SELECT Fecha, Documento, Entrada, Salida, Existencia, Observaciones FROM Movimientos WHERE CodigoArticulo = @Codigo ORDER BY Id ASC";
                                        using (var cmdMov = new SqliteCommand(queryMov, conexion))
                                        {
                                            cmdMov.Parameters.AddWithValue("@Codigo", codigo);
                                            using (var readerMov = cmdMov.ExecuteReader())
                                            {
                                                int fila = 2; // Empezamos en la fila 2 porque la 1 tiene los encabezados
                                                while (readerMov.Read())
                                                {
                                                    worksheet.Cell(fila, 1).Value = readerMov["Fecha"].ToString();
                                                    worksheet.Cell(fila, 2).Value = nombreArticulo;
                                                    worksheet.Cell(fila, 3).Value = "'" + codigo; // El apóstrofe evita que Excel borre los ceros a la izquierda
                                                    worksheet.Cell(fila, 4).Value = presentacion;
                                                    worksheet.Cell(fila, 5).Value = readerMov["Documento"].ToString();

                                                    // La magia de los guiones para entradas y salidas
                                                    worksheet.Cell(fila, 6).Value = readerMov["Entrada"] != DBNull.Value ? readerMov["Entrada"].ToString() : "-";
                                                    worksheet.Cell(fila, 7).Value = readerMov["Salida"] != DBNull.Value ? readerMov["Salida"].ToString() : "-";

                                                    worksheet.Cell(fila, 8).Value = readerMov["Existencia"].ToString();
                                                    worksheet.Cell(fila, 9).Value = readerMov["Observaciones"].ToString();

                                                    // Centramos las columnas de entrada, salida y existencia
                                                    worksheet.Cell(fila, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    worksheet.Cell(fila, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    worksheet.Cell(fila, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                                    fila++;
                                                }
                                            }
                                        }

                                        // Ajustamos el ancho de las columnas automáticamente para que se vea bien
                                        worksheet.Columns().AdjustToContents();
                                    }

                                    // Si el estante estaba vacío, creamos una hoja en blanco para que Excel no dé error
                                    if (!tieneArticulos)
                                    {
                                        workbook.Worksheets.Add("Sin Artículos");
                                    }
                                }
                            }
                        }

                        // 7. Guardamos el archivo físicamente en la PC
                        workbook.SaveAs(dialog.FileName);
                        MessageBox.Show("Archivo Excel exportado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar a Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportarExcel_Click(object sender, EventArgs e)
        {
            // 1. Asegurarnos de que el usuario haya seleccionado a qué estante van estos datos
            if (cmbEstantes.SelectedValue == null || !(cmbEstantes.SelectedValue is long idEstante))
            {
                MessageBox.Show("Por favor, selecciona el Estante al que deseas importar los datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Abrir la ventana para buscar el archivo .xlsx
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Archivos de Excel|*.xlsx";
            dialog.Title = "Seleccionar archivo de inventario para importar";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Leemos el Excel seleccionado
                    using (var workbook = new XLWorkbook(dialog.FileName))
                    {
                        using (var conexion = new SqliteConnection(cadenaConexion))
                        {
                            conexion.Open();

                            // Iniciamos una transacción de seguridad
                            using (var transaccion = conexion.BeginTransaction())
                            {
                                // Recorremos cada pestaña (hoja) del Excel
                                foreach (var worksheet in workbook.Worksheets)
                                {
                                    // Ignoramos hojas en blanco o de control
                                    if (worksheet.Name == "Sin Artículos") continue;

                                    // La fila 1 tiene los encabezados, la fila 2 tiene el primer dato real
                                    int fila = 2;

                                    // Si la celda A2 está vacía, la hoja no tiene datos, saltamos a la siguiente
                                    if (worksheet.Cell(fila, 1).IsEmpty()) continue;

                                    // Capturamos los datos básicos del artículo leyendo la fila 2
                                    string nombreArticulo = worksheet.Cell(fila, 2).GetString().Trim();
                                    string codigoArticulo = worksheet.Cell(fila, 3).GetString().Replace("'", "").Trim();
                                    string presentacion = worksheet.Cell(fila, 4).GetString().Trim();

                                    if (string.IsNullOrEmpty(codigoArticulo)) continue;

                                    // 3. Verificar si el artículo ya existe en la Base de Datos
                                    string queryVerificar = "SELECT COUNT(*) FROM Articulos WHERE Codigo = @Codigo";
                                    using (var cmdVerificar = new SqliteCommand(queryVerificar, conexion, transaccion))
                                    {
                                        cmdVerificar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                        long existe = (long)cmdVerificar.ExecuteScalar();

                                        // Si no existe (es un artículo nuevo), lo registramos automáticamente
                                        if (existe == 0)
                                        {
                                            string queryInsertarArticulo = "INSERT INTO Articulos (Codigo, Nombre, Presentacion, IdEstante) VALUES (@Codigo, @Nombre, @Presentacion, @IdEstante)";
                                            using (var cmdInsertar = new SqliteCommand(queryInsertarArticulo, conexion, transaccion))
                                            {
                                                cmdInsertar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                                // Si por alguna razón el nombre está vacío en la celda, usamos el nombre de la pestaña
                                                cmdInsertar.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(nombreArticulo) ? worksheet.Name : nombreArticulo);
                                                cmdInsertar.Parameters.AddWithValue("@Presentacion", presentacion);
                                                cmdInsertar.Parameters.AddWithValue("@IdEstante", idEstante);
                                                cmdInsertar.ExecuteNonQuery();
                                            }
                                        }
                                        else
                                        {
                                            // Si el artículo YA EXISTE, lo mudamos al nuevo estante que el usuario seleccionó
                                            string queryMudarArticulo = "UPDATE Articulos SET IdEstante = @IdEstante WHERE Codigo = @Codigo";
                                            using (var cmdMudar = new SqliteCommand(queryMudarArticulo, conexion, transaccion))
                                            {
                                                cmdMudar.Parameters.AddWithValue("@IdEstante", idEstante);
                                                cmdMudar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                                cmdMudar.ExecuteNonQuery();
                                            }
                                        }
                                      
                                    }
                                    // Limpiamos el historial viejo de este artículo específico para no duplicar datos
                                    string queryLimpiarKardex = "DELETE FROM Movimientos WHERE CodigoArticulo = @Codigo";
                                    using (var cmdLimpiar = new SqliteCommand(queryLimpiarKardex, conexion, transaccion))
                                    {
                                        cmdLimpiar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                        cmdLimpiar.ExecuteNonQuery();
                                    }

                                    // 4. Leer todo el historial de movimientos (Kardex) hacia abajo
                                    while (!worksheet.Cell(fila, 1).IsEmpty())
                                    {
                                        string fechaCruda = worksheet.Cell(fila, 1).GetString();
                                        string fecha = fechaCruda;

                                        if (DateTime.TryParse(fechaCruda, out DateTime fechaConvertida))
                                        {
                                            //Se intenta limpiar la fecha para que no agregue la hora como lo hace por defecto excel
                                            fecha = fechaConvertida.ToString("dd/MM/yyyy");
                                        }
                                        else if (fechaCruda.Length > 10)
                                        {  
                                            //si la conversion falla pero el texto es largo entonces se corta a la fuerza
                                            fecha = fechaCruda.Substring(0, 10).Trim();
                                        }
                                        string documento = worksheet.Cell(fila, 5).GetString();
                                        string entradaStr = worksheet.Cell(fila, 6).GetString();
                                        string salidaStr = worksheet.Cell(fila, 7).GetString();
                                        string existenciaStr = worksheet.Cell(fila, 8).GetString();
                                        string observaciones = worksheet.Cell(fila, 9).GetString();

                                        // Convertimos los guiones del Excel (-) o espacios vacíos a valores nulos para la BD
                                        object entrada = (entradaStr == "-" || string.IsNullOrWhiteSpace(entradaStr)) ? (object)DBNull.Value : Convert.ToInt32(entradaStr);
                                        object salida = (salidaStr == "-" || string.IsNullOrWhiteSpace(salidaStr)) ? (object)DBNull.Value : Convert.ToInt32(salidaStr);
                                        int existencia = string.IsNullOrWhiteSpace(existenciaStr) ? 0 : Convert.ToInt32(existenciaStr);

                                        // Guardamos el movimiento en el historial
                                        string queryInsertarMov = @"INSERT INTO Movimientos 
                                                            (CodigoArticulo, Fecha, Documento, Entrada, Salida, Existencia, Observaciones) 
                                                            VALUES (@Codigo, @Fecha, @Documento, @Entrada, @Salida, @Existencia, @Observaciones)";

                                        using (var cmdMov = new SqliteCommand(queryInsertarMov, conexion, transaccion))
                                        {
                                            cmdMov.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                            cmdMov.Parameters.AddWithValue("@Fecha", fecha);
                                            cmdMov.Parameters.AddWithValue("@Documento", string.IsNullOrWhiteSpace(documento) ? (object)DBNull.Value : documento);
                                            cmdMov.Parameters.AddWithValue("@Entrada", entrada);
                                            cmdMov.Parameters.AddWithValue("@Salida", salida);
                                            cmdMov.Parameters.AddWithValue("@Existencia", existencia);
                                            cmdMov.Parameters.AddWithValue("@Observaciones", string.IsNullOrWhiteSpace(observaciones) ? (object)DBNull.Value : observaciones);
                                            cmdMov.ExecuteNonQuery();
                                        }
                                        fila++; // Pasamos a la siguiente fila del Excel
                                    }
                                }
                                // Si todo el Excel se leyó perfecto, confirmamos el guardado definitivo
                                transaccion.Commit();
                            }
                        }

                        MessageBox.Show("Datos importados correctamente. El inventario ha sido actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refrescamos el ComboBox para que aparezcan los medicamentos recién importados
                        CargarArticulosPorEstante(idEstante);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al leer el archivo Excel. Verifica que el formato sea correcto.\nDetalle: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBuscarFecha_Click(object sender, EventArgs e)
        {

        }
    }
}
