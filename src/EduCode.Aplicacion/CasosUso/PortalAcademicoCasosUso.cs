using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CURegistrarDocenteInvitado
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioPortalAcademico _portal;
    private readonly IRepositorio<PerfilDocente> _perfiles;
    private readonly IRepositorioTokens _tokensRepositorio;
    private readonly IRepositorio<RegistroAuditoria> _auditoria;
    private readonly IServicioPassword _password;
    private readonly IServicioToken _tokens;
    private readonly IUnidadTrabajo _uow;

    public CURegistrarDocenteInvitado(
        IRepositorioUsuarios usuarios,
        IRepositorioPortalAcademico portal,
        IRepositorio<PerfilDocente> perfiles,
        IRepositorioTokens tokensRepositorio,
        IRepositorio<RegistroAuditoria> auditoria,
        IServicioPassword password,
        IServicioToken tokens,
        IUnidadTrabajo uow)
    {
        _usuarios = usuarios;
        _portal = portal;
        _perfiles = perfiles;
        _tokensRepositorio = tokensRepositorio;
        _auditoria = auditoria;
        _password = password;
        _tokens = tokens;
        _uow = uow;
    }

    public async Task<UsuarioAutenticadoDTO> EjecutarAsync(
        RegistroDocenteInvitadoDTO dto,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            throw new ReglaNegocioException("La contraseña debe tener al menos 8 caracteres.");
        if (!string.Equals(dto.Password, dto.ConfirmacionPassword, StringComparison.Ordinal))
            throw new ReglaNegocioException("Las contraseñas no coinciden.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ReglaNegocioException("El correo electrónico es obligatorio.");

        InvitacionDocente invitacion = await _portal.ObtenerInvitacionPorCodigoAsync(
            dto.CodigoInvitacion,
            ct) ?? throw new ReglaNegocioException("El código de invitación no existe.");

        if (!invitacion.PuedeUtilizarse(dto.Email))
            throw new ReglaNegocioException(
                "La invitación venció, ya fue utilizada o corresponde a otro correo.");
        if (await _usuarios.ExisteEmailAsync(dto.Email, ct))
            throw new ConflictoDatosException("El correo electrónico ya está registrado.");
        if (await _usuarios.ExisteNombreUsuarioAsync(dto.NombreUsuario, ct))
            throw new ConflictoDatosException("El nombre de usuario ya está registrado.");

        Usuario usuario = new(
            dto.Nombre,
            dto.Apellido,
            dto.NombreUsuario,
            dto.Email,
            _password.GenerarHash(dto.Password),
            RolUsuario.Docente);

        await _usuarios.AgregarAsync(usuario, ct);
        await _uow.GuardarCambiosAsync(ct);

        PerfilDocente perfil = new(usuario.Id, invitacion.Especialidad);
        perfil.ActualizarPresentacion(invitacion.Especialidad, dto.Biografia);
        perfil.Verificar();
        await _perfiles.AgregarAsync(perfil, ct);

        invitacion.MarcarUtilizada(usuario.Id);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            usuario.Id,
            TipoAccionAuditoria.Alta,
            nameof(PerfilDocente),
            usuario.Id.ToString(),
            string.Empty,
            $"Acceso docente activado mediante invitación {invitacion.Codigo}.",
            "registro-docente"), ct);

        TokenDTO token = _tokens.GenerarToken(usuario);
        await _tokensRepositorio.AgregarAsync(
            new TokenActualizacion(
                usuario.Id,
                token.RefreshToken,
                token.FechaExpiracionRefreshToken),
            ct);

        await _uow.GuardarCambiosAsync(ct);
        return MapearUsuario(usuario, token);
    }

    private static UsuarioAutenticadoDTO MapearUsuario(Usuario usuario, TokenDTO token) => new()
    {
        Id = usuario.Id,
        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
        NombreUsuario = usuario.NombreUsuario,
        Email = usuario.Email,
        Rol = usuario.Rol.ToString(),
        AvatarClave = usuario.AvatarClave,
        FotoPerfilRuta = usuario.FotoPerfilRuta ?? string.Empty,
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        FechaExpiracion = token.FechaExpiracion,
        FechaExpiracionRefreshToken = token.FechaExpiracionRefreshToken
    };
}

