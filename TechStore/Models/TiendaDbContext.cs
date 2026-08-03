using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TechStore.Models;

public partial class TiendaDbContext : DbContext
{
    public TiendaDbContext()
    {
    }

    public TiendaDbContext(DbContextOptions<TiendaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<CompraProveedor> CompraProveedors { get; set; }

    public virtual DbSet<DetalleCarrito> DetalleCarritos { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<Garantium> Garantia { get; set; }

    public virtual DbSet<HistorialInventario> HistorialInventarios { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    public virtual DbSet<VisHistorialInventario> VisHistorialInventarios { get; set; }

    public virtual DbSet<VisStockBajo> VisStockBajos { get; set; }

    public virtual DbSet<VisVentasDetalle> VisVentasDetalles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("User Id=TIENDA;Password=Tienda2026;Data Source=localhost:1521/xe;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("TIENDA")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.IdCarrito).HasName("SYS_C008483");

            entity.ToTable("CARRITO");

            entity.Property(e => e.IdCarrito)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CARRITO");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValueSql("'Activo' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_CREACION");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CARRITO_USUARIO");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("SYS_C008421");

            entity.ToTable("CATEGORIA");

            entity.Property(e => e.IdCategoria)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CATEGORIA");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<CompraProveedor>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("SYS_C008498");

            entity.ToTable("COMPRA_PROVEEDOR");

            entity.Property(e => e.IdCompra)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_COMPRA");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'Pendiente' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_COMPRA");
            entity.Property(e => e.IdProveedor)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PROVEEDOR");
            entity.Property(e => e.Total)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("TOTAL");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.CompraProveedors)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COMPRA_PROVEEDOR");
        });

        modelBuilder.Entity<DetalleCarrito>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCarrito).HasName("SYS_C008490");

            entity.ToTable("DETALLE_CARRITO");

            entity.HasIndex(e => e.IdCarrito, "IDX_DETCARRITO_ID_CARRITO");

            entity.Property(e => e.IdDetalleCarrito)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_DETALLE_CARRITO");
            entity.Property(e => e.Cantidad)
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");
            entity.Property(e => e.IdCarrito)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CARRITO");
            entity.Property(e => e.IdProducto)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.DetalleCarritos)
                .HasForeignKey(d => d.IdCarrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETCARRITO_CARRITO");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCarritos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETCARRITO_PRODUCTO");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra).HasName("SYS_C008507");

            entity.ToTable("DETALLE_COMPRA");

            entity.HasIndex(e => e.IdCompra, "IDX_DETCOMPRA_ID_COMPRA");

            entity.Property(e => e.IdDetalleCompra)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_DETALLE_COMPRA");
            entity.Property(e => e.Cantidad)
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");
            entity.Property(e => e.IdCompra)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_COMPRA");
            entity.Property(e => e.IdProducto)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.PrecioCompra)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("PRECIO_COMPRA");
            entity.Property(e => e.Subtotal)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("SUBTOTAL");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETCOMPRA_COMPRA");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETCOMPRA_PRODUCTO");
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("SYS_C008468");

            entity.ToTable("DETALLE_VENTA");

            entity.HasIndex(e => e.IdVenta, "IDX_DETVENTA_ID_VENTA");

            entity.Property(e => e.IdDetalleVenta)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_DETALLE_VENTA");

            entity.Property(e => e.Cantidad)
                .ValueGeneratedNever()   
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");

            entity.Property(e => e.IdProducto)
                .ValueGeneratedNever() 
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");

            entity.Property(e => e.IdVenta)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VENTA");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("PRECIO_UNITARIO");
            entity.Property(e => e.Subtotal)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("SUBTOTAL");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETVENTA_PRODUCTO");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETVENTA_VENTA");
        });

        modelBuilder.Entity<Garantium>(entity =>
        {
            entity.HasKey(e => e.IdGarantia).HasName("SYS_C008476");

            entity.ToTable("GARANTIA");

            entity.Property(e => e.IdGarantia)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_GARANTIA");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValueSql("'En proceso' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("DATE")
                .HasColumnName("FECHA_RESOLUCION");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_SOLICITUD");
            entity.Property(e => e.IdDetalleVenta)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_DETALLE_VENTA");
            entity.Property(e => e.Motivo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("MOTIVO");

            entity.HasOne(d => d.IdDetalleVentaNavigation).WithMany(p => p.Garantia)
                .HasForeignKey(d => d.IdDetalleVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GARANTIA_DETVENTA");
        });

        modelBuilder.Entity<HistorialInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("SYS_C008516");

            entity.ToTable("HISTORIAL_INVENTARIO");

            entity.Property(e => e.IdMovimiento)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MOVIMIENTO");
            entity.Property(e => e.Cantidad)
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdProducto)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.Observacion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("OBSERVACION");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TIPO_MOVIMIENTO");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.HistorialInventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HIST_PRODUCTO");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("SYS_C008424");

            entity.ToTable("MARCA");

            entity.Property(e => e.IdMarca)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MARCA");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.PaisOrigen)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("PAIS_ORIGEN");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("SYS_C008451");

            entity.ToTable("PRODUCTO");

            entity.HasIndex(e => e.Nombre, "IDX_PRODUCTO_NOMBRE");

            entity.Property(e => e.IdProducto)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValueSql("'Activo' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.IdCategoria)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CATEGORIA");
            entity.Property(e => e.IdMarca)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MARCA");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Precio)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("PRECIO");
            entity.Property(e => e.Stock)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("STOCK");
            entity.Property(e => e.StockMin)
                .HasDefaultValueSql("5 ")
                .HasColumnType("NUMBER")
                .HasColumnName("STOCK_MIN");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTO_CATEGORIA");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTO_MARCA");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("SYS_C008427");

            entity.ToTable("PROVEEDOR");

            entity.Property(e => e.IdProveedor)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PROVEEDOR");
            entity.Property(e => e.Correo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("CORREO");
            entity.Property(e => e.Empresa)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("EMPRESA");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("SITIO_WEB");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TELEFONO");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("SYS_C008418");

            entity.ToTable("ROL");

            entity.Property(e => e.IdRol)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_ROL");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("SYS_C008437");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Correo, "UQ_USUARIO_CORREO").IsUnique();

            entity.Property(e => e.IdUsuario)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("APELLIDOS");
            entity.Property(e => e.Contrasenia)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("CONTRASENIA");
            entity.Property(e => e.Correo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("CORREO");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValueSql("'Activo' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_REGISTRO");
            entity.Property(e => e.IdRol)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_ROL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TELEFONO");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("SYS_C008459");

            entity.ToTable("VENTA");

            entity.Property(e => e.IdVenta)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VENTA");
            entity.Property(e => e.Descuento)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("DESCUENTO");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'Pendiente' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");
            entity.Property(e => e.Impuesto)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("IMPUESTO");
            entity.Property(e => e.Total)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("TOTAL");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VENTA_USUARIO");
        });

        modelBuilder.Entity<VisHistorialInventario>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIS_HISTORIAL_INVENTARIO");

            entity.Property(e => e.Cantidad)
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");
            entity.Property(e => e.Fecha)
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdMovimiento)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MOVIMIENTO");
            entity.Property(e => e.Observacion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("OBSERVACION");
            entity.Property(e => e.Producto)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("PRODUCTO");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TIPO_MOVIMIENTO");
        });

        modelBuilder.Entity<VisStockBajo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIS_STOCK_BAJO");

            entity.Property(e => e.Categoria)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("CATEGORIA");
            entity.Property(e => e.IdProducto)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.Marca)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("MARCA");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Stock)
                .HasColumnType("NUMBER")
                .HasColumnName("STOCK");
            entity.Property(e => e.StockMin)
                .HasColumnType("NUMBER")
                .HasColumnName("STOCK_MIN");
        });

        modelBuilder.Entity<VisVentasDetalle>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIS_VENTAS_DETALLE");

            entity.Property(e => e.Cantidad)
                .HasColumnType("NUMBER")
                .HasColumnName("CANTIDAD");
            entity.Property(e => e.Cliente)
                .HasMaxLength(101)
                .IsUnicode(false)
                .HasColumnName("CLIENTE");
            entity.Property(e => e.Descuento)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("DESCUENTO");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("ESTADO");
            entity.Property(e => e.Fecha)
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdVenta)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VENTA");
            entity.Property(e => e.Impuesto)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("IMPUESTO");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("PRECIO_UNITARIO");
            entity.Property(e => e.Producto)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("PRODUCTO");
            entity.Property(e => e.Subtotal)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("SUBTOTAL");
            entity.Property(e => e.Total)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("TOTAL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
