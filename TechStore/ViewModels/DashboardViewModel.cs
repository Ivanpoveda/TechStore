using System.Collections.Generic;
using TechStore.Models;

namespace TechStore.ViewModels
{
    public class DashboardViewModel
    {
        // Métricas principales
        public decimal IngresosTotales { get; set; }
        public int UsuariosActivos { get; set; }
        public int ProductosEnStock { get; set; }
        public int OrdenesPendientes { get; set; }
        public List<PedidoDto> UltimosPedidos { get; set; } = new List<PedidoDto>();

        // Gráficos
        public List<string> Meses { get; set; } = new List<string>();
        public List<decimal> VentasPorMes { get; set; } = new List<decimal>();
        public List<string> Estados { get; set; } = new List<string>();
        public List<int> VentasPorEstado { get; set; } = new List<int>();

        // Reportes avanzados
        public List<VisStockBajo> ProductosStockBajo { get; set; } = new List<VisStockBajo>();
        public List<VisVentasDetalle> DetalleVentas { get; set; } = new List<VisVentasDetalle>();
        public List<VisHistorialInventario> HistorialInventario { get; set; } = new List<VisHistorialInventario>();
    }

    public class PedidoDto
    {
        public string Cliente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}


