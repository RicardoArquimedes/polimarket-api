"""
ClienteOperaciones - PoliMarket
Segundo cliente de la Actividad Sumativa U2, en una plataforma distinta al
cliente web (React/TS). Usa solo la librería estándar de Python (urllib),
sin dependencias externas, para consumir el backend PoliMarket.Api (C#/.NET)
expuesto en http://localhost:5000.

Cubre RF4 completo (gestionar reabastecimiento), tal como lo documenta la
Tabla 1 de Persona 2 para este cliente:
  - ConsultarProductosPorReabastecer
  - SolicitarReabastecimiento
  - RegistrarEntrada
"""

from __future__ import annotations

import json
import os
import sys
import urllib.error
import urllib.request

# Por defecto apunta al backend local. Para probar contra el backend desplegado
# (Render, etc.), exporta la variable de entorno o pásala como argumento:
#   POLIMARKET_API_URL=https://tu-servicio.onrender.com python3 operations_client.py
BASE_URL = sys.argv[1] if len(sys.argv) > 1 else os.environ.get("POLIMARKET_API_URL", "https://polimarket-api.onrender.com")


def _request(method: str, path: str, body: dict | None = None):
    url = f"{BASE_URL}{path}"
    data = json.dumps(body).encode("utf-8") if body is not None else None
    req = urllib.request.Request(url, data=data, method=method)
    if data is not None:
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req) as resp:
            raw = resp.read().decode("utf-8")
            return json.loads(raw) if raw else None
    except urllib.error.HTTPError as e:
        detalle = e.read().decode("utf-8")
        raise RuntimeError(f"Error {e.code} en {path}: {detalle}") from None
    except urllib.error.URLError as e:
        raise RuntimeError(
            f"No se pudo conectar a {BASE_URL}. "
            f"¿Está corriendo 'dotnet run --project PoliMarket.Api'? ({e.reason})"
        ) from None


def consultar_productos_por_reabastecer():
    return _request("GET", "/api/bodega/por-reabastecer")


def solicitar_reabastecimiento(id_producto: str, cantidad: int):
    # El endpoint toma idProducto y cantidad como query string (no body)
    path = f"/api/bodega/reabastecer?idProducto={id_producto}&cantidad={cantidad}"
    return _request("POST", path)


def registrar_entrada(id_orden: str):
    return _request("POST", f"/api/bodega/entrada/{id_orden}")


def menu():
    opciones = {
        "1": "Consultar productos por reabastecer",
        "2": "Solicitar reabastecimiento",
        "3": "Registrar entrada de mercancía",
        "0": "Salir",
    }
    for clave, texto in opciones.items():
        print(f"  {clave}. {texto}")


def main():
    print("=== ClienteOperaciones · PoliMarket (Python, stdlib) ===")
    while True:
        print()
        menu()
        opcion = input("Elige una opción: ").strip()

        try:
            if opcion == "1":
                productos = consultar_productos_por_reabastecer()
                if not productos:
                    print("No hay productos que requieran reabastecimiento.")
                for p in productos:
                    print(f"  - {p['idProducto']} | {p['nombre']} ({p['unidadMedida']})")

            elif opcion == "2":
                id_producto = input("ID de producto: ").strip()
                cantidad = int(input("Cantidad a solicitar: ").strip())
                orden = solicitar_reabastecimiento(id_producto, cantidad)
                print(f"Orden creada: {orden['idOrden']} (proveedor {orden['idProveedor']}, "
                      f"estado {orden['estado']})")

            elif opcion == "3":
                id_orden = input("ID de orden a confirmar: ").strip()
                resultado = registrar_entrada(id_orden)
                print(resultado.get("mensaje", resultado))

            elif opcion == "0":
                print("Hasta luego.")
                sys.exit(0)

            else:
                print("Opción no válida.")

        except RuntimeError as e:
            print(f"⚠️  {e}")
        except (ValueError, KeyError) as e:
            print(f"⚠️  Entrada inválida: {e}")


if __name__ == "__main__":
    main()
