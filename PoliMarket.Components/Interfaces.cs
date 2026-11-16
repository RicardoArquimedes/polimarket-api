using PoliMarket.Domain;

namespace PoliMarket.Components;

// Interfaz: gestionarAutorizaciones (RF1)
public interface IRecursosHumanosFacade
{
    Autorizacion AutorizarVendedor(string idVendedor, DateTime fechaVencimiento);
    bool EstaAutorizado(string idVendedor);
    Empleado? ConsultarEmpleado(string idEmpleado);
}

// Interfaz: gestionarVentas (RF2, RF5)
public interface IVentasFacade
{
    List<Cliente> ListarClientes();
    Venta RegistrarVenta(string idVendedor, string idCliente, List<DetalleVenta> detalles);
    double CalcularTotal(string idVenta);
    List<Venta> ConsultarHistorialCliente(string idCliente);
    List<Venta> ConsultarVentasPorEntregar();
    Venta? ObtenerVenta(string idVenta);
    void MarcarVentaEntregada(string idVenta);
}

// Interfaz: gestionarBodega (RF3, RF4, RF5)
public interface IBodegaFacade
{
    List<Producto> ListarProductosDisponibles();
    int ConsultarDisponibilidad(string idProducto);
    List<Producto> ConsultarProductosPorReabastecer();
    OrdenReabastecimiento SolicitarReabastecimiento(string idProducto, int cantidad);
    void RegistrarEntrada(string idOrden);
    void RegistrarSalida(string idProducto, int cantidad);
}

// Interfaz: gestionarAbastecimiento (RF4)
public interface IProveedoresFacade
{
    List<Proveedor> ConsultarProveedoresPorProducto(string idProducto);
    List<Producto> ConsultarCatalogo(string idProveedor);
    OrdenReabastecimiento CrearOrden(string idProveedor, string idProducto, int cantidad);
    OrdenReabastecimiento ConfirmarRecepcion(string idOrden);
}

// Interfaz: gestionarEntregas (RF5)
public interface IEntregasFacade
{
    Entrega ProgramarEntrega(string idVenta, DateTime fechaProgramada, string direccionEntrega);
    void RegistrarEntrega(string idEntrega);
    EstadoEntrega ConsultarEstadoEntrega(string idEntrega);
}