public sealed class CUInvitacionesDocentes
{
    private readonly IRepositorioPortalAcademico _portal;
    private readonly IRepositorio<InvitacionDocente> _invitaciones;
    private readonly IRepositorio<RegistroAuditoria> _auditoria;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUInvitacionesDocentes(
        IRepositorioPortalAcademico portal,
        IRepositorio<InvitacionDocente> invitaciones,
        IRepositorio<RegistroAuditoria> auditoria,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _portal = portal;
        _invitaciones = invitaciones;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public async Task<InvitacionDocenteDTO> CrearAsync(
        CrearInvitacionDocenteDTO dto,
        CancellationToken ct = default)
    {
        if (dto.DiasVigencia is < 1 or > 90)
            throw new ReglaNegocioException("La vigencia debe estar entre 1 y 90 días.");
        if (!string.IsNullOrWhiteSpace(dto.EmailPermitido) && !dto.EmailPermitido.Contains('@'))
            throw new ReglaNegocioException("El correo restringido no tiene un formato válido.");

        string codigoBase = $"EDU-{Guid.NewGuid():N}".ToUpperInvariant();
        string codigo = codigoBase[..16];
        InvitacionDocente invitacion = new(
            codigo,
            _usuarioActual.ObtenerUsuarioId(),
            string.IsNullOrWhiteSpace(dto.Especialidad) ? "Programación 1" : dto.Especialidad,
            DateTime.UtcNow.AddDays(dto.DiasVigencia),
            dto.EmailPermitido);

        await _invitaciones.AgregarAsync(invitacion, ct);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            _usuarioActual.ObtenerUsuarioId(),
            TipoAccionAuditoria.Alta,
            nameof(InvitacionDocente),
            codigo,
            string.Empty,
            $"Invitación docente creada para {invitacion.EmailPermitido ?? "cualquier correo"}.",
            "portal-administracion"), ct);
        await _uow.GuardarCambiosAsync(ct);
        return Mapear(invitacion);
    }

    public async Task RevocarAsync(int invitacionId, CancellationToken ct = default)
    {
        InvitacionDocente invitacion = await _invitaciones.ObtenerPorIdAsync(invitacionId, ct)
            ?? throw new EntidadNoEncontradaException("la invitación", invitacionId);
        invitacion.Revocar();
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            _usuarioActual.ObtenerUsuarioId(),
            TipoAccionAuditoria.Modificacion,
            nameof(InvitacionDocente),
            invitacion.Id.ToString(),
            EstadoInvitacionDocente.Disponible.ToString(),
            EstadoInvitacionDocente.Revocada.ToString(),
            "portal-administracion"), ct);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task<IReadOnlyCollection<InvitacionDocenteDTO>> ListarAsync(
        CancellationToken ct = default)
    {
        IReadOnlyCollection<InvitacionDocente> invitaciones =
            await _portal.ObtenerInvitacionesAsync(ct);
        return invitaciones.Select(Mapear).ToList();
    }

    private static InvitacionDocenteDTO Mapear(InvitacionDocente invitacion)
    {
        string estado = invitacion.Estado == EstadoInvitacionDocente.Disponible &&
                        invitacion.FechaExpiracion <= DateTime.UtcNow
            ? EstadoInvitacionDocente.Vencida.ToString()
            : invitacion.Estado.ToString();

        return new InvitacionDocenteDTO
        {
            Id = invitacion.Id,
            Codigo = invitacion.Codigo,
            Especialidad = invitacion.Especialidad,
            EmailPermitido = invitacion.EmailPermitido ?? "Cualquier correo",
            Estado = estado,
            FechaCreacion = invitacion.FechaCreacion,
            FechaExpiracion = invitacion.FechaExpiracion,
            UtilizadaPor = invitacion.UtilizadaPorUsuario is null
                ? string.Empty
                : $"{invitacion.UtilizadaPorUsuario.Nombre} {invitacion.UtilizadaPorUsuario.Apellido}"
        };
    }
}

