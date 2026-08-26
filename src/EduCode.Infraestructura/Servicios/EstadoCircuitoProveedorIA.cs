namespace EduCode.Infraestructura.Servicios;

public sealed class EstadoCircuitoProveedorIA
{
    private readonly object _sincronizacion = new();
    private int _fallasConsecutivas;
    private DateTime? _abiertoHastaUtc;

    public bool EstaAbierto(out TimeSpan tiempoRestante)
    {
        lock (_sincronizacion)
        {
            if (_abiertoHastaUtc is null)
            {
                tiempoRestante = TimeSpan.Zero;
                return false;
            }

            DateTime ahora = DateTime.UtcNow;

            if (_abiertoHastaUtc <= ahora)
            {
                _abiertoHastaUtc = null;
                _fallasConsecutivas = 0;
                tiempoRestante = TimeSpan.Zero;
                return false;
            }

            tiempoRestante = _abiertoHastaUtc.Value - ahora;
            return true;
        }
    }

    public void RegistrarExito()
    {
        lock (_sincronizacion)
        {
            _fallasConsecutivas = 0;
            _abiertoHastaUtc = null;
        }
    }

    public void RegistrarFalloTransitorio(
        int fallasParaAbrir,
        TimeSpan duracion)
    {
        lock (_sincronizacion)
        {
            _fallasConsecutivas++;

            if (_fallasConsecutivas >= Math.Max(1, fallasParaAbrir))
            {
                _abiertoHastaUtc = DateTime.UtcNow.Add(
                    duracion <= TimeSpan.Zero
                        ? TimeSpan.FromSeconds(90)
                        : duracion);
            }
        }
    }

    public EstadoCircuitoSnapshot ObtenerEstado()
    {
        lock (_sincronizacion)
        {
            bool abierto = _abiertoHastaUtc > DateTime.UtcNow;

            return new EstadoCircuitoSnapshot
            {
                Abierto = abierto,
                FallasConsecutivas = _fallasConsecutivas,
                AbiertoHastaUtc = abierto ? _abiertoHastaUtc : null
            };
        }
    }
}

public sealed class EstadoCircuitoSnapshot
{
    public bool Abierto { get; init; }
    public int FallasConsecutivas { get; init; }
    public DateTime? AbiertoHastaUtc { get; init; }
}
