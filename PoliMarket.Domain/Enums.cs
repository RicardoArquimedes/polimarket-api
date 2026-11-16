namespace PoliMarket.Domain;

public enum EstadoAutorizacion { Activa, Vencida, Revocada }
public enum EstadoVenta { Pendiente, Confirmada, Entregada, Cancelada }
public enum EstadoEntrega { Programada, EnTransito, Entregada, Fallida }
public enum TipoMovimiento { Entrada, Salida }
public enum EstadoOrden { Pendiente, Recibida, Cancelada }
