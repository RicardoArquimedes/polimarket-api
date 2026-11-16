namespace PoliMarket.Domain;

// --- Subsistema: Recursos Humanos ---
public abstract class Empleado
{
    public string IdEmpleado { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    public Empleado ConsultarDatos() => this;
}

public class Vendedor : Empleado
{
    public string CodigoVendedor { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public double MetaMensual { get; set; }
    public Autorizacion? AutorizacionActual { get; set; }

    public bool EstaAutorizado() => AutorizacionActual != null && AutorizacionActual.EsVigente();
}

public class Autorizacion
{
    public string IdAutorizacion { get; set; } = string.Empty;
    public string IdVendedor { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoAutorizacion Estado { get; set; }

    public bool EsVigente() => Estado == EstadoAutorizacion.Activa && FechaVencimiento >= DateTime.Now;
}

// --- Subsistema: Ventas ---
public class Cliente
{
    public string IdCliente { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public class DetalleVenta
{
    public string IdProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal => CalcularSubtotal();

    public double CalcularSubtotal() => Cantidad * PrecioUnitario;
}

public class Venta
{
    public string IdVenta { get; set; } = string.Empty;
    public string IdVendedor { get; set; } = string.Empty;
    public string IdCliente { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public double Total { get; set; }
    public EstadoVenta Estado { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();

    public double CalcularTotal()
    {
        Total = Detalles.Sum(d => d.CalcularSubtotal());
        return Total;
    }
}

// --- Subsistema: Bodega ---
public class Producto
{
    public string IdProducto { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double PrecioUnitario { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;

    public double ConsultarPrecio() => PrecioUnitario;
}

public class Inventario
{
    public string IdBodega { get; set; } = string.Empty;
    public string IdProducto { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int StockMinimo { get; set; }
    public DateTime UltimaActualizacion { get; set; }

    public bool RequiereReabastecer() => CantidadDisponible <= StockMinimo;

    public void DescontarStock(int cantidad)
    {
        if (CantidadDisponible < cantidad)
            throw new InvalidOperationException($"Stock insuficiente para el producto {IdProducto}.");
        CantidadDisponible -= cantidad;
        UltimaActualizacion = DateTime.Now;
    }

    public void AumentarStock(int cantidad)
    {
        CantidadDisponible += cantidad;
        UltimaActualizacion = DateTime.Now;
    }
}

public class Bodega
{
    public string IdBodega { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public List<Inventario> Inventarios { get; set; } = new();

    public int ConsultarDisponibilidad(string idProducto) =>
        Inventarios.FirstOrDefault(i => i.IdProducto == idProducto)?.CantidadDisponible ?? 0;
}

// --- Subsistema: Entregas ---
public class MovimientoInventario
{
    public string IdMovimiento { get; set; } = string.Empty;
    public string IdProducto { get; set; } = string.Empty;
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }

    public void AplicarMovimiento(Inventario inventario)
    {
        if (Tipo == TipoMovimiento.Salida) inventario.DescontarStock(Cantidad);
        else inventario.AumentarStock(Cantidad);
    }
}

public class Entrega
{
    public string IdEntrega { get; set; } = string.Empty;
    public string IdVenta { get; set; } = string.Empty;
    public DateTime FechaProgramada { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string DireccionEntrega { get; set; } = string.Empty;
    public string Transportador { get; set; } = string.Empty;
    public EstadoEntrega Estado { get; set; }
    public List<MovimientoInventario> Movimientos { get; set; } = new();

    public void RegistrarEntrega()
    {
        FechaEntrega = DateTime.Now;
        Estado = EstadoEntrega.Entregada;
    }
}

// --- Subsistema: Proveedores ---
public class Proveedor
{
    public string IdProveedor { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public List<Producto> Catalogo { get; set; } = new();

    public List<Producto> ConsultarCatalogo() => Catalogo;
}

public class OrdenReabastecimiento
{
    public string IdOrden { get; set; } = string.Empty;
    public string IdProveedor { get; set; } = string.Empty;
    public string IdProducto { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public int CantidadSolicitada { get; set; }
    public DateTime FechaEstimada { get; set; }
    public EstadoOrden Estado { get; set; }

    public void ConfirmarRecepcion() => Estado = EstadoOrden.Recibida;
}
