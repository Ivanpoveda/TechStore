namespace TECHSTORE.ViewModels
{
    public class DetalleVentaCreateViewModel
    {
        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}