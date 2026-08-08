using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class VisStockBajo
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public int Stock { get; set; }

    public int StockMin { get; set; }

    public string Categoria { get; set; } = null!;

    public string Marca { get; set; } = null!;
}
