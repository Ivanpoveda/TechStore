namespace TechStore.ViewModels
{
    public class DashboardViewModel
    {
        public decimal IngresosTotales { get; set; }
        public int NuevosClientes { get; set; }
        public int OrdenesPendientes { get; set; }
        public List<PedidoDto> UltimosPedidos { get; set; }
    }

    public class PedidoDto
    {
        public string Cliente { get; set; }
        public string Estado { get; set; }
        public decimal Monto { get; set; }
    }
}