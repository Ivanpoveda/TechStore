using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class DetalleCompra
{
    public int IdDetalleCompra { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal Subtotal { get; set; }

    public int IdCompra { get; set; }

    public int IdProducto { get; set; }

    public virtual CompraProveedor IdCompraNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
