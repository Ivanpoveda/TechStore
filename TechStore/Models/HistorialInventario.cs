using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class HistorialInventario
{
    public decimal IdMovimiento { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public DateTime Fecha { get; set; }

    public string? Observacion { get; set; }

    public decimal IdProducto { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
