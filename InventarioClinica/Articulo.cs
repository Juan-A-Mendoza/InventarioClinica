using System;

namespace InventarioClinica
{
    public class MovimientoInventario
    {
        public DateTime Fecha { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Presentacion { get; set; }
        public string Documento { get; set; } // Ej: "inventario", "compra", "hosp"

        // Usamos int? (con el signo de interrogación) para permitir valores nulos.
        // Así podemos dejar la celda en blanco o poner un guion "-" cuando no haya entrada o salida.
        public int? Entrada { get; set; }
        public int? Salida { get; set; }

        public int Existencia { get; set; } // El total acumulado hasta esa fecha
        public string Observaciones { get; set; }
    }
}