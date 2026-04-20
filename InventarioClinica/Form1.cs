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
            dtpBuscarDesde.Value = DateTime.Now;
            dtpBuscarHasta.Value = DateTime.Now;
            dtpFecha.Value = DateTime.Now;
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
                string query = "SELECT Codigo, Nombre, Presentacion FROM Articulos WHERE IdEstante = @IdEstante";

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
                            txtPresentacionSeleccionada.Text = "";
                            dgvKardex.Rows.Clear();
                        }
                    }
                }
            }
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
            if (cmbArticulos.SelectedValue != null)
            {
                DataRowView drv = cmbArticulos.SelectedItem as DataRowView;

                if(drv != null)
                {
                    string codigo = drv["Codigo"].ToString();
                    string presentacion = drv["Presentacion"].ToString();

                    txtCodigoSeleccionado.Text = codigo;
                    txtPresentacionSeleccionada.Text = presentacion;

                    ActualizarTablaKardex(codigo);
                }
            }
            
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            if (cmbEstantes.SelectedValue == null || !(cmbEstantes.SelectedValue is long idEstante)) return;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Archivo de Excel|*.xlsx";
            dialog.FileName = cmbEstantes.Text + "_Kardex.xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        using (var conexion = new SqliteConnection(cadenaConexion))
                        {
                            conexion.Open();
                            string queryArticulos = "SELECT Codigo, Nombre, Presentacion, Concentracion, MaximaCantidad, PideMasVencera, MinimaCantidad FROM Articulos WHERE IdEstante = @IdEstante";
                            using (var cmdArticulos = new SqliteCommand(queryArticulos, conexion))
                            {
                                cmdArticulos.Parameters.AddWithValue("@IdEstante", idEstante);
                                using (var readerArticulos = cmdArticulos.ExecuteReader())
                                {
                                    int contadorArticulos = 0;
                                    int hojaIndex = 1;
                                    IXLWorksheet ws = null;
                                    bool tieneArticulos = false;

                                    while (readerArticulos.Read())
                                    {
                                        tieneArticulos = true;

                                        // Si es múltiplo de 3 (0, 3, 6...), creamos una hoja nueva
                                        if (contadorArticulos % 3 == 0)
                                        {
                                            ws = workbook.Worksheets.Add("Kardex_Pag" + hojaIndex);
                                            hojaIndex++;

                                            // ---> NUEVA CONFIGURACIÓN DE IMPRESIÓN <---
                                            // 1. Orientación Horizontal
                                            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                                            // 2. Tamaño Carta estándar
                                            ws.PageSetup.PaperSize = XLPaperSize.LetterPaper;

                                            // 3. Márgenes estrechos para aprovechar al máximo la hoja
                                            ws.PageSetup.Margins.SetTop(0.5);
                                            ws.PageSetup.Margins.SetBottom(0.5);
                                            ws.PageSetup.Margins.SetLeft(0.25);
                                            ws.PageSetup.Margins.SetRight(0.25);

                                            // 4. ¡LA MAGIA! Fuerza a Excel a encoger todo para que quepa en 1 hoja de ancho por 1 de alto
                                            ws.PageSetup.FitToPages(1, 1);
                                            // -------------------------
                                        }

                                        // MATEMÁTICA: Calculamos dónde empieza la columna (1, 8 o 15)
                                        int c = (contadorArticulos % 3) * 7 + 1;

                                        string codigo = readerArticulos["Codigo"].ToString();
                                        string nombreArticulo = readerArticulos["Nombre"].ToString();

                                        // --- ENCABEZADO ---
                                        string rutaLogo = "logo.jpeg";
                                        if (System.IO.File.Exists(rutaLogo))
                                        {
                                            ws.AddPicture(rutaLogo).MoveTo(ws.Cell(1, c)).WithSize(60, 60);
                                        }

                                        ws.Cell(1, c + 1).Value = "HOSPITAL ADVENTISTA DE VENEZUELA";
                                        ws.Cell(2, c + 1).Value = "RIF J-08517758-2";
                                        ws.Cell(3, c + 1).Value = "CARDEX";
                                        ws.Range(1, c + 1, 3, c + 3).Style.Font.Bold = true;
                                        ws.Range(1, c + 1, 3, c + 3).Style.Font.FontSize = 10;

                                        // Tu ajuste en la Fila 3 (Equivalente a F3 para la primera tarjeta)
                                        ws.Cell(3, c + 4).Value = "Código:";
                                        ws.Cell(3, c + 4).Style.Font.Bold = true;
                                        ws.Cell(3, c + 5).Value = "'" + codigo;
                                        ws.Cell(3, c + 5).Style.Font.FontSize = 14;
                                        ws.Cell(3, c + 5).Style.Font.Bold = true;

                                        // --- BLOQUE DE DATOS ---
                                        ws.Cell(4, c).Value = "NOMBRE DEL MEDICAMENTO";
                                        ws.Cell(5, c).Value = "CONCENTRACIÓN";
                                        ws.Cell(6, c).Value = "PRESENTACIÓN (JARABE, AMPOLLAS, TABLETAS)";
                                        ws.Cell(7, c).Value = "MÁXIMA CANTIDAD A PEDIR";
                                        ws.Cell(8, c).Value = "SI PIDE MÁS SE VENCERÁ";
                                        ws.Cell(9, c).Value = "MÍNIMA CANTIDAD A TENER";

                                        ws.Cell(4, c + 3).Value = nombreArticulo;
                                        ws.Cell(5, c + 3).Value = readerArticulos["Concentracion"].ToString();
                                        ws.Cell(6, c + 3).Value = readerArticulos["Presentacion"].ToString();
                                        ws.Cell(7, c + 3).Value = readerArticulos["MaximaCantidad"].ToString();
                                        ws.Cell(8, c + 3).Value = readerArticulos["PideMasVencera"].ToString();
                                        ws.Cell(9, c + 3).Value = readerArticulos["MinimaCantidad"].ToString();

                                        var bloqueDatos = ws.Range(4, c, 9, c + 5);
                                        bloqueDatos.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                                        bloqueDatos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                        ws.Range(4, c, 9, c).Style.Font.FontSize = 9;

                                        for (int i = 4; i <= 9; i++)
                                        {
                                            ws.Range(i, c, i, c + 2).Merge();
                                            ws.Range(i, c + 3, i, c + 5).Merge();
                                        }

                                        // --- TABLA DE MOVIMIENTOS ---
                                        int filaHeader = 11;
                                        ws.Cell(filaHeader, c).Value = "FECHA";
                                        ws.Cell(filaHeader, c + 1).Value = "DOCUMENTO";
                                        ws.Cell(filaHeader, c + 2).Value = "ENTRADA";
                                        ws.Cell(filaHeader, c + 3).Value = "SALIDA";
                                        ws.Cell(filaHeader, c + 4).Value = "EXISTENCIA";
                                        ws.Cell(filaHeader, c + 5).Value = "OBSERVACIONES";

                                        var rangoHeaders = ws.Range(filaHeader, c, filaHeader, c + 5);
                                        rangoHeaders.Style.Font.Bold = true;
                                        rangoHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        rangoHeaders.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                                        rangoHeaders.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                                        string queryMov = "SELECT Fecha, Documento, Entrada, Salida, Existencia, Observaciones FROM Movimientos WHERE CodigoArticulo = @Codigo ORDER BY Id ASC";
                                        using (var cmdMov = new SqliteCommand(queryMov, conexion))
                                        {
                                            cmdMov.Parameters.AddWithValue("@Codigo", codigo);
                                            using (var readerMov = cmdMov.ExecuteReader())
                                            {
                                                int fila = 12;
                                                while (readerMov.Read())
                                                {
                                                    ws.Cell(fila, c).Value = readerMov["Fecha"].ToString();
                                                    ws.Cell(fila, c + 1).Value = readerMov["Documento"].ToString();
                                                    ws.Cell(fila, c + 2).Value = readerMov["Entrada"] != DBNull.Value ? readerMov["Entrada"].ToString() : "-";
                                                    ws.Cell(fila, c + 3).Value = readerMov["Salida"] != DBNull.Value ? readerMov["Salida"].ToString() : "-";
                                                    ws.Cell(fila, c + 4).Value = readerMov["Existencia"].ToString();
                                                    ws.Cell(fila, c + 5).Value = readerMov["Observaciones"].ToString();

                                                    ws.Range(fila, c + 2, fila, c + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    fila++;
                                                }

                                                if (fila > 12)
                                                {
                                                    var tablaMovimientos = ws.Range(11, c, fila - 1, c + 5);
                                                    tablaMovimientos.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                                                    tablaMovimientos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                                }
                                            }
                                        }

                                        // Ajustamos anchos de esta tarjeta específica
                                        ws.Column(c).Width = 12;
                                        ws.Column(c + 1).Width = 15;
                                        ws.Column(c + 2).Width = 10;
                                        ws.Column(c + 3).Width = 10;
                                        ws.Column(c + 4).Width = 12;
                                        ws.Column(c + 5).Width = 25;

                                        contadorArticulos++;
                                    }

                                    if (!tieneArticulos) workbook.Worksheets.Add("Sin Artículos");
                                }
                            }
                        }
                        workbook.SaveAs(dialog.FileName);
                        MessageBox.Show("Archivo Excel exportado con 3 tarjetas por hoja.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnImportarExcel_Click(object sender, EventArgs e)
        {
            if (cmbEstantes.SelectedValue == null || !(cmbEstantes.SelectedValue is long idEstante)) return;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Archivos de Excel|*.xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook(dialog.FileName))
                    {
                        using (var conexion = new SqliteConnection(cadenaConexion))
                        {
                            conexion.Open();
                            using (var transaccion = conexion.BeginTransaction())
                            {
                                foreach (var worksheet in workbook.Worksheets)
                                {
                                    if (worksheet.Name == "Sin Artículos") continue;

                                    // Hacemos un ciclo para revisar las 3 posiciones posibles de tarjetas en esta hoja
                                    for (int i = 0; i < 3; i++)
                                    {
                                        int c = (i * 7) + 1; // Genera columnas: 1, 8 y 15

                                        // Buscamos el código en la fila 3, columna c+5 (Equivalente a F3, M3, T3)
                                        string codigoArticulo = worksheet.Cell(3, c + 5).GetString().Replace("'", "").Trim();

                                        // Si en este espacio no hay código, significa que no hay tarjeta. Pasamos al siguiente.
                                        if (string.IsNullOrEmpty(codigoArticulo)) continue;

                                        string nombreArticulo = worksheet.Cell(4, c + 3).GetString().Trim();
                                        string concentracion = worksheet.Cell(5, c + 3).GetString().Trim();
                                        string presentacion = worksheet.Cell(6, c + 3).GetString().Trim();

                                        int.TryParse(worksheet.Cell(7, c + 3).GetString(), out int maximaCantidad);
                                        string pideMasVencera = worksheet.Cell(8, c + 3).GetString().Trim();
                                        int.TryParse(worksheet.Cell(9, c + 3).GetString(), out int minimaCantidad);

                                        // --- ACTUALIZAR O CREAR ARTÍCULO ---
                                        string queryVerificar = "SELECT COUNT(*) FROM Articulos WHERE Codigo = @Codigo";
                                        using (var cmdVerificar = new SqliteCommand(queryVerificar, conexion, transaccion))
                                        {
                                            cmdVerificar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                            if ((long)cmdVerificar.ExecuteScalar() == 0)
                                            {
                                                string queryInsertar = "INSERT INTO Articulos (Codigo, Nombre, Presentacion, Concentracion, MaximaCantidad, PideMasVencera, MinimaCantidad, IdEstante) VALUES (@Codigo, @Nombre, @Presentacion, @Concentracion, @MaximaCantidad, @PideMasVencera, @MinimaCantidad, @IdEstante)";
                                                using (var cmd = new SqliteCommand(queryInsertar, conexion, transaccion))
                                                {
                                                    cmd.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(nombreArticulo) ? "Articulo" : nombreArticulo);
                                                    cmd.Parameters.AddWithValue("@Presentacion", presentacion);
                                                    cmd.Parameters.AddWithValue("@Concentracion", concentracion);
                                                    cmd.Parameters.AddWithValue("@MaximaCantidad", maximaCantidad);
                                                    cmd.Parameters.AddWithValue("@PideMasVencera", pideMasVencera);
                                                    cmd.Parameters.AddWithValue("@MinimaCantidad", minimaCantidad);
                                                    cmd.Parameters.AddWithValue("@IdEstante", idEstante);
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                            else
                                            {
                                                string queryActualizar = "UPDATE Articulos SET Nombre = @Nombre, Presentacion = @Presentacion, Concentracion = @Concentracion, MaximaCantidad = @MaximaCantidad, PideMasVencera = @PideMasVencera, MinimaCantidad = @MinimaCantidad, IdEstante = @IdEstante WHERE Codigo = @Codigo";
                                                using (var cmd = new SqliteCommand(queryActualizar, conexion, transaccion))
                                                {
                                                    cmd.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                                    cmd.Parameters.AddWithValue("@Nombre", nombreArticulo);
                                                    cmd.Parameters.AddWithValue("@Presentacion", presentacion);
                                                    cmd.Parameters.AddWithValue("@Concentracion", concentracion);
                                                    cmd.Parameters.AddWithValue("@MaximaCantidad", maximaCantidad);
                                                    cmd.Parameters.AddWithValue("@PideMasVencera", pideMasVencera);
                                                    cmd.Parameters.AddWithValue("@MinimaCantidad", minimaCantidad);
                                                    cmd.Parameters.AddWithValue("@IdEstante", idEstante);
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                        }

                                        // --- LIMPIAR HISTORIAL ---
                                        using (var cmdLimpiar = new SqliteCommand("DELETE FROM Movimientos WHERE CodigoArticulo = @Codigo", conexion, transaccion))
                                        {
                                            cmdLimpiar.Parameters.AddWithValue("@Codigo", codigoArticulo);
                                            cmdLimpiar.ExecuteNonQuery();
                                        }

                                        // --- LEER MOVIMIENTOS ---
                                        int fila = 12;
                                        while (!worksheet.Cell(fila, c).IsEmpty())
                                        {
                                            string fechaCruda = worksheet.Cell(fila, c).GetString();
                                            string fecha = fechaCruda;
                                            if (DateTime.TryParse(fechaCruda, out DateTime fechaConvertida)) fecha = fechaConvertida.ToString("dd/MM/yyyy");
                                            else if (fechaCruda.Length > 10) fecha = fechaCruda.Substring(0, 10).Trim();

                                            string documento = worksheet.Cell(fila, c + 1).GetString();
                                            string entradaStr = worksheet.Cell(fila, c + 2).GetString();
                                            string salidaStr = worksheet.Cell(fila, c + 3).GetString();
                                            string existenciaStr = worksheet.Cell(fila, c + 4).GetString();
                                            string observaciones = worksheet.Cell(fila, c + 5).GetString();

                                            object entrada = (entradaStr == "-" || string.IsNullOrWhiteSpace(entradaStr)) ? (object)DBNull.Value : Convert.ToInt32(entradaStr);
                                            object salida = (salidaStr == "-" || string.IsNullOrWhiteSpace(salidaStr)) ? (object)DBNull.Value : Convert.ToInt32(salidaStr);
                                            int existencia = string.IsNullOrWhiteSpace(existenciaStr) ? 0 : Convert.ToInt32(existenciaStr);

                                            string queryInsertarMov = "INSERT INTO Movimientos (CodigoArticulo, Fecha, Documento, Entrada, Salida, Existencia, Observaciones) VALUES (@Codigo, @Fecha, @Documento, @Entrada, @Salida, @Existencia, @Observaciones)";
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
                                            fila++;
                                        }
                                    } // Fin del For (Las 3 tarjetas)
                                }
                                transaccion.Commit();
                            }
                        }

                        MessageBox.Show("Datos importados correctamente desde el formato panorámico.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        string codigoMantener = txtCodigoSeleccionado.Text;
                        CargarArticulosPorEstante(idEstante);

                        if (!string.IsNullOrEmpty(codigoMantener))
                        {
                            cmbArticulos.SelectedValue = codigoMantener;
                            ActualizarTablaKardex(codigoMantener);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al leer el archivo Excel: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBuscarFecha_Click(object sender, EventArgs e)
        {
            DateTime FechaDesde = dtpBuscarDesde.Value.Date;
            DateTime FechaHasta = dtpBuscarHasta.Value.Date;

            if (FechaDesde > FechaHasta)
            {
                MessageBox.Show("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvKardex.CurrentCell = null; // Desseleccionamos cualquier celda para evitar problemas al ocultar filas

           foreach (DataGridViewRow fila in dgvKardex.Rows)
            {
                if (fila.IsNewRow) continue;

                string fehaStr = fila.Cells[0].Value?.ToString() ??""; 

                if (DateTime.TryParseExact(fehaStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fechaFila))
                {
                    if (fechaFila >= FechaDesde && fechaFila <= FechaHasta)
                    {
                        fila.Visible = true;
                    }
                    else
                    {
                        fila.Visible = false;
                    }
                }
            }
        }

        private void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            dtpBuscarDesde.Value = DateTime.Now;
            dtpBuscarHasta.Value = DateTime.Now;

            dgvKardex.CurrentCell = null;

            foreach (DataGridViewRow fila in dgvKardex.Rows)
            {
                if (!fila.IsNewRow)
                {
                    fila.Visible = true;
                }
            }
        }

        private void agregarToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FrmGestionEstante frm = new FrmGestionEstante();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarEstantes();
            }
        }

        private void editarToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (cmbEstantes.SelectedValue == null)
            {
                MessageBox.Show("Selecciona primero en el desplegable el estante que deseas editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FrmGestionEstante frm = new FrmGestionEstante();
            frm.IdEstante = (long)cmbEstantes.SelectedValue;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarEstantes();
            }
        }

        private void eliminarToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (cmbEstantes.SelectedValue == null || !(cmbEstantes.SelectedValue is long idEstante)) return;

            DialogResult confirmacion = MessageBox.Show($"¿Estás seguro de que deseas eliminar el {cmbEstantes.Text}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion == DialogResult.Yes)
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var cmdCheck = new SqliteCommand("SELECT COUNT(*) FROM Articulos WHERE IdEstante = @IdEstante", conexion);
                    cmdCheck.Parameters.AddWithValue("@IdEstante", idEstante);

                    if ((long)cmdCheck.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("No puedes eliminar este estante porque tiene medicamentos adentro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var cmdDelete = new SqliteCommand("DELETE FROM Estantes WHERE Id = @IdEstante", conexion);
                    cmdDelete.Parameters.AddWithValue("@IdEstante", idEstante);
                    cmdDelete.ExecuteNonQuery();
                }
                MessageBox.Show("Estante eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarEstantes();
            }
        }

        private void agregarToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (cmbEstantes.SelectedValue == null) return;

            FrmNuevoArticulo frm = new FrmNuevoArticulo();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarArticulosPorEstante((long)cmbEstantes.SelectedValue);
            }
        }

        private void editarToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (cmbArticulos.SelectedValue == null || string.IsNullOrEmpty(txtCodigoSeleccionado.Text))
            {
                MessageBox.Show("Selecciona primero el artículo que deseas editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FrmNuevoArticulo frm = new FrmNuevoArticulo();
            // Le pasamos el código para que sepa que va a Editar y no a Crear
            frm.CodigoParaEditar = txtCodigoSeleccionado.Text;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarArticulosPorEstante((long)cmbEstantes.SelectedValue);
            }
        }

        private void eliminarToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (cmbArticulos.SelectedValue == null || string.IsNullOrEmpty(txtCodigoSeleccionado.Text)) return;

            string codigoArticulo = txtCodigoSeleccionado.Text;
            long idEstante = (long)cmbEstantes.SelectedValue;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                // 1. Revisamos cuántos movimientos tiene
                var cmdCheck = new SqliteCommand("SELECT COUNT(*) FROM Movimientos WHERE CodigoArticulo = @Codigo", conexion);
                cmdCheck.Parameters.AddWithValue("@Codigo", codigoArticulo);
                long cantidadMovimientos = (long)cmdCheck.ExecuteScalar();

                if (cantidadMovimientos > 0)
                {
                    // 2. ADVERTENCIA SEVERA (Nota que el botón por defecto es el "No" por seguridad)
                    DialogResult alertaMortal = MessageBox.Show(
                        $"¡CUIDADO!\n\nEl artículo '{cmbArticulos.Text}' tiene {cantidadMovimientos} movimientos en el Kardex.\n\nSi continúas, SE BORRARÁ TODO SU HISTORIAL para siempre. Esto solo debe usarse para corregir pruebas o errores graves.\n\n¿Estás COMPLETAMENTE SEGURO de querer forzar la eliminación?",
                        "Peligro de Pérdida de Datos",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2); // Button2 hace que el "No" esté seleccionado por defecto

                    if (alertaMortal != DialogResult.Yes) return; // Si dice que no, cancelamos todo

                    // 3. Borrado en cascada: Primero matamos el historial
                    var cmdDeleteMov = new SqliteCommand("DELETE FROM Movimientos WHERE CodigoArticulo = @Codigo", conexion);
                    cmdDeleteMov.Parameters.AddWithValue("@Codigo", codigoArticulo);
                    cmdDeleteMov.ExecuteNonQuery();
                }
                else
                {
                    // Confirmación suave si es un artículo limpio (sin movimientos)
                    DialogResult confirmacion = MessageBox.Show($"¿Deseas eliminar el artículo '{cmbArticulos.Text}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmacion != DialogResult.Yes) return;
                }

                // 4. Finalmente, eliminamos el artículo de la base de datos
                var cmdDeleteArt = new SqliteCommand("DELETE FROM Articulos WHERE Codigo = @Codigo", conexion);
                cmdDeleteArt.Parameters.AddWithValue("@Codigo", codigoArticulo);
                cmdDeleteArt.ExecuteNonQuery();
            }

            MessageBox.Show("Artículo eliminado correctamente del sistema.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarArticulosPorEstante(idEstante); // Recargamos la interfaz
        }

        private void descargarPlantillaExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Archivo de Excel|*.xlsx";
            dialog.Title = "Descargar Plantilla de Kardex Triple";
            dialog.FileName = "Plantilla_Kardex_Triple.xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Plantilla");

                        // --- CONFIGURACIÓN DE IMPRESIÓN ---
                        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                        ws.PageSetup.PaperSize = XLPaperSize.LetterPaper;
                        ws.PageSetup.Margins.SetTop(0.5);
                        ws.PageSetup.Margins.SetBottom(0.5);
                        ws.PageSetup.Margins.SetLeft(0.25);
                        ws.PageSetup.Margins.SetRight(0.25);
                        ws.PageSetup.FitToPages(1, 1);

                        // Dibujamos 3 tarjetas vacías (i = 0, 1, 2)
                        for (int i = 0; i < 3; i++)
                        {
                            int c = (i * 7) + 1; // Columnas: 1, 8 y 15

                            // 1. LOGO Y ENCABEZADO
                            string rutaLogo = "logo.jpeg";
                            if (System.IO.File.Exists(rutaLogo))
                            {
                                ws.AddPicture(rutaLogo).MoveTo(ws.Cell(1, c)).WithSize(60, 60);
                            }

                            ws.Cell(1, c + 1).Value = "HOSPITAL ADVENTISTA DE VENEZUELA";
                            ws.Cell(2, c + 1).Value = "RIF J-08517758-2";
                            ws.Cell(3, c + 1).Value = "CARDEX";
                            ws.Range(1, c + 1, 3, c + 3).Style.Font.Bold = true;
                            ws.Range(1, c + 1, 3, c + 3).Style.Font.FontSize = 10;

                            // Código en la Fila 3, Columna c+5 (F3, M3, T3)
                            ws.Cell(3, c + 4).Value = "Código:";
                            ws.Cell(3, c + 4).Style.Font.Bold = true;
                            ws.Cell(3, c + 5).Value = "[Código]";
                            ws.Cell(3, c + 5).Style.Font.FontColor = XLColor.Blue;
                            ws.Cell(3, c + 5).Style.Font.Bold = true;

                            // 2. BLOQUE DE DATOS
                            ws.Cell(4, c).Value = "NOMBRE DEL MEDICAMENTO";
                            ws.Cell(5, c).Value = "CONCENTRACIÓN";
                            ws.Cell(6, c).Value = "PRESENTACIÓN (JARABE, AMPOLLAS, TABLETAS)";
                            ws.Cell(7, c).Value = "MÁXIMA CANTIDAD A PEDIR";
                            ws.Cell(8, c).Value = "SI PIDE MÁS SE VENCERÁ";
                            ws.Cell(9, c).Value = "MÍNIMA CANTIDAD A TENER";

                            // Guías para el usuario
                            ws.Cell(4, c + 3).Value = "[Nombre]";
                            ws.Cell(5, c + 3).Value = "[Concentración]";
                            ws.Cell(6, c + 3).Value = "[Presentación]";
                            ws.Cell(7, c + 3).Value = "0";
                            ws.Cell(8, c + 3).Value = "[Vencimiento]";
                            ws.Cell(9, c + 3).Value = "0";
                            ws.Range(4, c + 3, 9, c + 3).Style.Font.Italic = true;
                            ws.Range(4, c + 3, 9, c + 3).Style.Font.FontColor = XLColor.Gray;

                            var bloqueDatos = ws.Range(4, c, 9, c + 5);
                            bloqueDatos.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            bloqueDatos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(4, c, 9, c).Style.Font.FontSize = 9;

                            for (int j = 4; j <= 9; j++)
                            {
                                ws.Range(j, c, j, c + 2).Merge();
                                ws.Range(j, c + 3, j, c + 5).Merge();
                            }

                            // 3. TABLA DE MOVIMIENTOS
                            int filaHeader = 11;
                            ws.Cell(filaHeader, c).Value = "FECHA";
                            ws.Cell(filaHeader, c + 1).Value = "DOCUMENTO";
                            ws.Cell(filaHeader, c + 2).Value = "ENTRADA";
                            ws.Cell(filaHeader, c + 3).Value = "SALIDA";
                            ws.Cell(filaHeader, c + 4).Value = "EXISTENCIA";
                            ws.Cell(filaHeader, c + 5).Value = "OBSERVACIONES";

                            var rangoHeaders = ws.Range(filaHeader, c, filaHeader, c + 5);
                            rangoHeaders.Style.Font.Bold = true;
                            rangoHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            rangoHeaders.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            rangoHeaders.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            // Dibujamos 15 filas vacías con guiones para que se vea como el físico
                            for (int f = 12; f <= 26; f++)
                            {
                                ws.Cell(f, c + 2).Value = "-";
                                ws.Cell(f, c + 3).Value = "-";
                                ws.Range(f, c + 2, f, c + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            var tablaMovimientos = ws.Range(11, c, 26, c + 5);
                            tablaMovimientos.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            tablaMovimientos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            // Ajustamos anchos
                            ws.Column(c).Width = 12;
                            ws.Column(c + 1).Width = 15;
                            ws.Column(c + 2).Width = 10;
                            ws.Column(c + 3).Width = 10;
                            ws.Column(c + 4).Width = 12;
                            ws.Column(c + 5).Width = 25;
                        }

                        workbook.SaveAs(dialog.FileName);
                        MessageBox.Show("Plantilla triple descargada con éxito.\nYa está configurada para imprimirse en una sola hoja horizontal.", "Ayuda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar la plantilla: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importarCargaMasivaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Archivos de Excel|*.xlsx";
            dialog.Title = "Seleccionar Carga Masiva Multi-Estante";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook(dialog.FileName))
                    {
                        using (var conexion = new SqliteConnection(cadenaConexion))
                        {
                            conexion.Open();
                            using (var transaccion = conexion.BeginTransaction())
                            {
                                int totalImportados = 0;
                                string hojasIgnoradas = "";

                                // El programa ahora revisa TODAS las hojas del Excel
                                foreach (var worksheet in workbook.Worksheets)
                                {
                                    string nombreHoja = worksheet.Name.Trim();

                                    // 1. Buscamos en la Base de Datos si existe un estante con el nombre de esta hoja
                                    string queryEstante = "SELECT Id FROM Estantes WHERE Nombre = @NombreHoja";
                                    long idEstanteDestino = -1;

                                    using (var cmdEstante = new SqliteCommand(queryEstante, conexion, transaccion))
                                    {
                                        cmdEstante.Parameters.AddWithValue("@NombreHoja", nombreHoja);
                                        object result = cmdEstante.ExecuteScalar(); // Devuelve el ID si lo encuentra

                                        if (result != null)
                                        {
                                            idEstanteDestino = (long)result;
                                        }
                                    }

                                    // 2. Si no encontró un estante que se llame igual, anota la hoja y la salta
                                    if (idEstanteDestino == -1)
                                    {
                                        hojasIgnoradas += $"• {nombreHoja}\n";
                                        continue;
                                    }

                                    // 3. Si el estante existe, procedemos a importar sus artículos
                                    // Empezamos en la FILA 2, porque la FILA 1 tiene los encabezados azules de la plantilla
                                    int fila = 2;

                                    while (!worksheet.Cell(fila, 1).IsEmpty() || !worksheet.Cell(fila, 2).IsEmpty())
                                    {
                                        string nombre = worksheet.Cell(fila, 1).GetString().Trim();
                                        string codigo = worksheet.Cell(fila, 2).GetString().Trim();
                                        string presentacion = worksheet.Cell(fila, 3).GetString().Trim();

                                        if (string.IsNullOrEmpty(codigo)) { fila++; continue; }

                                        // Verificamos si ya existe el código
                                        string queryExiste = "SELECT COUNT(*) FROM Articulos WHERE Codigo = @cod";
                                        using (var cmdCheck = new SqliteCommand(queryExiste, conexion, transaccion))
                                        {
                                            cmdCheck.Parameters.AddWithValue("@cod", codigo);
                                            long existe = (long)cmdCheck.ExecuteScalar();

                                            string queryFinal;
                                            if (existe == 0)
                                            {
                                                queryFinal = @"INSERT INTO Articulos 
                                            (Codigo, Nombre, Presentacion, IdEstante, Concentracion, MaximaCantidad, PideMasVencera, MinimaCantidad) 
                                            VALUES (@cod, @nom, @pre, @est, '', 0, '', 0)";
                                            }
                                            else
                                            {
                                                queryFinal = @"UPDATE Articulos 
                                            SET Nombre = @nom, Presentacion = @pre, IdEstante = @est 
                                            WHERE Codigo = @cod";
                                            }

                                            using (var cmdAccion = new SqliteCommand(queryFinal, conexion, transaccion))
                                            {
                                                cmdAccion.Parameters.AddWithValue("@cod", codigo);
                                                cmdAccion.Parameters.AddWithValue("@nom", string.IsNullOrEmpty(nombre) ? "Sin Nombre" : nombre);
                                                cmdAccion.Parameters.AddWithValue("@pre", presentacion);
                                                cmdAccion.Parameters.AddWithValue("@est", idEstanteDestino); // ¡Se va al estante correcto automáticamente!
                                                cmdAccion.ExecuteNonQuery();
                                            }
                                        }
                                        fila++;
                                        totalImportados++;
                                    }
                                }
                                transaccion.Commit();

                                // ==========================================
                                // REPORTE FINAL PARA EL USUARIO
                                // ==========================================
                                string mensajeFinal = $"¡Carga Inteligente completada!\nArtículos procesados y guardados: {totalImportados}.";

                                if (!string.IsNullOrEmpty(hojasIgnoradas))
                                {
                                    mensajeFinal += $"\n\nOJO: Las siguientes pestañas fueron ignoradas porque no coinciden con ningún estante creado en el sistema:\n{hojasIgnoradas}\n(Revisa la ortografía de la pestaña o crea el estante primero).";
                                }

                                MessageBox.Show(mensajeFinal, "Resumen de Carga", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                        // Recargamos la interfaz para mostrar los cambios en el estante que tengamos seleccionado en pantalla
                        if (cmbEstantes.SelectedValue != null && cmbEstantes.SelectedValue is long idActual)
                        {
                            CargarArticulosPorEstante(idActual);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar la carga masiva: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void descargarPlantillaCargaMasivaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Archivo de Excel|*.xlsx";
            dialog.Title = "Descargar Plantilla para Carga Masiva";
            dialog.FileName = "Plantilla_Carga_Masiva.xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        // Creamos una hoja sencilla
                        var ws = workbook.Worksheets.Add("Carga Masiva");

                        // ==========================================
                        // 1. ENCABEZADOS (Fila 1)
                        // ==========================================
                        ws.Cell("A1").Value = "NOMBRE DEL ARTÍCULO";
                        ws.Cell("B1").Value = "CÓDIGO (Obligatorio)";
                        ws.Cell("C1").Value = "PRESENTACIÓN";

                        // Le damos formato visual de "Tabla" a los encabezados
                        var rangoHeaders = ws.Range("A1:C1");
                        rangoHeaders.Style.Font.Bold = true;
                        rangoHeaders.Style.Font.FontColor = XLColor.White;
                        rangoHeaders.Style.Fill.BackgroundColor = XLColor.DarkBlue; // Un color institucional
                        rangoHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        rangoHeaders.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                        rangoHeaders.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // ==========================================
                        // 2. TEXTOS DE EJEMPLO (Fila 2)
                        // ==========================================
                        ws.Cell("A2").Value = "[Ej: Sonda Levin 8]";
                        ws.Cell("B2").Value = "[Ej: 02140306]";
                        ws.Cell("C2").Value = "[Ej: Unidad / Caja x 100]";

                        // Ponemos el ejemplo en gris cursiva para que sepan que deben borrarlo
                        var rangoEjemplo = ws.Range("A2:C2");
                        rangoEjemplo.Style.Font.Italic = true;
                        rangoEjemplo.Style.Font.FontColor = XLColor.Gray;

                        // ==========================================
                        // 3. AJUSTE DE COLUMNAS
                        // ==========================================
                        // Hacemos las columnas suficientemente anchas para escribir cómodamente
                        ws.Column(1).Width = 40; // Nombre
                        ws.Column(2).Width = 25; // Código
                        ws.Column(3).Width = 35; // Presentación

                        workbook.SaveAs(dialog.FileName);
                        MessageBox.Show("Plantilla de carga masiva descargada con éxito.\n\nEl personal puede llenarla borrando la fila de ejemplo y luego subirla desde el menú 'Artículos -> Importar Carga Masiva'.", "Plantilla Lista", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar la plantilla: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
    //AAA
}
