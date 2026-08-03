using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class VisVentasDetalle
{
    public decimal IdVenta { get; set; }

    public string? Cliente { get; set; }

    public DateTime Fecha { get; set; }

    public string Producto { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? Impuesto { get; set; }

    public decimal? Descuento { get; set; }

    public decimal? Total { get; set; }

    public string Estado { get; set; } = null!;
}
