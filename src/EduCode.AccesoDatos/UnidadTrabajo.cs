using EduCode.Aplicacion.Interfaces;

namespace EduCode.AccesoDatos;

public sealed class UnidadTrabajo : IUnidadTrabajo
{
    private readonly EduCodeContext _contexto;

    public UnidadTrabajo(EduCodeContext contexto)
    {
        _contexto = contexto;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) =>
        _contexto.SaveChangesAsync(ct);
}
