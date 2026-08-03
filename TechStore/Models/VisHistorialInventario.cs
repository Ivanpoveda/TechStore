using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class VisHistorialInventario
{
    public decimal IdMovimiento { get; set; }

    public string Producto { get; set; } = null!;

    public string TipoMovimiento { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public DateTime Fecha { get; set; }

    public string? Observacion { get; set; }
}
