using PoliMarket.Domain;

namespace PoliMarket.Components;

// 1. Componente RecursosHumanos (RF1)
public class RecursosHumanosFacade : IRecursosHumanosFacade
{
    private readonly AlmacenDatos _db;
    public RecursosHumanosFacade(AlmacenDatos db) => _db = db;

    public Autorizacion AutorizarVendedor(string idVendedor, DateTime fechaVencimiento)
    {
        var vendedor = _db.Vendedores.FirstOrDefault(v => v.IdEmpleado == idVendedor || v.CodigoVendedor == idVendedor)
            ?? throw new KeyNotFoundException("Vendedor no encontrado.");

        var auth = new Autorizacion
        {
            IdAutorizacion = $"AUTH-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdVendedor = vendedor.IdEmpleado,
            FechaEmision = DateTime.Now,
            FechaVencimiento = fechaVencimiento,
            Estado = EstadoAutorizacion.Activa
        };
        vendedor.AutorizacionActual = auth;
        return auth;
    }

    public bool EstaAutorizado(string idVendedor)
    {
        var vendedor = _db.Vendedores.FirstOrDefault(v => v.IdEmpleado == idVendedor || v.CodigoVendedor == idVendedor);
        return vendedor != null && vendedor.EstaAutorizado();
    }

    public Empleado? ConsultarEmpleado(string idEmpleado) =>
        _db.Vendedores.FirstOrDefault(v => v.IdEmpleado == idEmpleado)?.ConsultarDatos();
}

// 2. Componente Proveedores (RF4)
public class ProveedoresFacade : IProveedoresFacade
{
    private readonly AlmacenDatos _db;
    public ProveedoresFacade(AlmacenDatos db) => _db = db;

    public List<Proveedor> ConsultarProveedoresPorProducto(string idProducto) =>
        _db.Proveedores.Where(p => p.Catalogo.Any(prod => prod.IdProducto == idProducto)).ToList();

    public List<Producto> ConsultarCatalogo(string idProveedor) =>
        _db.Proveedores.FirstOrDefault(p => p.IdProveedor == idProveedor)?.ConsultarCatalogo() ?? new();

    public OrdenReabastecimiento CrearOrden(string idProveedor, string idProducto, int cantidad)
    {
        var orden = new OrdenReabastecimiento
        {
            IdOrden = $"ORD-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdProveedor = idProveedor,
            IdProducto = idProducto,
            FechaSolicitud = DateTime.Now,
            CantidadSolicitada = cantidad,
            FechaEstimada = DateTime.Now.AddDays(3),
            Estado = EstadoOrden.Pendiente
        };
        _db.Ordenes.Add(orden);
        return orden;
    }

    public OrdenReabastecimiento ConfirmarRecepcion(string idOrden)
    {
        var orden = _db.Ordenes.FirstOrDefault(o => o.IdOrden == idOrden)
            ?? throw new KeyNotFoundException("Orden no encontrada.");
        orden.ConfirmarRecepcion();
        return orden;
    }
}

// 3. Componente Bodega (RF3, RF4, RF5) -> Usa IProveedoresFacade
public class BodegaFacade : IBodegaFacade
{
    private readonly AlmacenDatos _db;
    private readonly IProveedoresFacade _proveedoresFacade;

    public BodegaFacade(AlmacenDatos db, IProveedoresFacade proveedoresFacade)
    {
        _db = db;
        _proveedoresFacade = proveedoresFacade;
    }

    public List<Producto> ListarProductosDisponibles()
    {
        var idsDisponibles = _db.BodegaPrincipal.Inventarios
            .Where(i => i.CantidadDisponible > 0)
            .Select(i => i.IdProducto)
            .ToHashSet();

        return _db.Productos.Where(p => idsDisponibles.Contains(p.IdProducto)).ToList();
    }

    public int ConsultarDisponibilidad(string idProducto) =>
        _db.BodegaPrincipal.ConsultarDisponibilidad(idProducto);

    public List<Producto> ConsultarProductosPorReabastecer()
    {
        var idsBajoStock = _db.BodegaPrincipal.Inventarios
            .Where(i => i.RequiereReabastecer())
            .Select(i => i.IdProducto)
            .ToHashSet();

        return _db.Productos.Where(p => idsBajoStock.Contains(p.IdProducto)).ToList();
    }

    public OrdenReabastecimiento SolicitarReabastecimiento(string idProducto, int cantidad)
    {
        var proveedor = _proveedoresFacade.ConsultarProveedoresPorProducto(idProducto).FirstOrDefault()
            ?? throw new InvalidOperationException("No hay proveedores disponibles para este producto.");

        return _proveedoresFacade.CrearOrden(proveedor.IdProveedor, idProducto, cantidad);
    }

    public void RegistrarEntrada(string idOrden)
    {
        var orden = _proveedoresFacade.ConfirmarRecepcion(idOrden);
        var inv = _db.BodegaPrincipal.Inventarios.First(i => i.IdProducto == orden.IdProducto);
        
        var mov = new MovimientoInventario
        {
            IdMovimiento = $"MOV-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdProducto = orden.IdProducto,
            Tipo = TipoMovimiento.Entrada,
            Cantidad = orden.CantidadSolicitada,
            Fecha = DateTime.Now
        };
        mov.AplicarMovimiento(inv);
    }

