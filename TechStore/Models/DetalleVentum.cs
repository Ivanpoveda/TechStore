using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class DetalleVentum
{
    public int IdDetalleVenta { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public int IdVenta { get; set; }

    public int IdProducto { get; set; }

    public virtual ICollection<Garantium> Garantia { get; set; } = new List<Garantium>();

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
