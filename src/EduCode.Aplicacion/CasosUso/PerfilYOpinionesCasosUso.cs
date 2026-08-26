using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CUPerfilUsuario
{
    private const int MaximoCambiosNombreUsuario = 2;
    private const int DiasVentanaCambiosNombreUsuario = 15;

    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioCambiosNombreUsuario _cambiosNombreUsuario;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUPerfilUsuario(
        IRepositorioUsuarios usuarios,
        IRepositorioCambiosNombreUsuario cambiosNombreUsuario,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _usuarios = usuarios;
        _cambiosNombreUsuario = cambiosNombreUsuario;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public async Task<PerfilUsuarioDTO> ObtenerAsync(
        CancellationToken ct = default)
    {
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        Usuario usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new EntidadNoEncontradaException("el usuario", usuarioId);

        return await MapearAsync(usuario, DateTime.UtcNow, ct);
    }

    public async Task<PerfilUsuarioDTO> ActualizarAsync(
        ActualizarPerfilDTO dto,
        CancellationToken ct = default)
    {
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        Usuario usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new EntidadNoEncontradaException("el usuario", usuarioId);

        DateTime ahoraUtc = DateTime.UtcNow;
        string nombreUsuarioSolicitado = Usuario.NormalizarNombreUsuario(dto.NombreUsuario);
        bool cambiaNombreUsuario = !string.Equals(
            usuario.NombreUsuario,
            nombreUsuarioSolicitado,
            StringComparison.Ordinal);

        if (usuario.Rol == RolUsuario.Administrador && cambiaNombreUsuario)
        {
            throw new ReglaNegocioException(
                "El nombre de usuario de una cuenta administradora está protegido para conservar la trazabilidad de auditoría.");
        }

        if (cambiaNombreUsuario)
        {
            DateTime desdeUtc = ahoraUtc.AddDays(-DiasVentanaCambiosNombreUsuario);
            IReadOnlyCollection<CambioNombreUsuario> cambiosRecientes =
                await _cambiosNombreUsuario.ObtenerDesdeAsync(usuarioId, desdeUtc, ct);

            if (cambiosRecientes.Count >= MaximoCambiosNombreUsuario)
            {
                DateTime proximaFecha = cambiosRecientes
                    .OrderBy(cambio => cambio.FechaCambio)
                    .First()
                    .FechaCambio
                    .AddDays(DiasVentanaCambiosNombreUsuario);

                throw new ReglaNegocioException(
                    $"Alcanzaste el máximo de {MaximoCambiosNombreUsuario} cambios de nombre de usuario " +
                    $"en {DiasVentanaCambiosNombreUsuario} días. Podrás volver a cambiarlo a partir del " +
                    $"{proximaFecha:dd/MM/yyyy}.");
            }

            if (await _usuarios.ExisteNombreUsuarioDeOtroAsync(
                nombreUsuarioSolicitado, usuarioId, ct))
            {
                throw new ConflictoDatosException(
                    "Ese nombre de usuario ya está siendo utilizado. Elegí otro.");
            }

            var cambio = new CambioNombreUsuario(
                usuarioId,
                usuario.NombreUsuario,
                nombreUsuarioSolicitado,
                ahoraUtc);

            usuario.CambiarNombreUsuario(nombreUsuarioSolicitado);
            await _cambiosNombreUsuario.AgregarAsync(cambio, ct);
        }

        usuario.SeleccionarAvatar(dto.AvatarClave);

        if (dto.EliminarFotoPerfil)
        {
            usuario.QuitarFotoPerfil();
        }
        else if (!string.IsNullOrWhiteSpace(dto.NuevaFotoPerfilRuta))
        {
            string nuevaRuta = dto.NuevaFotoPerfilRuta;
            string prefijoEsperado = $"/uploads/perfiles/perfil-{usuarioId}-";
            if (!nuevaRuta.StartsWith(
                prefijoEsperado,
                StringComparison.Ordinal))
            {
                throw new ReglaNegocioException("La foto de perfil enviada no es válida.");
            }

            usuario.CambiarFotoPerfil(nuevaRuta);
        }

        await _uow.GuardarCambiosAsync(ct);
        return await MapearAsync(usuario, ahoraUtc, ct);
    }

    private async Task<PerfilUsuarioDTO> MapearAsync(
        Usuario usuario,
        DateTime ahoraUtc,
        CancellationToken ct)
    {
        DateTime desdeUtc = ahoraUtc.AddDays(-DiasVentanaCambiosNombreUsuario);
        IReadOnlyCollection<CambioNombreUsuario> cambiosRecientes =
            await _cambiosNombreUsuario.ObtenerDesdeAsync(usuario.Id, desdeUtc, ct);

        bool identidadAdministrativa = usuario.Rol == RolUsuario.Administrador;
        int usados = identidadAdministrativa
            ? 0
            : Math.Min(cambiosRecientes.Count, MaximoCambiosNombreUsuario);
        int disponibles = identidadAdministrativa
            ? 0
            : Math.Max(0, MaximoCambiosNombreUsuario - usados);
        DateTime? proximoCambio = !identidadAdministrativa && disponibles == 0
            ? cambiosRecientes
                .OrderBy(cambio => cambio.FechaCambio)
                .First()
                .FechaCambio
                .AddDays(DiasVentanaCambiosNombreUsuario)
            : null;

        return new PerfilUsuarioDTO
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            AvatarClave = CatalogoAvataresPerfil.Validar(usuario.AvatarClave),
            FotoPerfilRuta = usuario.FotoPerfilRuta ?? string.Empty,
            MaximoCambiosNombreUsuario = MaximoCambiosNombreUsuario,
            DiasVentanaCambiosNombreUsuario = DiasVentanaCambiosNombreUsuario,
            CambiosNombreUsuarioUsados = usados,
            CambiosNombreUsuarioDisponibles = disponibles,
            PuedeCambiarNombreUsuario = !identidadAdministrativa && disponibles > 0,
            ProximoCambioNombreUsuarioDisponible = proximoCambio,
            AvataresDisponibles = ConstruirAvatares()
        };
    }

    private static List<AvatarDisponibleDTO> ConstruirAvatares() =>
    [
        new() { Clave = "codigo", Nombre = "Código", Icono = "</>" },
        new() { Clave = "terminal", Nombre = "Terminal", Icono = ">_" },
        new() { Clave = "cohete", Nombre = "Impulso", Icono = "🚀" },
        new() { Clave = "robot", Nombre = "Robot", Icono = "🤖" },
        new() { Clave = "cerebro", Nombre = "Lógica", Icono = "🧠" },
        new() { Clave = "laptop", Nombre = "Desarrollo", Icono = "💻" },
        new() { Clave = "bug", Nombre = "Debug", Icono = "🐞" },
        new() { Clave = "braces", Nombre = "Llaves", Icono = "{}" }
    ];
}