    public void RegistrarSalida(string idProducto, int cantidad)
    {
        var inv = _db.BodegaPrincipal.Inventarios.FirstOrDefault(i => i.IdProducto == idProducto)
            ?? throw new KeyNotFoundException("Producto no encontrado en inventario.");

        var mov = new MovimientoInventario
        {
            IdMovimiento = $"MOV-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdProducto = idProducto,
            Tipo = TipoMovimiento.Salida,
            Cantidad = cantidad,
            Fecha = DateTime.Now
        };
        mov.AplicarMovimiento(inv);
    }
}

// 4. Componente Ventas (RF2, RF5) -> Usa IRecursosHumanosFacade e IBodegaFacade
public class VentasFacade : IVentasFacade
{
    private readonly AlmacenDatos _db;
    private readonly IRecursosHumanosFacade _rrhhFacade;
    private readonly IBodegaFacade _bodegaFacade;

    public VentasFacade(AlmacenDatos db, IRecursosHumanosFacade rrhhFacade, IBodegaFacade bodegaFacade)
    {
        _db = db;
        _rrhhFacade = rrhhFacade;
        _bodegaFacade = bodegaFacade;
    }

    public List<Cliente> ListarClientes() => _db.Clientes;

    public Venta RegistrarVenta(string idVendedor, string idCliente, List<DetalleVenta> detalles)
    {
        if (!_rrhhFacade.EstaAutorizado(idVendedor))
            throw new UnauthorizedAccessException("El vendedor no está autorizado para registrar ventas.");

        if (!_db.Clientes.Any(c => c.IdCliente == idCliente))
            throw new KeyNotFoundException("Cliente no encontrado.");

        foreach (var item in detalles)
        {
            var prod = _db.Productos.FirstOrDefault(p => p.IdProducto == item.IdProducto)
                ?? throw new KeyNotFoundException($"Producto {item.IdProducto} no existe.");
            
            if (_bodegaFacade.ConsultarDisponibilidad(item.IdProducto) < item.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para {prod.Nombre}.");

            item.PrecioUnitario = prod.ConsultarPrecio();
        }

        var venta = new Venta
        {
            IdVenta = $"VTA-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdVendedor = idVendedor,
            IdCliente = idCliente,
            Fecha = DateTime.Now,
            Estado = EstadoVenta.Confirmada,
            Detalles = detalles
        };
        venta.CalcularTotal();
        _db.Ventas.Add(venta);
        return venta;
    }

    public double CalcularTotal(string idVenta) =>
        _db.Ventas.FirstOrDefault(v => v.IdVenta == idVenta)?.CalcularTotal() ?? 0;

    public List<Venta> ConsultarHistorialCliente(string idCliente) =>
        _db.Ventas.Where(v => v.IdCliente == idCliente).ToList();

    public List<Venta> ConsultarVentasPorEntregar() =>
        _db.Ventas.Where(v => v.Estado == EstadoVenta.Confirmada).ToList();

    public Venta? ObtenerVenta(string idVenta) =>
        _db.Ventas.FirstOrDefault(v => v.IdVenta == idVenta);

    public void MarcarVentaEntregada(string idVenta)
    {
        var venta = ObtenerVenta(idVenta);
        if (venta != null) venta.Estado = EstadoVenta.Entregada;
    }
}

// 5. Componente Entregas (RF5) -> Usa IVentasFacade e IBodegaFacade
public class EntregasFacade : IEntregasFacade
{
    private readonly AlmacenDatos _db;
    private readonly IVentasFacade _ventasFacade;
    private readonly IBodegaFacade _bodegaFacade;

    public EntregasFacade(AlmacenDatos db, IVentasFacade ventasFacade, IBodegaFacade bodegaFacade)
    {
        _db = db;
        _ventasFacade = ventasFacade;
        _bodegaFacade = bodegaFacade;
    }

    public Entrega ProgramarEntrega(string idVenta, DateTime fechaProgramada, string direccionEntrega)
    {
        var ventaPendiente = _ventasFacade.ConsultarVentasPorEntregar()
            .FirstOrDefault(v => v.IdVenta == idVenta)
            ?? throw new InvalidOperationException("La venta no existe o no está disponible para entrega.");

        var entrega = new Entrega
        {
            IdEntrega = $"ENT-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdVenta = ventaPendiente.IdVenta,
            FechaProgramada = fechaProgramada,
            DireccionEntrega = direccionEntrega,
            Transportador = "Logística PoliMarket",
            Estado = EstadoEntrega.Programada
        };
        _db.Entregas.Add(entrega);
        return entrega;
    }

    public void RegistrarEntrega(string idEntrega)
    {
        var entrega = _db.Entregas.FirstOrDefault(e => e.IdEntrega == idEntrega)
            ?? throw new KeyNotFoundException("Entrega no encontrada.");

        var venta = _ventasFacade.ObtenerVenta(entrega.IdVenta)
            ?? throw new KeyNotFoundException("Venta asociada no encontrada.");

        foreach (var detalle in venta.Detalles)
        {
            _bodegaFacade.RegistrarSalida(detalle.IdProducto, detalle.Cantidad);
        }

        entrega.RegistrarEntrega();
        _ventasFacade.MarcarVentaEntregada(venta.IdVenta);
    }

    public EstadoEntrega ConsultarEstadoEntrega(string idEntrega) =>
        _db.Entregas.FirstOrDefault(e => e.IdEntrega == idEntrega)?.Estado
            ?? throw new KeyNotFoundException("Entrega no encontrada.");
}
