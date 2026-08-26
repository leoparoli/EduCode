using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CUGamificacion
{
    private readonly IRepositorioGamificacion _gamificacion;
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUGamificacion(
        IRepositorioGamificacion gamificacion,
        IRepositorioEjercicios ejercicios,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _gamificacion = gamificacion;
        _ejercicios = ejercicios;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public async Task<ResumenGamificacionDTO> ObtenerResumenAsync(
        int limiteRanking = 20,
        CancellationToken ct = default)
    {
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        await EvaluarYAsignarLogrosAsync(usuarioId, ct);

        IReadOnlyCollection<Usuario> usuarios =
            await _gamificacion.ObtenerUsuariosConProgresosAsync(ct);
        Usuario? usuario = usuarios.FirstOrDefault(u => u.Id == usuarioId);

        if (usuario is null)
        {
            return new ResumenGamificacionDTO();
        }

        IReadOnlyCollection<Ejercicio> ejercicios =
            await _ejercicios.ObtenerActivosAsync(ct);
        IReadOnlyCollection<IntentoEjercicio> intentos =
            await _gamificacion.ObtenerIntentosUsuarioAsync(usuarioId, ct);
        IReadOnlyCollection<Logro> logros =
            await _gamificacion.ObtenerLogrosActivosAsync(ct);
        IReadOnlyCollection<UsuarioLogro> obtenidos =
            await _gamificacion.ObtenerLogrosUsuarioAsync(usuarioId, ct);

        MetricaUsuario metricaActual = CalcularMetrica(usuario);
        RachasCalculadas rachas = CalcularRachas(intentos);

        List<MetricaUsuario> elegibles = usuarios
            .Where(u =>
                u.Rol == RolUsuario.Alumno &&
                u.Estado == EstadoUsuario.Activo &&
                u.MostrarEnRanking)
            .Select(CalcularMetrica)
            .OrderByDescending(m => m.PuntajeTotal)
            .ThenByDescending(m => m.EjerciciosAprobados)
            .ThenByDescending(m => m.Promedio)
            .ThenBy(m => m.Usuario.FechaRegistro)
            .ThenBy(m => m.Usuario.Id)
            .ToList();

        int? posicion = usuario.MostrarEnRanking
            ? elegibles.FindIndex(m => m.Usuario.Id == usuarioId) + 1
            : null;

        Dictionary<int, UsuarioLogro> obtenidosPorLogro = obtenidos
            .GroupBy(ul => ul.LogroId)
            .ToDictionary(g => g.Key, g => g.First());

        return new ResumenGamificacionDTO
        {
            VisibleEnRanking = usuario.MostrarEnRanking,
            PosicionRanking = posicion is > 0 ? posicion : null,
            PuntajeRanking = metricaActual.PuntajeTotal,
            PuntajeMaximoDisponible = ejercicios.Count * 100m,
            PromedioMejoresPuntajes = metricaActual.Promedio,
            EjerciciosIntentados = metricaActual.EjerciciosIntentados,
            EjerciciosAprobados = metricaActual.EjerciciosAprobados,
            RachaActual = new RachaResolucionDTO
            {
                CantidadEjercicios = rachas.ActualCantidad,
                Promedio = rachas.ActualPromedio
            },
            MejorRacha = new RachaResolucionDTO
            {
                CantidadEjercicios = rachas.MejorCantidad,
                Promedio = rachas.MejorPromedio
            },
            Insignias = logros
                .OrderBy(l => l.Tipo)
                .ThenBy(l => l.ValorObjetivo)
                .Select(logro => ConstruirLogroDTO(
                    logro,
                    obtenidosPorLogro.GetValueOrDefault(logro.Id),
                    metricaActual,
                    rachas))
                .ToList(),
            Ranking = elegibles
                .Take(Math.Clamp(limiteRanking, 5, 100))
                .Select((m, indice) => new PosicionRankingDTO
                {
                    Posicion = indice + 1,
                    NombreVisible = $"{m.Usuario.Nombre} {m.Usuario.Apellido}",
                    NombreUsuario = m.Usuario.NombreUsuario,
                    AvatarClave = m.Usuario.AvatarClave,
                    FotoPerfilRuta = m.Usuario.FotoPerfilRuta ?? string.Empty,
                    PuntajeTotal = m.PuntajeTotal,
                    Promedio = m.Promedio,
                    EjerciciosAprobados = m.EjerciciosAprobados,
                    EjerciciosIntentados = m.EjerciciosIntentados,
                    EsUsuarioActual = m.Usuario.Id == usuarioId
                })
                .ToList()
        };
    }

    public async Task CambiarVisibilidadRankingAsync(
        bool mostrar,
        CancellationToken ct = default)
    {
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        Usuario usuario = await _gamificacion.ObtenerUsuarioAsync(usuarioId, ct)
            ?? throw new EntidadNoEncontradaException("el usuario", usuarioId);

        usuario.CambiarVisibilidadRanking(mostrar);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task<List<LogroDesbloqueadoDTO>> EvaluarYAsignarLogrosAsync(
        int usuarioId,
        CancellationToken ct = default)
    {
        IReadOnlyCollection<Usuario> usuarios =
            await _gamificacion.ObtenerUsuariosConProgresosAsync(ct);
        Usuario? usuario = usuarios.FirstOrDefault(u => u.Id == usuarioId);

        if (usuario is null)
        {
            return [];
        }

        MetricaUsuario metrica = CalcularMetrica(usuario);
        IReadOnlyCollection<IntentoEjercicio> intentos =
            await _gamificacion.ObtenerIntentosUsuarioAsync(usuarioId, ct);
        RachasCalculadas rachas = CalcularRachas(intentos);
        IReadOnlyCollection<Logro> logros =
            await _gamificacion.ObtenerLogrosActivosAsync(ct);
        IReadOnlyCollection<UsuarioLogro> obtenidos =
            await _gamificacion.ObtenerLogrosUsuarioAsync(usuarioId, ct);
        HashSet<int> idsObtenidos = obtenidos.Select(ul => ul.LogroId).ToHashSet();
        List<LogroDesbloqueadoDTO> nuevos = [];

        foreach (Logro logro in logros.Where(l => !idsObtenidos.Contains(l.Id)))
        {
            int valorActual = ObtenerValorActual(logro.Tipo, metrica, rachas);
            if (valorActual < logro.ValorObjetivo)
            {
                continue;
            }

            await _gamificacion.AgregarUsuarioLogroAsync(
                new UsuarioLogro(usuarioId, logro.Id),
                ct);

            nuevos.Add(new LogroDesbloqueadoDTO
            {
                LogroId = logro.Id,
                Nombre = logro.Nombre,
                Descripcion = logro.Descripcion,
                Icono = logro.Icono
            });
        }

        if (nuevos.Count > 0)
        {
            await _uow.GuardarCambiosAsync(ct);
        }

        return nuevos;
    }

    private static LogroUsuarioDTO ConstruirLogroDTO(
        Logro logro,
        UsuarioLogro? obtenido,
        MetricaUsuario metrica,
        RachasCalculadas rachas)
    {
        int actual = ObtenerValorActual(logro.Tipo, metrica, rachas);
        int porcentaje = logro.ValorObjetivo <= 0
            ? 100
            : Math.Clamp(
                (int)Math.Round(actual * 100m / logro.ValorObjetivo),
                0,
                100);

        return new LogroUsuarioDTO
        {
            LogroId = logro.Id,
            Nombre = logro.Nombre,
            Descripcion = logro.Descripcion,
            Icono = logro.Icono,
            Tipo = logro.Tipo.ToString(),
            ValorObjetivo = logro.ValorObjetivo,
            ValorActual = actual,
            PorcentajeProgreso = porcentaje,
            Obtenido = obtenido is not null,
            FechaObtencion = obtenido?.FechaObtencion
        };
    }

    private static int ObtenerValorActual(
        TipoLogro tipo,
        MetricaUsuario metrica,
        RachasCalculadas rachas) => tipo switch
    {
        TipoLogro.EjerciciosCompletados => metrica.EjerciciosAprobados,
        TipoLogro.RachaEjercicios => rachas.MejorCantidad,
        TipoLogro.PuntajeObtenido => (int)Math.Floor(metrica.PuntajeTotal),
        _ => 0
    };

    private static MetricaUsuario CalcularMetrica(Usuario usuario)
    {
        List<ProgresoEjercicio> progresos = usuario.ProgresosEjercicios
            .Where(p => p.CantidadIntentos > 0)
            .ToList();

        decimal total = decimal.Round(
            progresos.Sum(p => p.MejorPuntaje),
            2,
            MidpointRounding.AwayFromZero);

        int aprobados = progresos.Count(p =>
            p.Estado is EstadoProgreso.Aprobado or EstadoProgreso.Completado);

        return new MetricaUsuario(
            usuario,
            total,
            progresos.Count == 0
                ? 0m
                : decimal.Round(total / progresos.Count, 2),
            progresos.Count,
            aprobados);
    }

    private static RachasCalculadas CalcularRachas(
        IReadOnlyCollection<IntentoEjercicio> intentos)
    {
        int actual = 0;
        decimal sumaActual = 0m;
        int mejor = 0;
        decimal mejorPromedio = 0m;
        HashSet<int> ejerciciosRacha = [];

        foreach (IntentoEjercicio intento in intentos
                     .Where(i => i.PuntajeObtenido.HasValue)
                     .OrderBy(i => i.FechaEnvio)
                     .ThenBy(i => i.Id))
        {
            if (intento.Estado == EstadoIntento.Correcto)
            {
                if (!ejerciciosRacha.Add(intento.EjercicioId))
                {
                    continue;
                }

                actual++;
                sumaActual += intento.PuntajeObtenido ?? 0m;
                decimal promedioActual = decimal.Round(sumaActual / actual, 2);

                if (actual > mejor ||
                    (actual == mejor && promedioActual > mejorPromedio))
                {
                    mejor = actual;
                    mejorPromedio = promedioActual;
                }
            }
            else if (intento.Estado is
                     EstadoIntento.Incorrecto or
                     EstadoIntento.ParcialmenteCorrecto)
            {
                actual = 0;
                sumaActual = 0m;
                ejerciciosRacha.Clear();
            }
        }

        return new RachasCalculadas(
            actual,
            actual == 0 ? 0m : decimal.Round(sumaActual / actual, 2),
            mejor,
            mejorPromedio);
    }

    private sealed record MetricaUsuario(
        Usuario Usuario,
        decimal PuntajeTotal,
        decimal Promedio,
        int EjerciciosIntentados,
        int EjerciciosAprobados);

    private sealed record RachasCalculadas(
        int ActualCantidad,
        decimal ActualPromedio,
        int MejorCantidad,
        decimal MejorPromedio);
}
