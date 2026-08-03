using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class DetalleCompra
{
    public decimal IdDetalleCompra { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal Subtotal { get; set; }

    public decimal IdCompra { get; set; }

    public decimal IdProducto { get; set; }

    public virtual CompraProveedor IdCompraNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