public sealed class CUOpinionesUsuario
{
    private const int MaximoOpinionesDiarias = 5;
    private readonly IRepositorioOpiniones _opiniones;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUOpinionesUsuario(
        IRepositorioOpiniones opiniones,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _opiniones = opiniones;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public async Task<OpinionUsuarioDTO> CrearAsync(
        CrearOpinionDTO dto,
        CancellationToken ct = default)
    {
        ExigirRolParticipante();
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        DateTime inicioDiaUtc = DateTime.UtcNow.Date;
        int cantidadHoy = await _opiniones.ContarPorUsuarioDesdeAsync(
            usuarioId, inicioDiaUtc, ct);

        if (cantidadHoy >= MaximoOpinionesDiarias)
            throw new ReglaNegocioException(
                "Podés enviar hasta 5 opiniones por día. Gracias por ayudarnos a mejorar.");

        string comentario = (dto.Comentario ?? string.Empty).Trim();
        bool duplicada = await _opiniones.ExisteDuplicadaRecienteAsync(
            usuarioId, comentario, DateTime.UtcNow.AddMinutes(-10), ct);
        if (duplicada)
            throw new ConflictoDatosException(
                "Esta opinión ya fue enviada recientemente.");

        if (!Enum.IsDefined(typeof(CategoriaOpinion), dto.Categoria))
            throw new ReglaNegocioException("Seleccioná una categoría válida.");

        var opinion = new OpinionUsuario(
            usuarioId,
            (CategoriaOpinion)dto.Categoria,
            dto.Calificacion,
            comentario,
            dto.AutorizaPublicacion);

        await _opiniones.AgregarAsync(opinion, ct);
        await _uow.GuardarCambiosAsync(ct);
        return Mapear(opinion);
    }

    public async Task<List<OpinionUsuarioDTO>> ObtenerMiasAsync(
        CancellationToken ct = default)
    {
        ExigirRolParticipante();
        int usuarioId = _usuarioActual.ObtenerUsuarioId();
        IReadOnlyCollection<OpinionUsuario> opiniones =
            await _opiniones.ObtenerPorUsuarioAsync(usuarioId, ct);
        return opiniones.Select(Mapear).ToList();
    }

    public async Task<List<OpinionUsuarioDTO>> ObtenerTodasAsync(
        CancellationToken ct = default)
    {
        ExigirRolGestion();
        IReadOnlyCollection<OpinionUsuario> opiniones =
            await _opiniones.ObtenerTodasConUsuarioAsync(ct);
        return opiniones.Select(Mapear).ToList();
    }

    public async Task<OpinionUsuarioDTO> CambiarEstadoAsync(
        int opinionId,
        ActualizarEstadoOpinionDTO dto,
        CancellationToken ct = default)
    {
        ExigirRolGestion();
        if (!Enum.IsDefined(typeof(EstadoOpinion), dto.Estado))
            throw new ReglaNegocioException("Seleccioná un estado válido.");

        OpinionUsuario opinion = await _opiniones.ObtenerConUsuarioAsync(opinionId, ct)
            ?? throw new EntidadNoEncontradaException("la opinión", opinionId);
        opinion.CambiarEstado((EstadoOpinion)dto.Estado);
        await _uow.GuardarCambiosAsync(ct);
        return Mapear(opinion);
    }

    private void ExigirRolParticipante()
    {
        string rol = _usuarioActual.ObtenerRol();
        if (rol.Equals(RolUsuario.Administrador.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "Las cuentas administradoras supervisan las opiniones, pero no pueden enviarlas como participantes.");
        }
    }

    private void ExigirRolGestion()
    {
        string rol = _usuarioActual.ObtenerRol();
        if (!rol.Equals(RolUsuario.Administrador.ToString(), StringComparison.OrdinalIgnoreCase) &&
            !rol.Equals(RolUsuario.Docente.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "No tenés permisos para gestionar opiniones.");
        }
    }

    private static OpinionUsuarioDTO Mapear(OpinionUsuario opinion) => new()
    {
        Id = opinion.Id,
        UsuarioId = opinion.UsuarioId,
        NombreVisible = opinion.Usuario is null
            ? string.Empty
            : $"{opinion.Usuario.Nombre} {opinion.Usuario.Apellido}",
        NombreUsuario = opinion.Usuario?.NombreUsuario ?? string.Empty,
        AvatarClave = opinion.Usuario?.AvatarClave ?? CatalogoAvataresPerfil.Predeterminado,
        FotoPerfilRuta = opinion.Usuario?.FotoPerfilRuta ?? string.Empty,
        Categoria = (int)opinion.Categoria,
        CategoriaTexto = ObtenerCategoriaTexto(opinion.Categoria),
        Calificacion = opinion.Calificacion,
        Comentario = opinion.Comentario,
        AutorizaPublicacion = opinion.AutorizaPublicacion,
        Estado = (int)opinion.Estado,
        EstadoTexto = ObtenerEstadoTexto(opinion.Estado),
        FechaCreacion = opinion.FechaCreacion,
        FechaRevision = opinion.FechaRevision
    };

    private static string ObtenerCategoriaTexto(CategoriaOpinion categoria) => categoria switch
    {
        CategoriaOpinion.ExperienciaGeneral => "Experiencia general",
        CategoriaOpinion.TutorIA => "Tutor IA",
        CategoriaOpinion.Ejercicios => "Ejercicios y correcciones",
        CategoriaOpinion.DisenoYAccesibilidad => "Diseño y accesibilidad",
        CategoriaOpinion.ReporteDeError => "Reporte de error",
        CategoriaOpinion.Sugerencia => "Sugerencia",
        _ => "Otra"
    };

    private static string ObtenerEstadoTexto(EstadoOpinion estado) => estado switch
    {
        EstadoOpinion.Pendiente => "Pendiente de revisión",
        EstadoOpinion.Revisada => "Revisada",
        EstadoOpinion.Considerada => "Considerada para una mejora",
        EstadoOpinion.Archivada => "Archivada",
        _ => "Sin estado"
    };
}
