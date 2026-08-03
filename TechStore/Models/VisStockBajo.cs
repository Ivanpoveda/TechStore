using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class VisStockBajo
{
    public decimal IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Stock { get; set; }

    public decimal StockMin { get; set; }

    public string Categoria { get; set; } = null!;

    public string Marca { get; set; } = null!;
}
