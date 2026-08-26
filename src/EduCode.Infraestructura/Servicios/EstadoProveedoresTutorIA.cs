namespace EduCode.Infraestructura.Servicios;

/// <summary>
/// Conserva trazabilidad en memoria de los intentos reales del Tutor IA.
/// No almacena API keys ni cuerpos de las consultas.
/// </summary>
public sealed class EstadoProveedoresTutorIA
{
    private readonly object _sincronizacion = new();
    private readonly Dictionary<string, EstadoProveedorTutorMutable> _proveedores =
        new(StringComparer.OrdinalIgnoreCase);

    public void RegistrarIntento(string proveedor, string modelo)
    {
        lock (_sincronizacion)
        {
            EstadoProveedorTutorMutable estado = ObtenerOCrear(proveedor);
            estado.Modelo = modelo;
            estado.UltimoIntentoUtc = DateTime.UtcNow;
            estado.CantidadIntentos++;
        }
    }

    public void RegistrarExito(string proveedor, string modelo)
    {
        lock (_sincronizacion)
        {
            EstadoProveedorTutorMutable estado = ObtenerOCrear(proveedor);
            estado.Modelo = modelo;
            estado.Operativo = true;
            estado.UltimoExitoUtc = DateTime.UtcNow;
            estado.UltimoCodigoHttp = 200;
            estado.UltimoError = string.Empty;
        }
    }

    public void RegistrarFallo(
        string proveedor,
        string modelo,
        int? codigoHttp,
        string mensaje)
    {
        lock (_sincronizacion)
        {
            EstadoProveedorTutorMutable estado = ObtenerOCrear(proveedor);
            estado.Modelo = modelo;
            estado.Operativo = false;
            estado.UltimoFalloUtc = DateTime.UtcNow;
            estado.UltimoCodigoHttp = codigoHttp;
            estado.UltimoError = Sanitizar(mensaje);
            estado.CantidadFallos++;
        }
    }

    public EstadoProveedoresTutorSnapshot ObtenerEstado()
    {
        lock (_sincronizacion)
        {
            List<EstadoProveedorTutorSnapshot> proveedores = _proveedores
                .OrderBy(par => par.Key, StringComparer.OrdinalIgnoreCase)
                .Select(par => new EstadoProveedorTutorSnapshot
                {
                    Proveedor = par.Key,
                    Modelo = par.Value.Modelo,
                    Operativo = par.Value.Operativo,
                    UltimoIntentoUtc = par.Value.UltimoIntentoUtc,
                    UltimoExitoUtc = par.Value.UltimoExitoUtc,
                    UltimoFalloUtc = par.Value.UltimoFalloUtc,
                    UltimoCodigoHttp = par.Value.UltimoCodigoHttp,
                    UltimoError = par.Value.UltimoError,
                    CantidadIntentos = par.Value.CantidadIntentos,
                    CantidadFallos = par.Value.CantidadFallos
                })
                .ToList();

            string estadoGeneral = proveedores.Count == 0
                ? "Sin verificar"
                : proveedores.Any(p => p.Operativo)
                    ? proveedores.All(p => p.Operativo)
                        ? "Operativo"
                        : "Operativo con respaldo"
                    : "No disponible";

            return new EstadoProveedoresTutorSnapshot
            {
                EstadoGeneral = estadoGeneral,
                Proveedores = proveedores
            };
        }
    }

    private EstadoProveedorTutorMutable ObtenerOCrear(string proveedor)
    {
        if (_proveedores.TryGetValue(proveedor, out EstadoProveedorTutorMutable? estado))
            return estado;

        estado = new EstadoProveedorTutorMutable();
        _proveedores[proveedor] = estado;
        return estado;
    }

    private static string Sanitizar(string mensaje)
    {
        string limpio = (mensaje ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        return limpio.Length <= 500 ? limpio : limpio[..500];
    }

    private sealed class EstadoProveedorTutorMutable
    {
        public string Modelo { get; set; } = string.Empty;
        public bool Operativo { get; set; }
        public DateTime? UltimoIntentoUtc { get; set; }
        public DateTime? UltimoExitoUtc { get; set; }
        public DateTime? UltimoFalloUtc { get; set; }
        public int? UltimoCodigoHttp { get; set; }
        public string UltimoError { get; set; } = string.Empty;
        public int CantidadIntentos { get; set; }
        public int CantidadFallos { get; set; }
    }
}

public sealed class EstadoProveedoresTutorSnapshot
{
    public string EstadoGeneral { get; init; } = "Sin verificar";
    public List<EstadoProveedorTutorSnapshot> Proveedores { get; init; } = [];
}

public sealed class EstadoProveedorTutorSnapshot
{
    public string Proveedor { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public bool Operativo { get; init; }
    public DateTime? UltimoIntentoUtc { get; init; }
    public DateTime? UltimoExitoUtc { get; init; }
    public DateTime? UltimoFalloUtc { get; init; }
    public int? UltimoCodigoHttp { get; init; }
    public string UltimoError { get; init; } = string.Empty;
    public int CantidadIntentos { get; init; }
    public int CantidadFallos { get; init; }
}