public sealed class CUPortalDocente
{
    private readonly IRepositorioPortalAcademico _portal;
    private readonly IRepositorio<PropuestaEjercicio> _propuestas;
    private readonly IRepositorioTemas _temas;
    private readonly IRepositorio<RegistroAuditoria> _auditoria;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUPortalDocente(
        IRepositorioPortalAcademico portal,
        IRepositorio<PropuestaEjercicio> propuestas,
        IRepositorioTemas temas,
        IRepositorio<RegistroAuditoria> auditoria,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _portal = portal;
        _propuestas = propuestas;
        _temas = temas;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public Task<PanelDocenteDTO> ObtenerPanelAsync(CancellationToken ct = default) =>
        _portal.ObtenerPanelDocenteAsync(_usuarioActual.ObtenerUsuarioId(), ct);

    public async Task<IReadOnlyCollection<PropuestaEjercicioDTO>> ListarPropuestasAsync(
        CancellationToken ct = default)
    {
        IReadOnlyCollection<PropuestaEjercicio> propuestas =
            await _portal.ObtenerPropuestasDocenteAsync(
                _usuarioActual.ObtenerUsuarioId(),
                ct);
        return propuestas.Select(MapearPropuesta).ToList();
    }

    public async Task<PropuestaEjercicioDTO> CrearPropuestaAsync(
        CrearPropuestaEjercicioDTO dto,
        CancellationToken ct = default)
    {
        if (!await _temas.ExisteAsync(dto.TemaId, ct))
            throw new EntidadNoEncontradaException("el tema", dto.TemaId);

        PropuestaEjercicio propuesta = new(
            _usuarioActual.ObtenerUsuarioId(),
            dto.TemaId,
            dto.Titulo,
            dto.Consigna,
            dto.Dificultad,
            dto.CodigoInicial,
            dto.CriterioPrincipal,
            dto.OrientacionPista,
            dto.FundamentoPedagogico);

        await _propuestas.AgregarAsync(propuesta, ct);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            _usuarioActual.ObtenerUsuarioId(),
            TipoAccionAuditoria.Alta,
            nameof(PropuestaEjercicio),
            propuesta.Titulo,
            string.Empty,
            $"Propuesta enviada para el tema {dto.TemaId}.",
            "portal-docente"), ct);
        await _uow.GuardarCambiosAsync(ct);

        PropuestaEjercicio completa = await _portal.ObtenerPropuestaAsync(propuesta.Id, ct)
            ?? propuesta;
        return MapearPropuesta(completa);
    }

    public async Task<PropuestaEjercicioDTO> ActualizarPropuestaAsync(
        int propuestaId,
        CrearPropuestaEjercicioDTO dto,
        CancellationToken ct = default)
    {
        if (!await _temas.ExisteAsync(dto.TemaId, ct))
            throw new EntidadNoEncontradaException("el tema", dto.TemaId);

        PropuestaEjercicio propuesta = await _portal.ObtenerPropuestaAsync(propuestaId, ct)
            ?? throw new EntidadNoEncontradaException("la propuesta", propuestaId);
        int docenteId = _usuarioActual.ObtenerUsuarioId();
        propuesta.ActualizarParaReenvio(
            docenteId,
            dto.TemaId,
            dto.Titulo,
            dto.Consigna,
            dto.Dificultad,
            dto.CodigoInicial,
            dto.CriterioPrincipal,
            dto.OrientacionPista,
            dto.FundamentoPedagogico);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            docenteId,
            TipoAccionAuditoria.Modificacion,
            nameof(PropuestaEjercicio),
            propuesta.Id.ToString(),
            EstadoPropuestaEjercicio.RequiereAjustes.ToString(),
            EstadoPropuestaEjercicio.Enviada.ToString(),
            "portal-docente"), ct);
        await _uow.GuardarCambiosAsync(ct);
        return MapearPropuesta(propuesta);
    }

    internal static PropuestaEjercicioDTO MapearPropuesta(PropuestaEjercicio p) => new()
    {
        Id = p.Id,
        TemaId = p.TemaId,
        Tema = p.Tema?.Nombre ?? string.Empty,
        DocenteId = p.DocenteId,
        Docente = p.Docente is null ? string.Empty : $"{p.Docente.Nombre} {p.Docente.Apellido}",
        Titulo = p.Titulo,
        Consigna = p.Consigna,
        Dificultad = p.Dificultad.ToString(),
        CodigoInicial = p.CodigoInicial,
        CriterioPrincipal = p.CriterioPrincipal,
        OrientacionPista = p.OrientacionPista,
        FundamentoPedagogico = p.FundamentoPedagogico,
        Estado = p.Estado.ToString(),
        FechaCreacion = p.FechaCreacion,
        FechaRevision = p.FechaRevision,
        ObservacionRevision = p.ObservacionRevision,
        EjercicioPublicadoId = p.EjercicioPublicadoId
    };
}

