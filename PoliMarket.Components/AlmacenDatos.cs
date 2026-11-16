using PoliMarket.Domain;

namespace PoliMarket.Components;

public class AlmacenDatos
{
    public List<Vendedor> Vendedores { get; } = new()
    {
        new Vendedor { IdEmpleado = "EMP01", CodigoVendedor = "V01", Nombre = "Carlos Pérez", Documento = "101010", Cargo = "Vendedor Senior", Correo = "carlos@polimarket.com", Zona = "Norte", MetaMensual = 5000000 },
        new Vendedor { IdEmpleado = "EMP02", CodigoVendedor = "V02", Nombre = "Laura Gómez", Documento = "202020", Cargo = "Vendedor Junior", Correo = "laura@polimarket.com", Zona = "Sur", MetaMensual = 3000000 }
    };

    public List<Cliente> Clientes { get; } = new()
    {
        new Cliente { IdCliente = "CLI01", Nombre = "Supermercado El Ahorro", Documento = "900111222", Direccion = "Calle 10 # 5-20", Telefono = "3001112233" },
        new Cliente { IdCliente = "CLI02", Nombre = "Tienda La Esquina", Documento = "900333444", Direccion = "Carrera 15 # 8-40", Telefono = "3104445566" }
    };

    public List<Producto> Productos { get; } = new()
    {
        new Producto { IdProducto = "PROD01", Nombre = "Arroz Premium 5kg", Descripcion = "Arroz blanco seleccionado", PrecioUnitario = 18500, UnidadMedida = "Bolsa" },
        new Producto { IdProducto = "PROD02", Nombre = "Aceite Vegetal 3L", Descripcion = "Aceite de girasol", PrecioUnitario = 24000, UnidadMedida = "Garrafa" },
        new Producto { IdProducto = "PROD03", Nombre = "Café Molido 500g", Descripcion = "Café tostión media", PrecioUnitario = 14000, UnidadMedida = "Paquete" }
    };

    public Bodega BodegaPrincipal { get; }

    public List<Proveedor> Proveedores { get; } = new();
    public List<Venta> Ventas { get; } = new();
    public List<Entrega> Entregas { get; } = new();
    public List<OrdenReabastecimiento> Ordenes { get; } = new();

    public AlmacenDatos()
    {
        BodegaPrincipal = new Bodega
        {
            IdBodega = "BOD01",
            Nombre = "Bodega Central",
            Ubicacion = "Zona Industrial",
            Inventarios = new List<Inventario>
            {
                new() { IdBodega = "BOD01", IdProducto = "PROD01", CantidadDisponible = 50, StockMinimo = 15, UltimaActualizacion = DateTime.Now },
                new() { IdBodega = "BOD01", IdProducto = "PROD02", CantidadDisponible = 8, StockMinimo = 10, UltimaActualizacion = DateTime.Now },
                new() { IdBodega = "BOD01", IdProducto = "PROD03", CantidadDisponible = 30, StockMinimo = 10, UltimaActualizacion = DateTime.Now }
            }
        };

        Proveedores.Add(new Proveedor
        {
            IdProveedor = "PROV01",
            RazonSocial = "Distribuidora Alimentos S.A.",
            Nit = "800999888",
            Contacto = "Andrés Ruiz",
            Telefono = "6017778899",
            Catalogo = new List<Producto> { Productos[0], Productos[1], Productos[2] }
        });
    }
}
