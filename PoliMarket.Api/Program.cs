using PoliMarket.Components;
using PoliMarket.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de dependencias (Arquitectura de Fachadas)
builder.Services.AddSingleton<AlmacenDatos>();
builder.Services.AddSingleton<IRecursosHumanosFacade, RecursosHumanosFacade>();
builder.Services.AddSingleton<IProveedoresFacade, ProveedoresFacade>();
builder.Services.AddSingleton<IBodegaFacade, BodegaFacade>();
builder.Services.AddSingleton<IVentasFacade, VentasFacade>();
builder.Services.AddSingleton<IEntregasFacade, EntregasFacade>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

// Ruta raíz para evitar el 404 en http://localhost:5000/
app.MapGet("/", () => Results.Redirect("/swagger"));

// --- RF1: Fachada Recursos Humanos ---
app.MapPost("/api/rrhh/autorizar/{idVendedor}", (string idVendedor, DateTime fechaVencimiento, IRecursosHumanosFacade rrhh) =>
    Results.Ok(rrhh.AutorizarVendedor(idVendedor, fechaVencimiento)));

app.MapGet("/api/rrhh/autorizado/{idVendedor}", (string idVendedor, IRecursosHumanosFacade rrhh) =>
    Results.Ok(new { idVendedor, autorizado = rrhh.EstaAutorizado(idVendedor) }));

app.MapGet("/api/rrhh/empleado/{idEmpleado}", (string idEmpleado, IRecursosHumanosFacade rrhh) =>
    Results.Ok(rrhh.ConsultarEmpleado(idEmpleado)));

// --- RF2: Fachada Ventas ---
app.MapGet("/api/ventas/clientes", (IVentasFacade ventas) => Results.Ok(ventas.ListarClientes()));

app.MapPost("/api/ventas", (CrearVentaRequest req, IVentasFacade ventas) =>
{
    try { return Results.Ok(ventas.RegistrarVenta(req.IdVendedor, req.IdCliente, req.Detalles)); }
    catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
});

app.MapGet("/api/ventas/historial/{idCliente}", (string idCliente, IVentasFacade ventas) =>
    Results.Ok(ventas.ConsultarHistorialCliente(idCliente)));

app.MapGet("/api/ventas/por-entregar", (IVentasFacade ventas) =>
    Results.Ok(ventas.ConsultarVentasPorEntregar()));

// --- RF3 & RF4: Fachada Bodega ---
app.MapGet("/api/bodega/productos", (IBodegaFacade bodega) => Results.Ok(bodega.ListarProductosDisponibles()));

app.MapGet("/api/bodega/disponibilidad/{idProducto}", (string idProducto, IBodegaFacade bodega) =>
    Results.Ok(new { idProducto, disponible = bodega.ConsultarDisponibilidad(idProducto) }));

app.MapGet("/api/bodega/por-reabastecer", (IBodegaFacade bodega) =>
    Results.Ok(bodega.ConsultarProductosPorReabastecer()));

app.MapPost("/api/bodega/reabastecer", (string idProducto, int cantidad, IBodegaFacade bodega) =>
    Results.Ok(bodega.SolicitarReabastecimiento(idProducto, cantidad)));

app.MapPost("/api/bodega/entrada/{idOrden}", (string idOrden, IBodegaFacade bodega) =>
{
    bodega.RegistrarEntrada(idOrden);
    return Results.Ok(new { mensaje = "Entrada registrada y stock actualizado." });
});

// --- RF5: Fachada Entregas ---
app.MapPost("/api/entregas/programar", (ProgramarEntregaRequest req, IEntregasFacade entregas) =>
    Results.Ok(entregas.ProgramarEntrega(req.IdVenta, req.FechaProgramada, req.DireccionEntrega)));

app.MapPost("/api/entregas/registrar/{idEntrega}", (string idEntrega, IEntregasFacade entregas) =>
{
    entregas.RegistrarEntrega(idEntrega);
    return Results.Ok(new { mensaje = "Entrega confirmada y salida de stock aplicada." });
});

app.MapGet("/api/entregas/estado/{idEntrega}", (string idEntrega, IEntregasFacade entregas) =>
    Results.Ok(new { idEntrega, estado = entregas.ConsultarEstadoEntrega(idEntrega).ToString() }));

app.Run();

public record CrearVentaRequest(string IdVendedor, string IdCliente, List<DetalleVenta> Detalles);
public record ProgramarEntregaRequest(string IdVenta, DateTime FechaProgramada, string DireccionEntrega);
