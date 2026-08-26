namespace EduCode.Infraestructura.Servicios;

public sealed class ExcepcionProveedorIA : Exception
{
    public ExcepcionProveedorIA(
        string message,
        int? codigoEstadoHttp,
        bool esTransitoria,
        int numeroIntentoProveedor,
        Exception? innerException = null)
        : base(message, innerException)
    {
        CodigoEstadoHttp = codigoEstadoHttp;
        EsTransitoria = esTransitoria;
        NumeroIntentoProveedor = numeroIntentoProveedor;
    }

    public int? CodigoEstadoHttp { get; }
    public bool EsTransitoria { get; }
    public int NumeroIntentoProveedor { get; }
}
