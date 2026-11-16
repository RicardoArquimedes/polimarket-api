# PoliMarket - Backend por Componentes (C# / .NET 8)

Implementación de la capa de dominio, componentes de negocio (patrón Fachada) y API REST para el caso de estudio **PoliMarket**.

## Estructura de la Solución
- `PoliMarket.Domain`: Entidades y enumeraciones del diagrama de clases UML 2.5.
- `PoliMarket.Components`: Contratos (interfaces) y fachadas independientes (`RecursosHumanosFacade`, `VentasFacade`, `BodegaFacade`, `ProveedoresFacade`, `EntregasFacade`).
- `PoliMarket.Api`: Exposición HTTP REST en `http://localhost:5000` para ser consumida por los dos clientes externos (`ClienteVentas` y `ClienteOperaciones`).

## Ejecución
1. Abrir una terminal en la carpeta raíz `PoliMarket/`.
2. Ejecutar:
   ```bash
   dotnet run --project PoliMarket.Api
   ```
3. El servidor quedará escuchando en `http://localhost:5000`.