public sealed class CUPortalAdministracion
{
    private readonly IRepositorioPortalAcademico _portal;
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorio<Grupo> _grupos;
    private readonly IRepositorio<InscripcionCurso> _inscripciones;
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioCursos _cursos;
    private readonly IRepositorio<PeriodoAcademico> _periodos;
    private readonly IRepositorio<RegistroAuditoria> _auditoria;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUPortalAdministracion(
        IRepositorioPortalAcademico portal,
        IRepositorioEjercicios ejercicios,
        IRepositorio<Grupo> grupos,
        IRepositorio<InscripcionCurso> inscripciones,
        IRepositorioUsuarios usuarios,
        IRepositorioCursos cursos,
        IRepositorio<PeriodoAcademico> periodos,
        IRepositorio<RegistroAuditoria> auditoria,
        IServicioUsuarioActual usuarioActual,
        IUnidadTrabajo uow)
    {
        _portal = portal;
        _ejercicios = ejercicios;
        _grupos = grupos;
        _inscripciones = inscripciones;
        _usuarios = usuarios;
        _cursos = cursos;
        _periodos = periodos;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public Task<PanelAdministracionDTO> ObtenerPanelAsync(CancellationToken ct = default) =>
        _portal.ObtenerPanelAdministracionAsync(ct);

    public Task<EstructuraAcademicaDTO> ObtenerEstructuraAsync(CancellationToken ct = default) =>
        _portal.ObtenerEstructuraAcademicaAsync(ct);

    public async Task<IReadOnlyCollection<PropuestaEjercicioDTO>> ListarPropuestasAsync(
        CancellationToken ct = default)
    {
        IReadOnlyCollection<PropuestaEjercicio> propuestas =
            await _portal.ObtenerPropuestasAdministracionAsync(ct);
        return propuestas.Select(CUPortalDocente.MapearPropuesta).ToList();
    }

    public async Task<PropuestaEjercicioDTO> RevisarPropuestaAsync(
        int propuestaId,
        RevisarPropuestaEjercicioDTO dto,
        CancellationToken ct = default)
    {
        PropuestaEjercicio propuesta = await _portal.ObtenerPropuestaAsync(propuestaId, ct)
            ?? throw new EntidadNoEncontradaException("la propuesta", propuestaId);

        if (propuesta.Estado is EstadoPropuestaEjercicio.Aprobada or EstadoPropuestaEjercicio.Rechazada)
            throw new ReglaNegocioException("La propuesta ya tiene una decisión final.");

        string decision = (dto.Decision ?? string.Empty).Trim().ToLowerInvariant();
        int adminId = _usuarioActual.ObtenerUsuarioId();

        if (decision == "aprobar")
        {
            IReadOnlyCollection<Ejercicio> existentes =
                await _ejercicios.ObtenerPorTemaAsync(propuesta.TemaId, ct);
            int orden = existentes.Count == 0 ? 1 : existentes.Max(e => e.Orden) + 1;

            Ejercicio ejercicio = new(
                propuesta.Titulo,
                propuesta.Consigna,
                TipoEjercicio.Codigo,
                propuesta.Dificultad,
                100,
                70,
                orden,
                "JavaScript",
                propuesta.CodigoInicial,
                propuesta.TemaId,
                adminId);

            ejercicio.ConfigurarAyudas(4, true, false);
            ejercicio.ConfigurarVersionRubrica($"propuesta-docente-{DateTime.UtcNow:yyyyMMdd}-3.0");
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0, propuesta.CriterioPrincipal, TipoCriterio.Funcional, 40, true, 1, 70));
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0, "Aplica correctamente los conceptos centrales del tema.", TipoCriterio.Conceptual, 25, true, 2, 65));
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0, "Contempla entradas habituales y casos borde relevantes.", TipoCriterio.CasoPrueba, 20, false, 3));
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0, "La solución es clara, coherente y adecuada al nivel de Programación 1.", TipoCriterio.CalidadCodigo, 15, false, 4));

            if (!string.IsNullOrWhiteSpace(propuesta.OrientacionPista))
            {
                ejercicio.AgregarPista(new PistaEjercicio(
                    0,
                    1,
                    propuesta.OrientacionPista,
                    1,
                    0));
            }

            await _ejercicios.AgregarAsync(ejercicio, ct);
            await _uow.GuardarCambiosAsync(ct);
            propuesta.Aprobar(adminId, ejercicio.Id, dto.Observacion);
        }
        else if (decision is "ajustes" or "solicitar-ajustes")
        {
            propuesta.SolicitarAjustes(adminId, dto.Observacion);
        }
        else if (decision == "rechazar")
        {
            propuesta.Rechazar(adminId, dto.Observacion);
        }
        else
        {
            throw new ReglaNegocioException("La decisión debe ser aprobar, ajustes o rechazar.");
        }

        await _auditoria.AgregarAsync(new RegistroAuditoria(
            adminId,
            decision == "aprobar"
                ? TipoAccionAuditoria.PublicacionContenido
                : TipoAccionAuditoria.Modificacion,
            nameof(PropuestaEjercicio),
            propuesta.Id.ToString(),
            string.Empty,
            $"Decisión editorial: {decision}. {dto.Observacion}".Trim(),
            "portal-administracion"), ct);
        await _uow.GuardarCambiosAsync(ct);
        return CUPortalDocente.MapearPropuesta(propuesta);
    }

    public async Task<int> CrearGrupoAsync(
        CrearGrupoDTO dto,
        CancellationToken ct = default)
    {
        Usuario docente = await _usuarios.ObtenerPorIdAsync(dto.DocenteId, ct)
            ?? throw new EntidadNoEncontradaException("el docente", dto.DocenteId);
        if (docente.Rol != RolUsuario.Docente)
            throw new ReglaNegocioException("El usuario seleccionado no tiene rol Docente.");
        if (!await _cursos.ExisteAsync(dto.CursoId, ct))
            throw new EntidadNoEncontradaException("el curso", dto.CursoId);
        if (!await _periodos.ExisteAsync(dto.PeriodoAcademicoId, ct))
            throw new EntidadNoEncontradaException("el período académico", dto.PeriodoAcademicoId);
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ReglaNegocioException("El nombre del grupo es obligatorio.");

        Grupo grupo = new(
            dto.Nombre,
            dto.CursoId,
            dto.PeriodoAcademicoId,
            dto.DocenteId);
        await _grupos.AgregarAsync(grupo, ct);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            _usuarioActual.ObtenerUsuarioId(),
            TipoAccionAuditoria.Alta,
            nameof(Grupo),
            dto.Nombre,
            string.Empty,
            $"Grupo creado para docente {dto.DocenteId}, curso {dto.CursoId} y período {dto.PeriodoAcademicoId}.",
            "portal-administracion"), ct);
        await _uow.GuardarCambiosAsync(ct);
        return grupo.Id;
    }

    public async Task InscribirAlumnoAsync(
        int grupoId,
        InscribirAlumnoDTO dto,
        CancellationToken ct = default)
    {
        Grupo grupo = await _portal.ObtenerGrupoAsync(grupoId, ct)
            ?? throw new EntidadNoEncontradaException("el grupo", grupoId);
        Usuario alumno = await _usuarios.ObtenerPorIdAsync(dto.AlumnoId, ct)
            ?? throw new EntidadNoEncontradaException("el alumno", dto.AlumnoId);
        if (alumno.Rol != RolUsuario.Alumno)
            throw new ReglaNegocioException("Solo se pueden asociar usuarios con rol Alumno.");
        if (await _portal.ExisteInscripcionAsync(dto.AlumnoId, grupo.Id, ct))
            throw new ConflictoDatosException("El alumno ya pertenece a ese grupo.");

        await _inscripciones.AgregarAsync(new InscripcionCurso(dto.AlumnoId, grupo.Id), ct);
        await _auditoria.AgregarAsync(new RegistroAuditoria(
            _usuarioActual.ObtenerUsuarioId(),
            TipoAccionAuditoria.Alta,
            nameof(InscripcionCurso),
            $"{dto.AlumnoId}:{grupo.Id}",
            string.Empty,
            $"Alumno {dto.AlumnoId} asociado al grupo {grupo.Nombre}.",
            "portal-administracion"), ct);
        await _uow.GuardarCambiosAsync(ct);
    }
}
