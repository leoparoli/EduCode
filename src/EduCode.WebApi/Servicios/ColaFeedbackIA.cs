using System.Threading.Channels;
using EduCode.Aplicacion.Interfaces;

namespace EduCode.WebApi.Servicios;

public sealed class ColaFeedbackIA : IColaFeedbackIA
{
    private readonly Channel<int> _canal = Channel.CreateBounded<int>(
        new BoundedChannelOptions(200)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EncolarAsync(
        int intentoId,
        CancellationToken ct = default) =>
        _canal.Writer.WriteAsync(intentoId, ct);

    public ValueTask<int> DesencolarAsync(CancellationToken ct) =>
        _canal.Reader.ReadAsync(ct);
}
