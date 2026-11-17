# ClienteOperaciones · PoliMarket (Python)

Segundo cliente de la Actividad Sumativa U2 — Reutilización de software. Se construyó deliberadamente en una **plataforma distinta** al cliente web (Python puro, consola, sin frameworks) para cumplir el requisito de heterogeneidad de clientes de la actividad.

Usa únicamente la librería estándar de Python (`urllib`, `json`) — no requiere `pip install` de nada.

## Funcionalidades cubiertas (RF4 — Gestionar reabastecimiento)

Corresponde al rol `ClienteOperaciones` documentado en la Tabla 1 del diseño:

1. `ConsultarProductosPorReabastecer` — qué productos están en o bajo su stock mínimo.
2. `SolicitarReabastecimiento` — generar una orden de compra al proveedor.
3. `RegistrarEntrada` — confirmar la recepción de una orden y actualizar el inventario.

## Requisitos previos

El backend `PoliMarket.Api` debe estar corriendo en `http://localhost:5000`:

```bash
dotnet run --project PoliMarket.Api
```

## Ejecución

```bash
python3 operations_client.py
```

No necesita entorno virtual ni dependencias: solo Python 3 (probado con 3.10, compatible desde 3.7+ gracias a `from __future__ import annotations`).
