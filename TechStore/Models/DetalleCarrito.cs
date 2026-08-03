using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class DetalleCarrito
{
    public decimal IdDetalleCarrito { get; set; }

    public decimal Cantidad { get; set; }

    public decimal IdCarrito { get; set; }

    public decimal IdProducto { get; set; }

    public virtual Carrito IdCarritoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
