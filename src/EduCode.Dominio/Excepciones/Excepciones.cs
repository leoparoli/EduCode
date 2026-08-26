namespace EduCode.Dominio.Excepciones;

public abstract class ExcepcionDominio : Exception
{
    protected ExcepcionDominio(string message) : base(message) { }
}

public sealed class EntidadNoEncontradaException : ExcepcionDominio
{
    public EntidadNoEncontradaException(string entidad, int id)
        : base($"No se encontró {entidad} con identificador {id}.") { }
}

public sealed class ReglaNegocioException : ExcepcionDominio
{
    public ReglaNegocioException(string message) : base(message) { }
}

public sealed class CredencialesInvalidasException : ExcepcionDominio
{
    public CredencialesInvalidasException() : base("Las credenciales ingresadas no son válidas.") { }
}

public sealed class ConflictoDatosException : ExcepcionDominio
{
    public ConflictoDatosException(string message) : base(message) { }
}

public sealed class ServicioExternoException : ExcepcionDominio
{
    public ServicioExternoException(string message) : base(message) { }
}

public sealed class ServicioExternoTemporalException : ExcepcionDominio
{
    public ServicioExternoTemporalException(
        string message,
        int? codigoEstadoHttp = null,
        int cantidadReintentos = 0)
        : base(message)
    {
        CodigoEstadoHttp = codigoEstadoHttp;
        CantidadReintentos = cantidadReintentos;
    }

    public int? CodigoEstadoHttp { get; }
    public int CantidadReintentos { get; }
}

public sealed class LimiteSolicitudesIAException : ExcepcionDominio
{
    public LimiteSolicitudesIAException(string message) : base(message) { }
}
