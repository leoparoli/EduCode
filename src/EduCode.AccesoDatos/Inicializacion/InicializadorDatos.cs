using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos.Inicializacion;

public static class InicializadorDatos
{
    public static async Task InicializarAsync(
        EduCodeContext context,
        IServicioPassword servicioPassword,
        CancellationToken ct = default)
    {
        await context.Database.EnsureCreatedAsync(ct);
        await ActualizadorEsquemaPerfilOpiniones.AplicarAsync(context, ct);
        await ActualizadorEsquemaPortalAcademico.AplicarAsync(context, ct);

        Usuario administrador = await ObtenerOCrearUsuarioAsync(
            context, servicioPassword, "Administrador", "EduCode", "admin",
            "admin@educode.local", "Admin123!", RolUsuario.Administrador, ct);

        Usuario alumno = await ObtenerOCrearUsuarioAsync(
            context, servicioPassword, "Alumno", "Demostración", "alumno",
            "alumno@educode.local", "Alumno123!", RolUsuario.Alumno, ct);

        Usuario docente = await ObtenerOCrearUsuarioAsync(
            context, servicioPassword, "Docente", "Demostración", "docente",
            "docente@educode.local", "Docente123!", RolUsuario.Docente, ct);

        Curso curso = await context.Cursos.FirstOrDefaultAsync(
            c => c.Codigo == "P1", ct) ?? new Curso(
                "Programación 1", "P1",
                "Recorrido progresivo de JavaScript, organizado por conceptos, dificultad y rúbricas granulares.");

        if (curso.Id == 0)
        {
            context.Cursos.Add(curso);
            await context.SaveChangesAsync(ct);
        }

        await ObtenerOCrearEstructuraDocenteAsync(
            context,
            administrador,
            docente,
            alumno,
            curso,
            ct);

        Dictionary<string, Tema> temas = await ObtenerOCrearTemasAsync(context, curso.Id, ct);
        Dictionary<string, Ejercicio> creados = [];

        foreach (DefinicionEjercicio definicion in ObtenerDefiniciones())
        {
            Tema tema = temas[definicion.Tema];
            string clavePrerequisito = definicion.Orden switch
            {
                7 => $"{definicion.Tema}|5",
                8 => $"{definicion.Tema}|7",
                _ => string.Empty
            };

            int? prerequisitoId = creados.TryGetValue(clavePrerequisito, out Ejercicio? previo)
                ? previo.Id
                : null;

            Ejercicio ejercicio = await CrearEjercicioSiNoExisteAsync(
                context, administrador.Id, tema, definicion, prerequisitoId, ct);

            creados[$"{definicion.Tema}|{definicion.Orden}"] = ejercicio;
        }

        await ObtenerOCrearLogrosAsync(context, ct);
    }

    private static async Task ObtenerOCrearEstructuraDocenteAsync(
        EduCodeContext context,
        Usuario administrador,
        Usuario docente,
        Usuario alumno,
        Curso curso,
        CancellationToken ct)
    {
        PerfilDocente? perfil = await context.PerfilesDocentes
            .FirstOrDefaultAsync(p => p.UsuarioId == docente.Id, ct);
        if (perfil is null)
        {
            perfil = new PerfilDocente(docente.Id, "Programación 1 y pensamiento computacional");
            perfil.Verificar();
            context.PerfilesDocentes.Add(perfil);
        }
        else if (!perfil.Verificado)
        {
            perfil.Verificar();
        }

        PeriodoAcademico periodo = await context.PeriodosAcademicos
            .FirstOrDefaultAsync(p => p.Anio == 2026 && p.Semestre == Semestre.Segundo, ct)
            ?? new PeriodoAcademico(
                2026,
                Semestre.Segundo,
                new DateTime(2026, 7, 1),
                new DateTime(2026, 12, 20));

        if (periodo.Id == 0)
            context.PeriodosAcademicos.Add(periodo);

        await context.SaveChangesAsync(ct);

        Grupo grupo = await context.Grupos
            .FirstOrDefaultAsync(g => g.Nombre == "P1 · Grupo demostración", ct)
            ?? new Grupo("P1 · Grupo demostración", curso.Id, periodo.Id, docente.Id);

        if (grupo.Id == 0)
        {
            context.Grupos.Add(grupo);
            await context.SaveChangesAsync(ct);
        }

        bool inscripto = await context.InscripcionesCursos.AnyAsync(
            i => i.UsuarioId == alumno.Id && i.GrupoId == grupo.Id,
            ct);
        if (!inscripto)
            context.InscripcionesCursos.Add(new InscripcionCurso(alumno.Id, grupo.Id));

        bool invitacionDemoExiste = await context.InvitacionesDocentes.AnyAsync(
            i => i.Codigo == "DOCENTE-DEMO-3-0",
            ct);
        if (!invitacionDemoExiste)
        {
            context.InvitacionesDocentes.Add(new InvitacionDocente(
                "DOCENTE-DEMO-3-0",
                administrador.Id,
                "Programación 1",
                new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task ObtenerOCrearLogrosAsync(
        EduCodeContext context,
        CancellationToken ct)
    {
        var definiciones = new[]
        {
            new LogroDef(
                "NACE UN PROGRAMADOR",
                "Aprobaste tu primer ejercicio. Todo recorrido empieza con una solución que funciona.",
                "🌱",
                TipoLogro.EjerciciosCompletados,
                1),
            new LogroDef(
                "PRIMER IMPULSO",
                "Ya aprobaste 3 ejercicios. El hábito empieza a tomar forma.",
                "🧩",
                TipoLogro.EjerciciosCompletados,
                3),
            new LogroDef(
                "LA PRÁCTICA HACE AL PROGRAMADOR",
                "Aprobaste 5 ejercicios. La constancia ya empieza a convertirse en dominio.",
                "🛠️",
                TipoLogro.EjerciciosCompletados,
                5),
            new LogroDef(
                "EN MARCHA",
                "Aprobaste 8 ejercicios. Ya existe un ritmo real de aprendizaje.",
                "👣",
                TipoLogro.EjerciciosCompletados,
                8),
            new LogroDef(
                "CÓDIGO EN MOVIMIENTO",
                "Aprobaste 10 ejercicios distintos dentro del recorrido.",
                "⚙️",
                TipoLogro.EjerciciosCompletados,
                10),
            new LogroDef(
                "CONSTRUYENDO HÁBITO",
                "Aprobaste 15 ejercicios. Lo que antes costaba, ahora empieza a salir con naturalidad.",
                "📚",
                TipoLogro.EjerciciosCompletados,
                15),
            new LogroDef(
                "MODO CONSTANTE",
                "Aprobaste 20 ejercicios. Tu progreso ya no es casualidad.",
                "🚀",
                TipoLogro.EjerciciosCompletados,
                20),
            new LogroDef(
                "MAPA DEL RECORRIDO",
                "Aprobaste 30 ejercicios. Ya recorriste una parte enorme del camino.",
                "🗺️",
                TipoLogro.EjerciciosCompletados,
                30),
            new LogroDef(
                "CASI AL FINAL",
                "Aprobaste 40 ejercicios. La meta ya se ve de cerca.",
                "🏁",
                TipoLogro.EjerciciosCompletados,
                40),
            new LogroDef(
                "RECORRIDO DOMINADO",
                "Aprobaste los 48 ejercicios del recorrido principal de Programación 1.",
                "🏆",
                TipoLogro.EjerciciosCompletados,
                48),
            new LogroDef(
                "DOBLE ACIERTO",
                "Encadenaste 2 ejercicios aprobados. Ya hay continuidad en tu avance.",
                "✨",
                TipoLogro.RachaEjercicios,
                2),
            new LogroDef(
                "EN RACHA",
                "Encadenaste 3 ejercicios aprobados sin una entrega fallida entre ellos.",
                "🔥",
                TipoLogro.RachaEjercicios,
                3),
            new LogroDef(
                "FLUJO CONTINUO",
                "Encadenaste 5 ejercicios aprobados de forma consecutiva.",
                "🌊",
                TipoLogro.RachaEjercicios,
                5),
            new LogroDef(
                "PASO FIRME",
                "Lograste una racha de 7 ejercicios aprobados. Tu constancia se nota.",
                "🥾",
                TipoLogro.RachaEjercicios,
                7),
            new LogroDef(
                "IMPARABLE",
                "Alcanzaste una racha de 10 ejercicios aprobados consecutivos.",
                "⚡",
                TipoLogro.RachaEjercicios,
                10),
            new LogroDef(
                "PRECISIÓN SOSTENIDA",
                "Mantuviste una racha de 15 ejercicios aprobados. Es un nivel de constancia superior.",
                "🎯",
                TipoLogro.RachaEjercicios,
                15),
            new LogroDef(
                "LEYENDA DE LA CONSTANCIA",
                "Alcanzaste una racha de 20 ejercicios correctos. Tu disciplina ya es una marca propia.",
                "👑",
                TipoLogro.RachaEjercicios,
                20),
            new LogroDef(
                "TOMA DE IMPULSO",
                "La suma de tus mejores notas alcanzó 300 puntos.",
                "🪜",
                TipoLogro.PuntajeObtenido,
                300),
            new LogroDef(
                "BASE SÓLIDA",
                "La suma de tus mejores notas alcanzó 750 puntos.",
                "🧱",
                TipoLogro.PuntajeObtenido,
                750),
            new LogroDef(
                "MIL PUNTOS DE CÓDIGO",
                "La suma de tus mejores notas alcanzó 1000 puntos.",
                "💻",
                TipoLogro.PuntajeObtenido,
                1000),
            new LogroDef(
                "MENTE ALGORÍTMICA",
                "La suma de tus mejores notas alcanzó 1500 puntos.",
                "🧠",
                TipoLogro.PuntajeObtenido,
                1500),
            new LogroDef(
                "NIVEL EN ASCENSO",
                "La suma de tus mejores notas alcanzó 2000 puntos.",
                "📈",
                TipoLogro.PuntajeObtenido,
                2000),
            new LogroDef(
                "ARQUITECTO DEL RECORRIDO",
                "La suma de tus mejores notas alcanzó 2500 puntos.",
                "🏛️",
                TipoLogro.PuntajeObtenido,
                2500),
            new LogroDef(
                "ÉLITE DEL RECORRIDO",
                "La suma de tus mejores notas alcanzó 3500 puntos.",
                "🌟",
                TipoLogro.PuntajeObtenido,
                3500),
            new LogroDef(
                "MAESTRÍA CASI TOTAL",
                "La suma de tus mejores notas alcanzó 4500 puntos. El recorrido está prácticamente completo.",
                "🏅",
                TipoLogro.PuntajeObtenido,
                4500)
        };

        foreach (LogroDef def in definiciones)
        {
            bool existe = await context.Logros.AnyAsync(
                logro => logro.Nombre == def.Nombre,
                ct);

            if (!existe)
            {
                context.Logros.Add(new Logro(
                    def.Nombre,
                    def.Descripcion,
                    def.Icono,
                    def.Tipo,
                    def.ValorObjetivo));
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task<Usuario> ObtenerOCrearUsuarioAsync(
        EduCodeContext context, IServicioPassword passwords, string nombre,
        string apellido, string usuario, string email, string password,
        RolUsuario rol, CancellationToken ct)
    {
        Usuario? existente = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existente is not null) return existente;

        Usuario nuevo = new(nombre, apellido, usuario, email, passwords.GenerarHash(password), rol);
        context.Usuarios.Add(nuevo);
        await context.SaveChangesAsync(ct);
        return nuevo;
    }

    private static async Task<Dictionary<string, Tema>> ObtenerOCrearTemasAsync(
        EduCodeContext context, int cursoId, CancellationToken ct)
    {
        TemaDef[] definiciones =
        [
            new("Variables y operadores", "Tipos de datos, asignación, operadores aritméticos y expresiones.", 1),
            new("Condicionales", "Decisiones con if, else, rangos y operadores lógicos.", 2),
            new("Bucles", "Repetición controlada, acumuladores, contadores y recorridos.", 3),
            new("Strings", "Recorrido, normalización, búsqueda y validación de textos.", 4),
            new("Array", "Creación, acceso por índice, longitud, recorrido y búsqueda de elementos.", 5),
            new("Lista", "Incorporación, eliminación, actualización y organización de elementos en una colección.", 6),
            new("Funciones", "Parámetros, retorno, reutilización y descomposición de problemas.", 7),
            new("Clases y objetos", "Modelado de entidades, estado, comportamiento y colaboración entre objetos.", 8),
        ];

        List<Tema> existentes = await context.Temas
            .Where(tema => tema.CursoId == cursoId)
            .OrderBy(tema => tema.Orden)
            .ThenBy(tema => tema.Id)
            .ToListAsync(ct);

        bool temaLegadoAjustado = false;
        Tema? temaLegado = existentes.FirstOrDefault(tema =>
            tema.Activo &&
            (tema.Nombre.Equals("Arrays y listas", StringComparison.OrdinalIgnoreCase) ||
             tema.Nombre.Equals("Array y lista", StringComparison.OrdinalIgnoreCase)));
        if (temaLegado is not null)
        {
            bool existeArray = existentes.Any(tema =>
                tema.Id != temaLegado.Id &&
                tema.Nombre.Equals("Array", StringComparison.OrdinalIgnoreCase));
            bool existeLista = existentes.Any(tema =>
                tema.Id != temaLegado.Id &&
                tema.Nombre.Equals("Lista", StringComparison.OrdinalIgnoreCase));

            if (!existeArray)
            {
                temaLegado.Editar(
                    "Array",
                    "Creación, acceso por índice, longitud, recorrido y búsqueda de elementos.",
                    temaLegado.Orden);
            }
            else if (!existeLista)
            {
                temaLegado.Editar(
                    "Lista",
                    "Incorporación, eliminación, actualización y organización de elementos en una colección.",
                    temaLegado.Orden);
            }
            else
            {
                temaLegado.Desactivar();
            }

            temaLegadoAjustado = true;
        }

        Dictionary<int, int> ordenOriginal = existentes.ToDictionary(
            tema => tema.Id,
            tema => tema.Orden);
        HashSet<string> nombresCanonicos = definiciones
            .Select(def => def.Nombre)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool faltaTemaCanonico = definiciones.Any(def =>
            !existentes.Any(tema =>
                tema.Activo &&
                tema.Nombre.Equals(def.Nombre, StringComparison.OrdinalIgnoreCase)));
        bool hayOrdenCanonicoIncorrecto = definiciones.Any(def =>
            existentes.Any(tema =>
                tema.Activo &&
                tema.Nombre.Equals(def.Nombre, StringComparison.OrdinalIgnoreCase) &&
                tema.Orden != def.Orden));
        bool hayTemaPersonalizadoEnRangoReservado = existentes.Any(tema =>
            tema.Activo &&
            !nombresCanonicos.Contains(tema.Nombre) &&
            tema.Orden <= definiciones.Max(def => def.Orden));

        bool requiereReorden = temaLegadoAjustado ||
            faltaTemaCanonico ||
            hayOrdenCanonicoIncorrecto ||
            hayTemaPersonalizadoEnRangoReservado;

        if (requiereReorden)
        {
            // El orden temporal evita colisiones con el índice único CursoId + Orden
            // mientras se insertan Array y Lista entre temas de una base existente.
            foreach (Tema tema in existentes)
            {
                tema.Editar(tema.Nombre, tema.Descripcion, 100000 + tema.Id);
            }

            await context.SaveChangesAsync(ct);
        }

        Dictionary<string, Tema> resultado =
            new(StringComparer.OrdinalIgnoreCase);
        foreach (TemaDef def in definiciones)
        {
            Tema? tema = existentes.FirstOrDefault(item =>
                item.Activo && item.Nombre.Equals(def.Nombre, StringComparison.OrdinalIgnoreCase));
            if (tema is null)
            {
                tema = new Tema(def.Nombre, def.Descripcion, def.Orden, cursoId);
                context.Temas.Add(tema);
                existentes.Add(tema);
            }
            else
            {
                tema.Editar(def.Nombre, def.Descripcion, def.Orden);
            }

            resultado[def.Nombre] = tema;
        }

        int siguienteOrden = definiciones.Max(def => def.Orden) + 1;
        foreach (Tema tema in existentes
            .Where(tema => tema.Activo && !nombresCanonicos.Contains(tema.Nombre))
            .OrderBy(tema => ordenOriginal.GetValueOrDefault(tema.Id, int.MaxValue))
            .ThenBy(tema => tema.Id))
        {
            tema.Editar(tema.Nombre, tema.Descripcion, siguienteOrden++);
        }

        await context.SaveChangesAsync(ct);
        return resultado;
    }

    private static async Task<Ejercicio> CrearEjercicioSiNoExisteAsync(
        EduCodeContext context, int usuarioCreadorId, Tema tema,
        DefinicionEjercicio def, int? prerequisitoId, CancellationToken ct)
    {
        Ejercicio? existente = await context.Ejercicios
            .Include(e => e.CriteriosEvaluacion)
            .Include(e => e.Pistas)
            .Include(e => e.CasosPrueba)
            .Include(e => e.ConceptosPermitidos)
            .Include(e => e.ConceptosNoPermitidos)
            .FirstOrDefaultAsync(
                e => e.TemaId == tema.Id && e.Orden == def.Orden,
                ct);

        if (existente is not null)
        {
            SincronizarEjercicioExistente(
                existente,
                def,
                prerequisitoId);
            await context.SaveChangesAsync(ct);
            return existente;
        }

        Ejercicio ejercicio = new(
            def.Titulo, def.Consigna, TipoEjercicio.Codigo, def.Dificultad,
            100, 70, def.Orden, "JavaScript", def.CodigoInicial,
            tema.Id, usuarioCreadorId, prerequisitoId);
        ejercicio.ConfigurarVersionRubrica($"{Slug(def.Titulo)}-2.5");

        (decimal[] pesos, int[] minimos) = ConfiguracionRubrica(def.Dificultad);
        string[] descripciones = ConstruirDescripcionesRubrica(def);
        TipoCriterio[] tipos =
        [
            TipoCriterio.Funcional, TipoCriterio.Conceptual,
            TipoCriterio.CasoPrueba, TipoCriterio.Sintactico,
            TipoCriterio.CalidadCodigo
        ];

        for (int i = 0; i < descripciones.Length; i++)
        {
            bool obligatorio = i < 2;
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0, descripciones[i], tipos[i], pesos[i], obligatorio,
                i + 1, obligatorio ? minimos[i] : 0));
        }

        for (int i = 0; i < def.Pistas.Length; i++)
        {
            ejercicio.AgregarPista(new PistaEjercicio(
                0, i + 1, def.Pistas[i], i + 1, 0));
        }

        foreach (string concepto in def.Conceptos)
        {
            ejercicio.AgregarConceptoPermitido(new ConceptoPermitido(
                0, concepto, "Concepto esperado para resolver este ejercicio."));
        }

        foreach (string concepto in def.ConceptosNoPermitidos ?? [])
        {
            ejercicio.AgregarConceptoNoPermitido(new ConceptoNoPermitido(
                0,
                concepto,
                "La consigna solicita resolver el ejercicio sin utilizar este recurso."));
        }

        int ordenCaso = 1;
        foreach (CasoDef caso in def.Casos)
        {
            ejercicio.AgregarCasoPrueba(new CasoPrueba(
                0, caso.Nombre, caso.Entrada, caso.SalidaEsperada,
                ordenCaso <= 2 ? VisibilidadCasoPrueba.Publico : VisibilidadCasoPrueba.Oculto,
                0, ordenCaso++));
        }

        context.Ejercicios.Add(ejercicio);
        await context.SaveChangesAsync(ct);
        return ejercicio;
    }

    private static void SincronizarEjercicioExistente(
        Ejercicio ejercicio,
        DefinicionEjercicio def,
        int? prerequisitoId)
    {
        ejercicio.ActualizarContenidoAcademico(
            def.Titulo,
            def.Consigna,
            def.Dificultad,
            def.Orden,
            def.CodigoInicial,
            prerequisitoId);
        ejercicio.ConfigurarVersionRubrica($"{Slug(def.Titulo)}-2.5");

        (decimal[] pesos, int[] minimos) = ConfiguracionRubrica(def.Dificultad);
        string[] descripciones = ConstruirDescripcionesRubrica(def);
        TipoCriterio[] tipos =
        [
            TipoCriterio.Funcional,
            TipoCriterio.Conceptual,
            TipoCriterio.CasoPrueba,
            TipoCriterio.Sintactico,
            TipoCriterio.CalidadCodigo
        ];

        for (int i = 0; i < descripciones.Length; i++)
        {
            bool obligatorio = i < 2;
            CriterioEvaluacion? criterio = ejercicio.CriteriosEvaluacion
                .FirstOrDefault(c => c.Orden == i + 1);

            if (criterio is null)
            {
                ejercicio.AgregarCriterio(new CriterioEvaluacion(
                    ejercicio.Id,
                    descripciones[i],
                    tipos[i],
                    pesos[i],
                    obligatorio,
                    i + 1,
                    obligatorio ? minimos[i] : 0));
            }
            else
            {
                criterio.ActualizarContenidoAcademico(
                    descripciones[i],
                    tipos[i],
                    pesos[i],
                    obligatorio,
                    i + 1,
                    obligatorio ? minimos[i] : 0);
            }
        }

        for (int i = 0; i < def.Pistas.Length; i++)
        {
            int nivel = i + 1;
            PistaEjercicio? pista = ejercicio.Pistas
                .FirstOrDefault(p => p.Nivel == nivel);

            if (pista is null)
            {
                ejercicio.AgregarPista(new PistaEjercicio(
                    ejercicio.Id,
                    nivel,
                    def.Pistas[i],
                    nivel,
                    0));
            }
            else
            {
                pista.ActualizarContenidoAcademico(
                    nivel,
                    def.Pistas[i],
                    nivel,
                    0);
            }
        }

        int ordenCaso = 1;
        foreach (CasoDef casoDef in def.Casos)
        {
            CasoPrueba? caso = ejercicio.CasosPrueba
                .FirstOrDefault(c => c.Orden == ordenCaso);
            VisibilidadCasoPrueba visibilidad = ordenCaso <= 2
                ? VisibilidadCasoPrueba.Publico
                : VisibilidadCasoPrueba.Oculto;

            if (caso is null)
            {
                ejercicio.AgregarCasoPrueba(new CasoPrueba(
                    ejercicio.Id,
                    casoDef.Nombre,
                    casoDef.Entrada,
                    casoDef.SalidaEsperada,
                    visibilidad,
                    0,
                    ordenCaso));
            }
            else
            {
                caso.ActualizarContenidoAcademico(
                    casoDef.Nombre,
                    casoDef.Entrada,
                    casoDef.SalidaEsperada,
                    visibilidad,
                    0,
                    ordenCaso);
            }

            ordenCaso++;
        }

        SincronizarConceptosPermitidos(ejercicio, def.Conceptos);
        SincronizarConceptosNoPermitidos(
            ejercicio,
            def.ConceptosNoPermitidos ?? []);
    }

    private static void SincronizarConceptosPermitidos(
        Ejercicio ejercicio,
        IReadOnlyList<string> conceptos)
    {
        for (int i = 0; i < conceptos.Count; i++)
        {
            string nombre = conceptos[i];
            ConceptoPermitido? concepto = ejercicio.ConceptosPermitidos
                .OrderBy(c => c.Id)
                .ElementAtOrDefault(i);

            if (concepto is null)
            {
                ejercicio.AgregarConceptoPermitido(new ConceptoPermitido(
                    ejercicio.Id,
                    nombre,
                    "Concepto esperado para resolver este ejercicio."));
            }
            else
            {
                concepto.ActualizarContenidoAcademico(
                    nombre,
                    "Concepto esperado para resolver este ejercicio.");
            }
        }
    }

    private static void SincronizarConceptosNoPermitidos(
        Ejercicio ejercicio,
        IReadOnlyList<string> conceptos)
    {
        for (int i = 0; i < conceptos.Count; i++)
        {
            string nombre = conceptos[i];
            ConceptoNoPermitido? concepto = ejercicio.ConceptosNoPermitidos
                .OrderBy(c => c.Id)
                .ElementAtOrDefault(i);

            if (concepto is null)
            {
                ejercicio.AgregarConceptoNoPermitido(new ConceptoNoPermitido(
                    ejercicio.Id,
                    nombre,
                    "La consigna solicita resolver el ejercicio sin utilizar este recurso."));
            }
            else
            {
                concepto.ActualizarContenidoAcademico(
                    nombre,
                    "La consigna solicita resolver el ejercicio sin utilizar este recurso.");
            }
        }
    }

    private static string[] ConstruirDescripcionesRubrica(
        DefinicionEjercicio def)
    {
        (string casos, string estructura, string calidad) = def.Tema switch
        {
            "Variables y operadores" => (
                "Produce el resultado esperado para valores habituales, límites y valores decimales configurados.",
                "La expresión, los operadores y el return son coherentes con el cálculo solicitado.",
                "La solución es directa, clara y no agrega complejidad innecesaria al cálculo."),
            "Condicionales" => (
                "Resuelve correctamente cada rama y los valores límite definidos por la consigna.",
                "Las condiciones son excluyentes, están ordenadas correctamente y todos los caminos retornan un resultado.",
                "La solución expresa las decisiones con claridad y evita condiciones redundantes."),
            "Bucles" => (
                "Recorre la cantidad correcta de elementos o iteraciones y contempla los casos borde.",
                "La inicialización, la condición, la actualización y los acumuladores son coherentes con el recorrido.",
                "La solución utiliza un recorrido comprensible, sin iteraciones innecesarias ni accesos fuera de rango."),
            "Strings" => (
                "Procesa correctamente textos vacíos, caracteres límite y variantes de mayúsculas o minúsculas cuando corresponde.",
                "El recorrido y las operaciones sobre índices o caracteres respetan el rango válido del string.",
                "La solución mantiene una lectura clara del texto y evita transformaciones ajenas a la consigna."),
            "Array" => (
                "Procesa arrays vacíos, de un elemento y con valores repetidos sin acceder fuera de rango.",
                "Los índices, la longitud, el recorrido y la búsqueda respetan la estructura del array.",
                "La solución evita recorridos innecesarios y expresa con claridad qué elemento o posición está utilizando."),
            "Lista" => (
                "Resuelve correctamente inserciones, eliminaciones, búsquedas y repeticiones, incluida la lista vacía.",
                "Las operaciones modifican únicamente los elementos solicitados y mantienen una colección coherente.",
                "La solución distingue con claridad entre consultar, agregar, quitar y actualizar elementos de la lista."),
            "Funciones" => (
                "Respeta el contrato de entradas y salida para los escenarios de referencia y casos borde.",
                "Los parámetros, el retorno y la descomposición de la función son coherentes con su responsabilidad.",
                "La función es reutilizable, legible y no exige pasos adicionales que no aportan al resultado."),
            "Clases y objetos" => (
                "Las instancias conservan un estado correcto y sus métodos responden bien ante operaciones válidas e inválidas.",
                "El constructor, las propiedades y los métodos distribuyen correctamente las responsabilidades del modelo.",
                "El modelo representa el problema con nombres claros y sin duplicar estado o comportamiento."),
            _ => (
                "Contempla las entradas de referencia y los casos borde configurados.",
                "La estructura y el retorno son coherentes con la consigna.",
                "La solución es clara y adecuada al nivel del ejercicio.")
        };

        return
        [
            def.CriterioFuncional,
            def.CriterioTecnico,
            casos,
            estructura,
            calidad
        ];
    }

    private static (decimal[] Pesos, int[] Minimos) ConfiguracionRubrica(NivelDificultad dificultad) => dificultad switch
    {
        NivelDificultad.Inicial => ([38m, 20m, 17m, 13m, 12m], [72, 62]),
        NivelDificultad.Basico => ([35m, 22m, 18m, 14m, 11m], [72, 64]),
        NivelDificultad.Intermedio => ([34m, 21m, 18m, 14m, 13m], [74, 65]),
        NivelDificultad.Avanzado => ([32m, 24m, 18m, 14m, 12m], [76, 68]),
        _ => ([30m, 25m, 20m, 15m, 10m], [78, 70])
    };

    private static string Slug(string texto)
    {
        string valor = new(
            texto.ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray());

        return valor.Length <= 32 ? valor : valor[..32];
    }

    private static IReadOnlyCollection<DefinicionEjercicio> ObtenerDefiniciones() =>
    [
        new(
            "Variables y operadores",
            "Calcular el doble",
            "Implementá una función que reciba un número y retorne su doble.",
            NivelDificultad.Inicial, 1,
            "function calcularDoble(numero) {\n    // Tu código\n}",
            "Calcula y retorna exactamente el doble del número recibido.",
            "Utiliza una expresión aritmética adecuada sin valores fijos que alteren el resultado.",
            [
                new("Positivo", "calcularDoble(7)", "14"),
                new("Cero", "calcularDoble(0)", "0"),
                new("Negativo", "calcularDoble(-4)", "-8"),
            ],
            [
                "Identificá qué operación representa dos veces un valor.",
                "La función debe usar el parámetro recibido en el cálculo.",
                "Revisá que el resultado se devuelva con return.",
            ],
            [
                "variables",
                "operadores aritméticos",
                "return",
            ]),
        new(
            "Variables y operadores",
            "Minutos a segundos",
            "Recibí una cantidad de minutos y devolvé su equivalencia en segundos.",
            NivelDificultad.Inicial, 2,
            "function minutosASegundos(minutos) {\n    // Tu código\n}",
            "Convierte correctamente minutos a segundos para cualquier valor numérico válido.",
            "Aplica la relación de 60 segundos por minuto y retorna el resultado.",
            [
                new("Un minuto", "minutosASegundos(1)", "60"),
                new("Varios minutos", "minutosASegundos(5)", "300"),
                new("Cero", "minutosASegundos(0)", "0"),
            ],
            [
                "Recordá cuántos segundos tiene un minuto.",
                "La conversión puede resolverse con una única expresión.",
                "No olvides retornar el valor calculado.",
            ],
            [
                "variables",
                "multiplicación",
                "return",
            ]),
        new(
            "Variables y operadores",
            "Precio con IVA",
            "Implementá una función que reciba un precio y un porcentaje de IVA y retorne el precio final.",
            NivelDificultad.Basico, 3,
            "function precioConIva(precio, porcentajeIva) {\n    // Tu código\n}",
            "Calcula el precio final incorporando el porcentaje de IVA indicado.",
            "Diferencia correctamente el importe del impuesto del precio base y evita sumar el porcentaje como valor absoluto.",
            [
                new("IVA 22%", "precioConIva(100, 22)", "122"),
                new("Sin IVA", "precioConIva(80, 0)", "80"),
                new("Precio decimal", "precioConIva(50.5, 10)", "55.55"),
            ],
            [
                "Primero calculá cuánto representa el porcentaje sobre el precio.",
                "Un porcentaje se transforma dividiéndolo entre 100.",
                "El precio final es la suma del precio base y el impuesto.",
            ],
            [
                "porcentajes",
                "operadores aritméticos",
                "variables",
            ]),
        new(
            "Variables y operadores",
            "Área de un rectángulo",
            "Recibí la base y la altura de un rectángulo y devolvé su área.",
            NivelDificultad.Basico, 4,
            "function areaRectangulo(base, altura) {\n    // Tu código\n}",
            "Calcula correctamente el área multiplicando las dos dimensiones recibidas.",
            "Utiliza base y altura en una expresión aritmética y retorna un único valor numérico.",
            [
                new("Rectángulo", "areaRectangulo(5, 3)", "15"),
                new("Cuadrado", "areaRectangulo(4, 4)", "16"),
                new("Decimales", "areaRectangulo(2.5, 2)", "5"),
            ],
            [
                "El área representa la superficie interior de la figura.",
                "Usá las dos dimensiones recibidas en el cálculo.",
                "La función debe retornar un número, no mostrarlo en consola.",
            ],
            [
                "variables",
                "multiplicación",
                "return",
            ]),
        new(
            "Variables y operadores",
            "Segundos restantes",
            "Recibí una cantidad total de segundos y devolvé cuántos segundos sobran después de formar minutos completos.",
            NivelDificultad.Intermedio, 5,
            "function segundosRestantes(totalSegundos) {\n    // Tu código\n}",
            "Obtiene correctamente los segundos que no completan un minuto.",
            "Utiliza el operador módulo con 60 y retorna un único valor numérico.",
            [
                new("Minuto exacto", "segundosRestantes(60)", "0"),
                new("Combinado", "segundosRestantes(3671)", "11"),
                new("Menos de minuto", "segundosRestantes(45)", "45"),
            ],
            [
                "Un minuto completo contiene 60 segundos.",
                "Necesitás conocer el sobrante, no la cantidad de minutos.",
                "El operador módulo devuelve el resto de una división.",
            ],
            [
                "módulo",
                "variables",
                "return",
            ]),
        new(
            "Variables y operadores",
            "Consumo promedio",
            "Recibí kilómetros recorridos y litros consumidos y devolvé los kilómetros por litro.",
            NivelDificultad.Intermedio, 6,
            "function consumoPromedio(kilometros, litros) {\n    // Tu código\n}",
            "Calcula correctamente la relación entre distancia recorrida y combustible consumido.",
            "Respeta el orden de la división y contempla que el consumo se expresa en kilómetros por litro.",
            [
                new("Habitual", "consumoPromedio(500, 40)", "12.5"),
                new("Exacto", "consumoPromedio(300, 30)", "10"),
                new("Decimal", "consumoPromedio(125, 12.5)", "10"),
            ],
            [
                "Pensá qué magnitud debe quedar en el numerador.",
                "La unidad buscada es kilómetros por cada litro.",
                "Retorná el cociente sin redondearlo innecesariamente.",
            ],
            [
                "división",
                "variables",
                "return",
            ]),
        new(
            "Variables y operadores",
            "Tarifa de viaje",
            "Calculá la tarifa total recibiendo bajada de bandera, kilómetros, precio por kilómetro, minutos de espera y precio por minuto.",
            NivelDificultad.Avanzado, 7,
            "function calcularTarifa(bajada, kilometros, precioKm, esperaMinutos, precioMinuto) {\n    // Tu código\n}",
            "Integra correctamente todos los componentes de la tarifa.",
            "Calcula por separado distancia y espera, evitando omitir o duplicar conceptos.",
            [
                new("Sin espera", "calcularTarifa(50, 10, 20, 0, 5)", "250"),
                new("Con espera", "calcularTarifa(50, 10, 20, 4, 5)", "270"),
                new("Solo bajada", "calcularTarifa(75, 0, 20, 0, 5)", "75"),
            ],
            [
                "Identificá los tres componentes del total.",
                "Distancia y espera tienen precios unitarios diferentes.",
                "Sumá los subtotales una sola vez junto a la bajada.",
            ],
            [
                "expresiones compuestas",
                "subtotales",
                "parámetros",
            ]),
        new(
            "Variables y operadores",
            "Sobrante de presupuesto",
            "Recibí un presupuesto y tres porcentajes de asignación. Devolvé el dinero que queda sin asignar.",
            NivelDificultad.Desafio, 8,
            "function sobrantePresupuesto(total, porcentajeA, porcentajeB, porcentajeC) {\n    // Tu código\n}",
            "Calcula correctamente el dinero no asignado luego de aplicar los tres porcentajes.",
            "Convierte los porcentajes en proporciones, suma lo asignado una sola vez y conserva la precisión decimal.",
            [
                new("Sin sobrante", "sobrantePresupuesto(1000, 40, 35, 25)", "0"),
                new("Con sobrante", "sobrantePresupuesto(1000, 30, 20, 10)", "400"),
                new("Decimales", "sobrantePresupuesto(250, 33, 33, 33)", "2.5"),
            ],
            [
                "Sumá primero los tres porcentajes asignados.",
                "Calculá cuánto dinero representa ese porcentaje total.",
                "El sobrante es la diferencia entre el presupuesto original y lo asignado.",
            ],
            [
                "porcentajes",
                "expresiones compuestas",
                "precisión decimal",
                "return",
            ]),
        new(
            "Condicionales",
            "Validar mayoría de edad",
            "Retorná true si la edad recibida es 18 o mayor; en caso contrario, false.",
            NivelDificultad.Inicial, 1,
            "function esMayorDeEdad(edad) {\n    // Tu código\n}",
            "Clasifica correctamente edades menores y mayores o iguales a 18.",
            "Utiliza una comparación inclusiva para contemplar exactamente los 18 años.",
            [
                new("Menor", "esMayorDeEdad(17)", "false"),
                new("Límite", "esMayorDeEdad(18)", "true"),
                new("Mayor", "esMayorDeEdad(30)", "true"),
            ],
            [
                "El valor 18 debe pertenecer a uno de los dos grupos.",
                "Podés retornar directamente el resultado de una comparación.",
                "Revisá si el operador incluye el valor límite.",
            ],
            [
                "comparaciones",
                "booleanos",
                "if",
            ]),
        new(
            "Condicionales",
            "Positivo, negativo o cero",
            "Devolvé \"Positivo\", \"Negativo\" o \"Cero\" según el número recibido.",
            NivelDificultad.Inicial, 2,
            "function clasificarNumero(numero) {\n    // Tu código\n}",
            "Distingue correctamente las tres categorías posibles.",
            "Ordena las condiciones sin dejar al cero incluido en positivo o negativo.",
            [
                new("Positivo", "clasificarNumero(8)", "Positivo"),
                new("Negativo", "clasificarNumero(-3)", "Negativo"),
                new("Cero", "clasificarNumero(0)", "Cero"),
            ],
            [
                "Hay tres resultados mutuamente excluyentes.",
                "Comprobá primero una categoría y luego la siguiente.",
                "El cero no es positivo ni negativo.",
            ],
            [
                "if",
                "else if",
                "comparaciones",
            ]),
        new(
            "Condicionales",
            "Mayor de dos números",
            "Devolvé el mayor de dos números o el texto \"Iguales\" cuando coincidan.",
            NivelDificultad.Basico, 3,
            "function mayorDeDos(a, b) {\n    // Tu código\n}",
            "Devuelve el mayor valor y reconoce correctamente la igualdad.",
            "Contempla los tres caminos sin asumir que uno siempre será diferente.",
            [
                new("Primero mayor", "mayorDeDos(9, 4)", "9"),
                new("Segundo mayor", "mayorDeDos(2, 7)", "7"),
                new("Iguales", "mayorDeDos(5, 5)", "Iguales"),
            ],
            [
                "Antes de elegir un mayor, revisá si coinciden.",
                "Las comparaciones > y < cubren los valores diferentes.",
                "Cada camino debe devolver el resultado esperado.",
            ],
            [
                "if/else",
                "comparaciones",
                "return",
            ]),
        new(
            "Condicionales",
            "Clasificar una nota",
            "Recibí una nota de 0 a 100 y devolvé Insuficiente, Aceptable, Buena o Excelente según rangos definidos en la consigna.",
            NivelDificultad.Basico, 4,
            "function clasificarNota(nota) {\n    // Excelente: 90 o más\n    // Buena: 75 a 89\n    // Aceptable: 60 a 74\n    // Insuficiente: menos de 60\n}",
            "Clasifica correctamente cada nota dentro de los cuatro rangos.",
            "Evalúa los límites en un orden que evite solapamientos y contempla exactamente 60, 75 y 90.",
            [
                new("Insuficiente", "clasificarNota(59)", "Insuficiente"),
                new("Aceptable", "clasificarNota(60)", "Aceptable"),
                new("Buena", "clasificarNota(80)", "Buena"),
                new("Excelente", "clasificarNota(90)", "Excelente"),
            ],
            [
                "Empezar por el rango más alto simplifica las condiciones.",
                "Una vez cumplida una condición, los rangos anteriores quedan descartados.",
                "Probá especialmente los valores límite.",
            ],
            [
                "rangos",
                "else if",
                "operadores relacionales",
            ]),
        new(
            "Condicionales",
            "Mayor de tres números",
            "Determiná el mayor de tres números sin utilizar arrays ni Math.max. Si todos son iguales, devolvé \"Iguales\".",
            NivelDificultad.Intermedio, 5,
            "function mayorDeTres(a, b, c) {\n    // Tu código\n}",
            "Identifica correctamente el mayor entre tres valores y la igualdad total.",
            "Combina comparaciones lógicas sin excluir empates parciales que igualmente tienen un mayor.",
            [
                new("Primero", "mayorDeTres(9, 4, 3)", "9"),
                new("Segundo", "mayorDeTres(2, 8, 6)", "8"),
                new("Tercero", "mayorDeTres(2, 5, 7)", "7"),
                new("Todos iguales", "mayorDeTres(4, 4, 4)", "Iguales"),
            ],
            [
                "Primero tratá el caso en que los tres coinciden.",
                "Para ser mayor, un valor debe compararse con los otros dos.",
                "Pensá qué ocurre cuando dos valores comparten el máximo.",
            ],
            [
                "operadores lógicos",
                "comparaciones",
                "if",
            ],
            [
                "arrays",
                "Math.max",
            ]),
        new(
            "Condicionales",
            "Costo de envío",
            "Calculá el costo de envío según monto y zona: gratis desde 3000; si no, 150 para Montevideo y 250 para Interior.",
            NivelDificultad.Intermedio, 6,
            "function costoEnvio(monto, zona) {\n    // Tu código\n}",
            "Aplica correctamente gratuidad y tarifa según zona.",
            "Da prioridad a la regla del monto antes de evaluar la zona y distingue los textos esperados.",
            [
                new("Gratis", "costoEnvio(3500, \"Interior\")", "0"),
                new("Montevideo", "costoEnvio(1000, \"Montevideo\")", "150"),
                new("Interior", "costoEnvio(1000, \"Interior\")", "250"),
            ],
            [
                "Hay una regla que vuelve irrelevante la zona.",
                "Evaluá primero la gratuidad.",
                "Luego distinguí las dos zonas.",
            ],
            [
                "condiciones anidadas",
                "strings",
                "prioridad de reglas",
            ]),
        new(
            "Condicionales",
            "Año bisiesto",
            "Devolvé true cuando el año sea bisiesto según las reglas del calendario gregoriano.",
            NivelDificultad.Avanzado, 7,
            "function esBisiesto(anio) {\n    // Tu código\n}",
            "Aplica correctamente las reglas combinadas de divisibilidad por 4, 100 y 400.",
            "Utiliza operadores lógicos con la agrupación correcta y contempla los años seculares.",
            [
                new("Común bisiesto", "esBisiesto(2024)", "true"),
                new("No bisiesto", "esBisiesto(2023)", "false"),
                new("Secular no", "esBisiesto(1900)", "false"),
                new("Secular sí", "esBisiesto(2000)", "true"),
            ],
            [
                "La divisibilidad por 4 no es suficiente para todos los años.",
                "Los múltiplos de 100 tienen una excepción.",
                "Los múltiplos de 400 vuelven a ser bisiestos.",
            ],
            [
                "módulo",
                "operadores lógicos",
                "precedencia",
            ]),
        new(
            "Condicionales",
            "Descuentos acumulables",
            "Calculá el precio final: 10% por cliente premium, 5% por pago contado y 8% si el monto supera 5000. El descuento total no puede superar 20%.",
            NivelDificultad.Desafio, 8,
            "function precioConDescuentos(monto, esPremium, pagoContado) {\n    // Tu código\n}",
            "Acumula correctamente los descuentos aplicables y respeta el máximo del 20%.",
            "Diferencia porcentaje acumulado de importe descontado y aplica una sola vez el descuento final.",
            [
                new("Sin descuento", "precioConDescuentos(1000, false, false)", "1000"),
                new("Dos descuentos", "precioConDescuentos(4000, true, true)", "3400"),
                new("Tope", "precioConDescuentos(6000, true, true)", "4800"),
            ],
            [
                "Acumulá primero un porcentaje total.",
                "Cada regla puede sumar al porcentaje existente.",
                "Antes de calcular el precio, limitá el descuento máximo.",
            ],
            [
                "booleanos",
                "porcentajes",
                "Math.min",
            ]),
        new(
            "Bucles",
            "Sumar del 1 al límite",
            "Retorná la suma de todos los enteros desde 1 hasta el límite inclusive.",
            NivelDificultad.Inicial, 1,
            "function sumarHasta(limite) {\n    let suma = 0;\n    // Tu código\n}",
            "Acumula todos los valores desde 1 hasta el límite inclusive.",
            "Inicializa, actualiza y retorna correctamente el acumulador sin omitir el límite.",
            [
                new("Uno", "sumarHasta(1)", "1"),
                new("Cinco", "sumarHasta(5)", "15"),
                new("Diez", "sumarHasta(10)", "55"),
            ],
            [
                "Necesitás una variable que conserve la suma parcial.",
                "El recorrido comienza en 1.",
                "Revisá si el límite debe incluirse.",
            ],
            [
                "for",
                "acumulador",
                "return",
            ]),
        new(
            "Bucles",
            "Números pares en un rango",
            "Recibí inicio y fin y devolvé un array con todos los números pares del rango inclusive.",
            NivelDificultad.Inicial, 2,
            "function paresEnRango(inicio, fin) {\n    let pares = [];\n    // Tu código\n}",
            "Incluye todos y únicamente los números pares dentro del rango.",
            "Recorre los límites de forma inclusiva y agrega valores al array sin posiciones vacías.",
            [
                new("Rango", "paresEnRango(1, 8)", "[2,4,6,8]"),
                new("Inicio par", "paresEnRango(2, 5)", "[2,4]"),
                new("Sin pares", "paresEnRango(1, 1)", "[]"),
            ],
            [
                "Recorré cada número entre inicio y fin.",
                "El resto al dividir entre 2 identifica los pares.",
                "Agregá al array solo cuando se cumpla la condición.",
            ],
            [
                "for",
                "módulo",
                "push",
            ]),
        new(
            "Bucles",
            "Tabla de multiplicar",
            "Devolvé un array con los resultados de multiplicar un número por los valores del 1 al 10.",
            NivelDificultad.Basico, 3,
            "function tablaMultiplicar(numero) {\n    let resultados = [];\n    // Tu código\n}",
            "Genera exactamente los diez productos esperados en orden.",
            "Controla correctamente inicio, fin e incremento del bucle y agrega cada producto al array.",
            [
                new("Tabla 2", "tablaMultiplicar(2)", "[2,4,6,8,10,12,14,16,18,20]"),
                new("Tabla 0", "tablaMultiplicar(0)", "[0,0,0,0,0,0,0,0,0,0]"),
            ],
            [
                "El multiplicador cambia de 1 a 10.",
                "El número recibido permanece fijo.",
                "El array debe terminar con diez elementos.",
            ],
            [
                "for",
                "arrays",
                "multiplicación",
            ]),
        new(
            "Bucles",
            "Contar múltiplos",
            "Contá cuántos números entre inicio y fin inclusive son múltiplos de un divisor recibido.",
            NivelDificultad.Basico, 4,
            "function contarMultiplos(inicio, fin, divisor) {\n    let cantidad = 0;\n    // Tu código\n}",
            "Cuenta exactamente los valores divisibles dentro del rango.",
            "Utiliza módulo y un acumulador, recorriendo ambos límites de forma coherente.",
            [
                new("Habitual", "contarMultiplos(1, 10, 3)", "3"),
                new("Incluye límites", "contarMultiplos(3, 9, 3)", "3"),
                new("Ninguno", "contarMultiplos(1, 2, 5)", "0"),
            ],
            [
                "Recorré todo el rango.",
                "Un múltiplo deja resto cero.",
                "Incrementá la cantidad solo al encontrar uno.",
            ],
            [
                "for",
                "módulo",
                "contador",
            ]),
        new(
            "Bucles",
            "Calcular factorial",
            "Retorná el factorial de un entero no negativo. Recordá que 0! es 1.",
            NivelDificultad.Intermedio, 5,
            "function factorial(numero) {\n    // Tu código\n}",
            "Calcula correctamente el producto desde 1 hasta el número y contempla el caso cero.",
            "Inicializa el acumulador multiplicativo en un valor neutro y evita multiplicar por cero.",
            [
                new("Cero", "factorial(0)", "1"),
                new("Uno", "factorial(1)", "1"),
                new("Cinco", "factorial(5)", "120"),
            ],
            [
                "En una multiplicación acumulada, iniciar en cero eliminaría todo.",
                "El producto puede comenzar en 1.",
                "El caso 0 funciona naturalmente si el bucle no se ejecuta.",
            ],
            [
                "for",
                "acumulador multiplicativo",
                "caso borde",
            ]),
        new(
            "Bucles",
            "Mayor valor de un array",
            "Recibí un array no vacío y retorná su valor mayor sin utilizar Math.max.",
            NivelDificultad.Intermedio, 6,
            "function obtenerMayor(valores) {\n    // Tu código\n}",
            "Encuentra el mayor valor para arrays con positivos, negativos y un solo elemento.",
            "Inicializa el mayor a partir de un elemento real del array y recorre únicamente índices válidos.",
            [
                new("Positivos", "obtenerMayor([3,8,2])", "8"),
                new("Negativos", "obtenerMayor([-9,-2,-7])", "-2"),
                new("Único", "obtenerMayor([5])", "5"),
            ],
            [
                "No uses cero como mayor inicial: puede fallar con negativos.",
                "Tomá como referencia el primer elemento.",
                "El recorrido puede comenzar en el siguiente índice.",
            ],
            [
                "arrays",
                "for",
                "comparaciones",
            ],
            [
                "Math.max",
            ]),
        new(
            "Bucles",
            "Número primo",
            "Devolvé true si el entero mayor o igual a 2 es primo y false en caso contrario.",
            NivelDificultad.Avanzado, 7,
            "function esPrimo(numero) {\n    // Tu código\n}",
            "Reconoce correctamente primos y compuestos, incluyendo los casos menores a 2.",
            "Busca divisores de manera eficiente y corta el recorrido al encontrarlos.",
            [
                new("Primo", "esPrimo(13)", "true"),
                new("Compuesto", "esPrimo(12)", "false"),
                new("Uno", "esPrimo(1)", "false"),
                new("Dos", "esPrimo(2)", "true"),
            ],
            [
                "Los números menores a 2 no son primos.",
                "Un divisor exacto distinto de 1 y del propio número invalida la primalidad.",
                "No necesitás revisar divisores mayores que la raíz cuadrada.",
            ],
            [
                "for",
                "módulo",
                "retorno temprano",
            ]),
        new(
            "Bucles",
            "Secuencia de Fibonacci",
            "Generá un array con la cantidad solicitada de términos de Fibonacci, comenzando en 0 y 1, sin recursividad.",
            NivelDificultad.Desafio, 8,
            "function fibonacci(cantidad) {\n    // Tu código\n}",
            "Genera la cantidad exacta de términos en el orden correcto y contempla cantidades pequeñas.",
            "Actualiza los dos valores previos sin perder información y evita agregar elementos de más.",
            [
                new("Uno", "fibonacci(1)", "[0]"),
                new("Dos", "fibonacci(2)", "[0,1]"),
                new("Seis", "fibonacci(6)", "[0,1,1,2,3,5]"),
            ],
            [
                "Trat&aacute primero cantidades 0, 1 o 2.",
                "Cada término surge de los dos anteriores.",
                "Actualizá ambos valores después de guardar el nuevo término.",
            ],
            [
                "arrays",
                "for",
                "variables auxiliares",
            ]),
        new(
            "Strings",
            "Contar caracteres sin espacios",
            "Retorná cuántos caracteres tiene un texto sin contar espacios simples.",
            NivelDificultad.Inicial, 1,
            "function contarSinEspacios(texto) {\n    // Tu código\n}",
            "Cuenta todos los caracteres excepto los espacios.",
            "Recorre únicamente índices válidos y diferencia el carácter espacio de los demás.",
            [
                new("Texto", "contarSinEspacios(\"hola mundo\")", "9"),
                new("Sin espacios", "contarSinEspacios(\"abc\")", "3"),
                new("Solo espacios", "contarSinEspacios(\"   \")", "0"),
            ],
            [
                "Recorré el texto carácter por carácter.",
                "Compará cada carácter con un espacio.",
                "Incrementá un contador cuando sea diferente.",
            ],
            [
                "strings",
                "for",
                "contador",
            ]),
        new(
            "Strings",
            "Convertir a mayúsculas",
            "Recibí un texto y retornalo completamente en mayúsculas.",
            NivelDificultad.Inicial, 2,
            "function convertirMayusculas(texto) {\n    // Tu código\n}",
            "Retorna el texto normalizado en mayúsculas sin modificar su contenido restante.",
            "Utiliza correctamente el método de string y devuelve el nuevo valor.",
            [
                new("Palabra", "convertirMayusculas(\"hola\")", "HOLA"),
                new("Mixto", "convertirMayusculas(\"EduCode 1\")", "EDUCODE 1"),
                new("Vacío", "convertirMayusculas(\"\")", ""),
            ],
            [
                "Los strings tienen un método específico para esta conversión.",
                "El método no modifica el string original: produce otro.",
                "Retorná directamente el resultado.",
            ],
            [
                "toUpperCase",
                "strings",
                "return",
            ]),
        new(
            "Strings",
            "Contar vocales",
            "Retorná cuántas vocales contiene un texto sin distinguir mayúsculas de minúsculas.",
            NivelDificultad.Basico, 3,
            "function contarVocales(texto) {\n    let cantidad = 0;\n    // Tu código\n}",
            "Cuenta todas y únicamente las vocales del texto.",
            "Normaliza el texto y recorre únicamente índices válidos; i <= texto.length constituye un error de límite.",
            [
                new("Habitual", "contarVocales(\"hola\")", "2"),
                new("Mayúsculas", "contarVocales(\"PROGRAMAR\")", "3"),
                new("Vacío", "contarVocales(\"\")", "0"),
                new("Sin vocales", "contarVocales(\"xyz\")", "0"),
            ],
            [
                "Podés normalizar el texto antes de recorrerlo.",
                "Los índices válidos van de 0 a length - 1.",
                "Comprobá si cada carácter pertenece al conjunto de vocales.",
            ],
            [
                "toLowerCase",
                "includes",
                "recorrido de strings",
            ]),
        new(
            "Strings",
            "Invertir una palabra",
            "Recibí una palabra y retornala con sus caracteres en orden inverso sin usar reverse().",
            NivelDificultad.Basico, 4,
            "function invertirPalabra(palabra) {\n    // Tu código\n}",
            "Construye correctamente la palabra invertida para cualquier longitud.",
            "Recorre desde el último índice válido hasta cero sin acceder fuera de rango.",
            [
                new("Palabra", "invertirPalabra(\"casa\")", "asac"),
                new("Única", "invertirPalabra(\"a\")", "a"),
                new("Vacía", "invertirPalabra(\"\")", ""),
            ],
            [
                "El último carácter está en length - 1.",
                "El índice debe disminuir en cada iteración.",
                "Podés concatenar cada carácter a un acumulador string.",
            ],
            [
                "for descendente",
                "concatenación",
                "índices",
            ],
            [
                "reverse",
            ]),
        new(
            "Strings",
            "Palabra palíndroma",
            "Devolvé true si una palabra se lee igual en ambos sentidos, ignorando mayúsculas.",
            NivelDificultad.Intermedio, 5,
            "function esPalindroma(palabra) {\n    // Tu código\n}",
            "Reconoce correctamente palabras palíndromas y no palíndromas.",
            "Normaliza el texto y compara posiciones simétricas usando únicamente índices válidos.",
            [
                new("Sí", "esPalindroma(\"Reconocer\")", "true"),
                new("No", "esPalindroma(\"programar\")", "false"),
                new("Única", "esPalindroma(\"A\")", "true"),
            ],
            [
                "Convertí todo a un mismo formato.",
                "Compará el primer carácter con el último, luego avanzá hacia el centro.",
                "No es necesario recorrer toda la palabra.",
            ],
            [
                "strings",
                "índices simétricos",
                "booleanos",
            ]),
        new(
            "Strings",
            "Contar palabras",
            "Retorná la cantidad de palabras de un texto, ignorando espacios al inicio, al final y espacios consecutivos.",
            NivelDificultad.Intermedio, 6,
            "function contarPalabras(texto) {\n    // Tu código\n}",
            "Cuenta correctamente palabras separadas por una o más espacios.",
            "Gestiona inicio, final, texto vacío y transiciones entre espacio y carácter sin duplicar conteos.",
            [
                new("Habitual", "contarPalabras(\"hola mundo\")", "2"),
                new("Espacios", "contarPalabras(\"  hola   mundo  \")", "2"),
                new("Vacío", "contarPalabras(\"   \")", "0"),
            ],
            [
                "Una palabra comienza cuando aparece un carácter luego de un espacio o al inicio.",
                "Podés registrar si actualmente estás dentro de una palabra.",
                "Probá con varios espacios consecutivos.",
            ],
            [
                "strings",
                "estado booleano",
                "recorrido",
            ]),
        new(
            "Strings",
            "Validar contraseña académica",
            "Validá largo mínimo 8, una mayúscula, una minúscula, un número, un carácter especial y ausencia de espacios.",
            NivelDificultad.Avanzado, 7,
            "function validarPassword(password) {\n    // Tu código\n}",
            "Comprueba todas las reglas y retorna true únicamente cuando se cumplen en conjunto.",
            "Recorre índices válidos, actualiza indicadores booleanos y distingue correctamente cada tipo de carácter.",
            [
                new("Válida", "validarPassword(\"Clave#12\")", "true"),
                new("Sin mayúscula", "validarPassword(\"clave#12\")", "false"),
                new("Con espacio", "validarPassword(\"Clave #12\")", "false"),
                new("Corta", "validarPassword(\"Aa#1\")", "false"),
            ],
            [
                "Separá cada regla en una variable booleana.",
                "Un único recorrido puede detectar varios tipos de caracteres.",
                "El resultado final combina todas las condiciones con AND.",
            ],
            [
                "strings",
                "booleanos",
                "recorrido",
                "operadores lógicos",
            ]),
        new(
            "Strings",
            "Anagramas",
            "Devolvé true si dos textos contienen las mismas letras con igual frecuencia, ignorando mayúsculas y espacios.",
            NivelDificultad.Desafio, 8,
            "function sonAnagramas(textoA, textoB) {\n    // Tu código\n}",
            "Reconoce anagramas considerando frecuencia de caracteres, no solo presencia.",
            "Normaliza ambos textos, compara longitudes útiles y contabiliza repeticiones correctamente.",
            [
                new("Sí", "sonAnagramas(\"Roma\", \"amor\")", "true"),
                new("Con espacios", "sonAnagramas(\"the eyes\", \"they see\")", "true"),
                new("No", "sonAnagramas(\"casa\", \"caso\")", "false"),
                new("Repeticiones", "sonAnagramas(\"aab\", \"abb\")", "false"),
            ],
            [
                "Normalizá mayúsculas y quitá espacios.",
                "Dos anagramas deben tener la misma cantidad de letras.",
                "Contá cuántas veces aparece cada carácter.",
            ],
            [
                "strings",
                "objetos de frecuencia",
                "recorridos",
            ]),
        new(
            "Array",
            "Obtener el primer elemento",
            "Recibí un array y retorná su primer elemento. Si está vacío, retorná null.",
            NivelDificultad.Inicial, 1,
            "function obtenerPrimero(array) {\n    // Tu código\n}",
            "Retorna el elemento ubicado en el índice cero y contempla el array vacío.",
            "Utiliza el acceso por índice y la longitud del array sin recorrer elementos innecesariamente.",
            [
                new("Habitual", "obtenerPrimero([8, 4, 2])", "8"),
                new("Un elemento", "obtenerPrimero([15])", "15"),
                new("Vacío", "obtenerPrimero([])", "null"),
            ],
            [
                "El primer elemento ocupa el índice cero.",
                "Antes de acceder, revisá la longitud del array.",
                "Para el caso vacío, retorná null de forma explícita.",
            ],
            [
                "array",
                "índices",
                "length",
                "return",
            ]),
        new(
            "Array",
            "Sumar los elementos",
            "Recibí un array de números y retorná la suma de todos sus elementos. Un array vacío debe devolver 0.",
            NivelDificultad.Basico, 2,
            "function sumarArray(numeros) {\n    // Tu código\n}",
            "Acumula correctamente todos los valores y devuelve cero cuando no hay elementos.",
            "Inicializa el acumulador, recorre únicamente índices válidos y suma cada elemento una sola vez.",
            [
                new("Positivos", "sumarArray([2, 4, 6])", "12"),
                new("Con negativos", "sumarArray([5, -2, -3])", "0"),
                new("Vacío", "sumarArray([])", "0"),
            ],
            [
                "Necesitás una variable acumuladora que comience en cero.",
                "Recorré desde el índice cero hasta antes de length.",
                "En cada iteración, sumá el elemento actual al acumulador.",
            ],
            [
                "array",
                "recorrido",
                "acumulador",
                "length",
            ]),
        new(
            "Array",
            "Contar valores mayores",
            "Recibí un array de números y un límite. Retorná cuántos elementos son estrictamente mayores que ese límite.",
            NivelDificultad.Intermedio, 3,
            "function contarMayores(numeros, limite) {\n    // Tu código\n}",
            "Cuenta solamente los elementos que superan estrictamente el límite recibido.",
            "Combina un recorrido completo con una condición y un contador sin modificar el array.",
            [
                new("Habitual", "contarMayores([2, 8, 5, 10], 5)", "2"),
                new("Ninguno", "contarMayores([1, 2, 3], 5)", "0"),
                new("Iguales no cuentan", "contarMayores([5, 5, 6], 5)", "1"),
            ],
            [
                "Separá el recorrido del criterio de conteo.",
                "La comparación solicitada es estrictamente mayor.",
                "Incrementá el contador únicamente cuando se cumpla la condición.",
            ],
            [
                "array",
                "contador",
                "condicional",
                "recorrido",
            ]),
        new(
            "Array",
            "Buscar el índice de un valor",
            "Recibí un array y un valor buscado. Retorná el índice de su primera aparición o -1 cuando no exista.",
            NivelDificultad.Avanzado, 4,
            "function buscarIndice(array, buscado) {\n    // Tu código\n}",
            "Devuelve el primer índice coincidente y -1 cuando el valor no aparece.",
            "Detiene la búsqueda al encontrar la primera coincidencia y respeta todos los índices válidos.",
            [
                new("Primera posición", "buscarIndice([7, 4, 7], 7)", "0"),
                new("Posición intermedia", "buscarIndice([3, 9, 5], 9)", "1"),
                new("No existe", "buscarIndice([1, 2, 3], 8)", "-1"),
            ],
            [
                "Recorré el array desde el comienzo.",
                "Cuando encuentres una coincidencia, retorná el índice inmediatamente.",
                "Si termina el recorrido sin coincidencias, retorná -1.",
            ],
            [
                "array",
                "búsqueda",
                "índices",
                "retorno anticipado",
            ]),
        new(
            "Lista",
            "Agregar un elemento al final",
            "Recibí una lista y un elemento. Agregalo al final y retorná la nueva cantidad de elementos.",
            NivelDificultad.Inicial, 1,
            "function agregarElemento(lista, elemento) {\n    // Tu código\n}",
            "Incorpora el elemento al final y devuelve la longitud resultante.",
            "Utiliza una operación de inserción coherente y no reemplaza elementos existentes.",
            [
                new("Lista con datos", "agregarElemento([1, 2], 3)", "3"),
                new("Lista vacía", "agregarElemento([], 9)", "1"),
                new("Otro tipo de valor", "agregarElemento([4], 0)", "2"),
            ],
            [
                "La lista debe conservar los elementos que ya tenía.",
                "Existe una operación que incorpora al final.",
                "Luego de agregar, retorná la longitud actualizada.",
            ],
            [
                "lista",
                "push",
                "length",
                "inserción",
            ]),
        new(
            "Lista",
            "Quitar el último elemento",
            "Recibí una lista y retorná el último elemento quitándolo de la colección. Si está vacía, retorná null.",
            NivelDificultad.Basico, 2,
            "function quitarUltimo(lista) {\n    // Tu código\n}",
            "Elimina y devuelve el último elemento, contemplando de forma explícita la lista vacía.",
            "Valida la longitud antes de eliminar y utiliza una operación adecuada sobre el final de la lista.",
            [
                new("Habitual", "quitarUltimo([2, 4, 6])", "6"),
                new("Un elemento", "quitarUltimo([11])", "11"),
                new("Vacía", "quitarUltimo([])", "null"),
            ],
            [
                "Primero comprobá si la lista contiene elementos.",
                "La operación debe afectar el final, no el comienzo.",
                "Para la lista vacía, retorná null antes de intentar quitar.",
            ],
            [
                "lista",
                "pop",
                "length",
                "eliminación",
            ]),
        new(
            "Lista",
            "Contar apariciones",
            "Recibí una lista y un valor. Retorná cuántas veces aparece ese valor en la colección.",
            NivelDificultad.Intermedio, 3,
            "function contarApariciones(lista, valor) {\n    // Tu código\n}",
            "Cuenta todas las coincidencias, incluidas las repetidas, y devuelve cero cuando no existen.",
            "Recorre la lista completa y actualiza el contador solamente ante igualdad con el valor buscado.",
            [
                new("Repetidos", "contarApariciones([1, 2, 1, 1], 1)", "3"),
                new("Una aparición", "contarApariciones([4, 8, 6], 8)", "1"),
                new("Sin apariciones", "contarApariciones([2, 3], 9)", "0"),
            ],
            [
                "Necesitás un contador inicializado en cero.",
                "Compará cada elemento con el valor recibido.",
                "No finalices el recorrido en la primera coincidencia.",
            ],
            [
                "lista",
                "recorrido",
                "contador",
                "comparación",
            ]),
        new(
            "Lista",
            "Eliminar la primera aparición",
            "Recibí una lista y un valor. Eliminá solamente su primera aparición y retorná true. Si no existe, retorná false.",
            NivelDificultad.Avanzado, 4,
            "function eliminarPrimeraAparicion(lista, valor) {\n    // Tu código\n}",
            "Elimina una única coincidencia, la primera, e informa correctamente si la operación se realizó.",
            "Localiza el índice, modifica la lista una sola vez y evita continuar el recorrido luego de eliminar.",
            [
                new("Primera posición", "eliminarPrimeraAparicion([3, 5, 3], 3)", "true"),
                new("Posición intermedia", "eliminarPrimeraAparicion([1, 4, 8], 4)", "true"),
                new("No existe", "eliminarPrimeraAparicion([1, 2], 7)", "false"),
            ],
            [
                "Buscá primero el índice de la primera coincidencia.",
                "La eliminación debe quitar exactamente un elemento.",
                "Retorná apenas se complete la operación; si no ocurre, devolvé false al final.",
            ],
            [
                "lista",
                "búsqueda",
                "splice",
                "eliminación",
            ]),
        new(
            "Funciones",
            "Determinar si es par",
            "Creá una función que reciba un entero y retorne un booleano indicando si es par.",
            NivelDificultad.Inicial, 1,
            "function esPar(numero) {\n    // Tu código\n}",
            "Retorna true para pares y false para impares, incluyendo negativos y cero.",
            "Utiliza módulo y devuelve un booleano, no textos equivalentes.",
            [
                new("Par", "esPar(8)", "true"),
                new("Impar", "esPar(7)", "false"),
                new("Cero", "esPar(0)", "true"),
                new("Negativo", "esPar(-4)", "true"),
            ],
            [
                "El resto de dividir un par entre 2 es cero.",
                "La comparación ya produce un booleano.",
                "Retorná el resultado de esa comparación.",
            ],
            [
                "funciones",
                "módulo",
                "booleanos",
            ]),
        new(
            "Funciones",
            "Calcular promedio",
            "Recibí tres números y retorná su promedio.",
            NivelDificultad.Inicial, 2,
            "function calcularPromedio(a, b, c) {\n    // Tu código\n}",
            "Calcula el promedio aritmético correcto de los tres parámetros.",
            "Respeta la agrupación de la suma antes de dividir y retorna el valor.",
            [
                new("Enteros", "calcularPromedio(6, 8, 10)", "8"),
                new("Decimales", "calcularPromedio(1, 2, 2)", "1.6666666666666667"),
                new("Ceros", "calcularPromedio(0,0,0)", "0"),
            ],
            [
                "Sumá los tres valores antes de dividir.",
                "Usá paréntesis para dejar clara la precedencia.",
                "La cantidad de valores es tres.",
            ],
            [
                "parámetros",
                "return",
                "precedencia",
            ]),
        new(
            "Funciones",
            "Obtener iniciales",
            "Recibí nombre y apellido y retorná sus iniciales en mayúsculas separadas por un punto.",
            NivelDificultad.Basico, 3,
            "function obtenerIniciales(nombre, apellido) {\n    // Tu código\n}",
            "Construye las dos iniciales en el formato solicitado.",
            "Accede al primer carácter válido de cada texto, normaliza y concatena correctamente.",
            [
                new("Habitual", "obtenerIniciales(\"Leonardo\", \"Paroli\")", "L.P"),
                new("Minúsculas", "obtenerIniciales(\"ana\", \"suarez\")", "A.S"),
            ],
            [
                "La inicial está en la posición cero.",
                "Convertí cada carácter a mayúscula.",
                "Concatená ambas iniciales con el separador solicitado.",
            ],
            [
                "parámetros",
                "strings",
                "concatenación",
            ]),
        new(
            "Funciones",
            "Calcular descuento",
            "Creá una función calcularDescuento(monto, porcentaje) y otra precioFinal(monto, porcentaje) que reutilice la primera.",
            NivelDificultad.Basico, 4,
            "function calcularDescuento(monto, porcentaje) {\n    // Tu código\n}\n\nfunction precioFinal(monto, porcentaje) {\n    // Tu código\n}",
            "Ambas funciones producen importes correctos y precioFinal reutiliza calcularDescuento.",
            "Separa responsabilidades, pasa correctamente los parámetros y utiliza los retornos.",
            [
                new("Descuento", "calcularDescuento(1000, 15)", "150"),
                new("Final", "precioFinal(1000, 15)", "850"),
                new("Sin descuento", "precioFinal(500, 0)", "500"),
            ],
            [
                "La primera función solo calcula el importe descontado.",
                "La segunda puede llamar a la primera.",
                "Restá el resultado recibido al monto original.",
            ],
            [
                "composición de funciones",
                "parámetros",
                "return",
            ]),
        new(
            "Funciones",
            "Contar apariciones",
            "Creá una función que reciba un array y un valor y retorne cuántas veces aparece.",
            NivelDificultad.Intermedio, 5,
            "function contarApariciones(valores, buscado) {\n    // Tu código\n}",
            "Cuenta todas las coincidencias, incluyendo repeticiones consecutivas y ausencia.",
            "Recorre únicamente índices válidos y compara cada elemento con el valor buscado.",
            [
                new("Repetidos", "contarApariciones([1,2,1,1], 1)", "3"),
                new("Ausente", "contarApariciones([1,2,3], 5)", "0"),
                new("Strings", "contarApariciones([\"a\",\"b\",\"a\"], \"a\")", "2"),
            ],
            [
                "Usá un contador inicializado en cero.",
                "Revisá cada elemento del array.",
                "Incrementá solo cuando exista igualdad.",
            ],
            [
                "arrays",
                "parámetros",
                "contador",
            ]),
        new(
            "Funciones",
            "Buscar el mayor reutilizable",
            "Creá una función mayorDeDos(a,b) y utilizala desde mayorDeCuatro(a,b,c,d).",
            NivelDificultad.Intermedio, 6,
            "function mayorDeDos(a, b) {\n    // Tu código\n}\n\nfunction mayorDeCuatro(a, b, c, d) {\n    // Tu código\n}",
            "Obtiene correctamente el mayor de cuatro valores para cualquier orden.",
            "Reutiliza mayorDeDos en vez de duplicar toda la lógica y encadena retornos correctamente.",
            [
                new("Primero", "mayorDeCuatro(9,2,3,4)", "9"),
                new("Último", "mayorDeCuatro(1,2,3,8)", "8"),
                new("Negativos", "mayorDeCuatro(-5,-2,-9,-4)", "-2"),
            ],
            [
                "Resolvé primero el problema de dos valores.",
                "Compará los resultados parciales.",
                "Cada llamada devuelve un valor que puede usarse en otra llamada.",
            ],
            [
                "funciones auxiliares",
                "reutilización",
                "return",
            ]),
        new(
            "Funciones",
            "Validar fecha simplificada",
            "Separá en funciones la validación de mes, día y año. Considerá meses de 30/31 días y febrero de 28, sin años bisiestos.",
            NivelDificultad.Avanzado, 7,
            "function diasDelMes(mes) {\n    // Tu código\n}\n\nfunction fechaValida(dia, mes, anio) {\n    // Tu código\n}",
            "Valida correctamente día, mes y año dentro del modelo simplificado.",
            "Descompone la solución en funciones, utiliza el resultado de diasDelMes y controla límites inclusivos.",
            [
                new("Válida", "fechaValida(31,1,2026)", "true"),
                new("Febrero inválido", "fechaValida(29,2,2026)", "false"),
                new("Mes inválido", "fechaValida(10,13,2026)", "false"),
                new("Día cero", "fechaValida(0,5,2026)", "false"),
            ],
            [
                "Primero determiná cuántos días admite el mes.",
                "Un mes válido está entre 1 y 12.",
                "El día debe ser al menos 1 y no superar el máximo.",
            ],
            [
                "funciones auxiliares",
                "condicionales",
                "límites",
            ]),
        new(
            "Funciones",
            "Procesar una compra",
            "Separá en funciones subtotal, descuento, impuestos y total. Recibí precio, cantidad, porcentaje de descuento e IVA.",
            NivelDificultad.Desafio, 8,
            "function calcularSubtotal(precio, cantidad) {\n    // Tu código\n}\nfunction calcularDescuento(subtotal, porcentaje) {\n    // Tu código\n}\nfunction calcularImpuesto(base, iva) {\n    // Tu código\n}\nfunction calcularTotal(precio, cantidad, descuento, iva) {\n    // Tu código\n}",
            "Calcula el total aplicando descuento antes del impuesto y obtiene todos los importes correctamente.",
            "Reutiliza las funciones auxiliares, transmite los valores correctos y evita recalcular o aplicar conceptos dos veces.",
            [
                new("Habitual", "calcularTotal(100, 2, 10, 22)", "219.6"),
                new("Sin descuento", "calcularTotal(50, 3, 0, 22)", "183"),
                new("Sin IVA", "calcularTotal(80, 1, 25, 0)", "60"),
            ],
            [
                "Calculá primero el subtotal.",
                "La base del impuesto es el subtotal luego del descuento.",
                "Encadená los retornos de las funciones auxiliares.",
            ],
            [
                "composición",
                "subtotales",
                "porcentajes",
            ]),
        new(
            "Clases y objetos",
            "Clase Producto",
            "Creá Producto con nombre, precio y stock, y un método hayStock() que retorne si stock es mayor a cero.",
            NivelDificultad.Inicial, 1,
            "class Producto {\n    constructor(nombre, precio, stock) {\n        // Tu código\n    }\n\n    hayStock() {\n        // Tu código\n    }\n}",
            "Inicializa los tres atributos y hayStock refleja correctamente la disponibilidad.",
            "Utiliza this para conservar el estado del objeto y retorna un booleano desde el método.",
            [
                new("Con stock", "new Producto(\"Teclado\", 100, 2).hayStock()", "true"),
                new("Sin stock", "new Producto(\"Mouse\", 50, 0).hayStock()", "false"),
            ],
            [
                "El constructor recibe los datos iniciales.",
                "Guardá cada parámetro en una propiedad con this.",
                "El método compara el stock del propio objeto.",
            ],
            [
                "class",
                "constructor",
                "this",
                "métodos",
            ]),
        new(
            "Clases y objetos",
            "Clase Persona",
            "Creá Persona con nombre y edad, y un método esMayorDeEdad().",
            NivelDificultad.Inicial, 2,
            "class Persona {\n    constructor(nombre, edad) {\n        // Tu código\n    }\n\n    esMayorDeEdad() {\n        // Tu código\n    }\n}",
            "Representa correctamente nombre y edad y determina mayoría de edad con el límite correcto.",
            "El método consulta el estado de la instancia mediante this y retorna un booleano.",
            [
                new("Mayor", "new Persona(\"Ana\", 20).esMayorDeEdad()", "true"),
                new("Límite", "new Persona(\"Luis\", 18).esMayorDeEdad()", "true"),
                new("Menor", "new Persona(\"Eva\", 17).esMayorDeEdad()", "false"),
            ],
            [
                "El objeto debe recordar nombre y edad.",
                "El método no necesita parámetros: usa this.edad.",
                "Los 18 años cuentan como mayoría de edad.",
            ],
            [
                "class",
                "this",
                "métodos",
            ]),
        new(
            "Clases y objetos",
            "Clase Estudiante",
            "Creá Estudiante con nombre y array de notas. Agregá agregarNota(nota) y calcularPromedio().",
            NivelDificultad.Basico, 3,
            "class Estudiante {\n    constructor(nombre) {\n        // Tu código\n    }\n\n    agregarNota(nota) {\n        // Tu código\n    }\n\n    calcularPromedio() {\n        // Tu código\n    }\n}",
            "Permite agregar notas y calcula correctamente el promedio de las almacenadas.",
            "Inicializa el array en cada instancia, usa this y contempla que todavía no existan notas.",
            [
                new("Promedio", "const e = new Estudiante(\"Ana\"); e.agregarNota(8); e.agregarNota(10); e.calcularPromedio();", "9"),
                new("Sin notas", "new Estudiante(\"Ana\").calcularPromedio()", "0"),
            ],
            [
                "Cada estudiante necesita su propio array vacío.",
                "agregarNota incorpora un elemento al estado.",
                "El promedio requiere suma y cantidad de notas.",
            ],
            [
                "class",
                "arrays",
                "métodos",
                "this",
            ]),
        new(
            "Clases y objetos",
            "Clase CuentaBancaria",
            "Modelá titular y saldo. Implementá depositar(monto), retirar(monto) y consultarSaldo(); no permitas retirar más del saldo.",
            NivelDificultad.Basico, 4,
            "class CuentaBancaria {\n    constructor(titular, saldoInicial) {\n        // Tu código\n    }\n\n    depositar(monto) {\n        // Tu código\n    }\n\n    retirar(monto) {\n        // Tu código\n    }\n\n    consultarSaldo() {\n        // Tu código\n    }\n}",
            "Actualiza correctamente el saldo en depósitos y retiros válidos y conserva el saldo ante retiros inválidos.",
            "Encapsula las operaciones mediante métodos y utiliza el estado actual de la instancia.",
            [
                new("Depósito", "const c = new CuentaBancaria(\"A\",100); c.depositar(50); c.consultarSaldo();", "150"),
                new("Retiro", "const c = new CuentaBancaria(\"A\",100); c.retirar(40); c.consultarSaldo();", "60"),
                new("Exceso", "const c = new CuentaBancaria(\"A\",100); c.retirar(120); c.consultarSaldo();", "100"),
            ],
            [
                "Todas las operaciones modifican o consultan this.saldo.",
                "Antes de retirar, compará monto y saldo.",
                "Un retiro inválido no debe alterar el estado.",
            ],
            [
                "class",
                "estado",
                "condicionales",
                "métodos",
            ]),
        new(
            "Clases y objetos",
            "Clase Habitación",
            "Modelá tipo, precio y disponible. Implementá reservar() y liberar(), devolviendo si la operación pudo realizarse.",
            NivelDificultad.Intermedio, 5,
            "class Habitacion {\n    constructor(tipo, precio) {\n        // Tu código\n    }\n\n    reservar() {\n        // Tu código\n    }\n\n    liberar() {\n        // Tu código\n    }\n}",
            "Mantiene correctamente el estado de disponibilidad a través de reservas y liberaciones.",
            "Evita reservar dos veces, retorna resultados coherentes y modifica solo el estado de la instancia.",
            [
                new("Reserva", "const h = new Habitacion(\"Doble\",100); h.reservar();", "true"),
                new("Doble reserva", "const h = new Habitacion(\"Doble\",100); h.reservar(); h.reservar();", "false"),
                new("Liberar", "const h = new Habitacion(\"Doble\",100); h.reservar(); h.liberar();", "true"),
            ],
            [
                "Una habitación nueva comienza disponible.",
                "reservar solo puede cambiar un estado disponible.",
                "liberar debe restaurar la disponibilidad.",
            ],
            [
                "class",
                "estado booleano",
                "métodos",
            ]),
        new(
            "Clases y objetos",
            "Clase Película",
            "Modelá título, género y puntuaciones. Implementá agregarPuntuacion(valor) entre 1 y 5 y promedioPuntuaciones().",
            NivelDificultad.Intermedio, 6,
            "class Pelicula {\n    constructor(titulo, genero) {\n        // Tu código\n    }\n\n    agregarPuntuacion(valor) {\n        // Tu código\n    }\n\n    promedioPuntuaciones() {\n        // Tu código\n    }\n}",
            "Almacena únicamente puntuaciones válidas y calcula su promedio correctamente.",
            "Cada instancia mantiene su propia colección, valida límites y contempla una colección vacía.",
            [
                new("Promedio", "const p = new Pelicula(\"X\",\"Drama\"); p.agregarPuntuacion(4); p.agregarPuntuacion(5); p.promedioPuntuaciones();", "4.5"),
                new("Inválida", "const p = new Pelicula(\"X\",\"Drama\"); p.agregarPuntuacion(8); p.promedioPuntuaciones();", "0"),
            ],
            [
                "Inicializá un array vacío en el constructor.",
                "Solo agregues valores entre 1 y 5 inclusive.",
                "Sin puntuaciones, el promedio puede ser 0.",
            ],
            [
                "class",
                "arrays",
                "validación",
                "promedio",
            ]),
        new(
            "Clases y objetos",
            "Carrito de compras",
            "Creá Carrito con items. Agregá agregarProducto(producto, cantidad), calcularSubtotal() y cantidadTotal(); rechazá cantidades no positivas.",
            NivelDificultad.Avanzado, 7,
            "class Carrito {\n    constructor() {\n        // Tu código\n    }\n\n    agregarProducto(producto, cantidad) {\n        // Tu código\n    }\n\n    calcularSubtotal() {\n        // Tu código\n    }\n\n    cantidadTotal() {\n        // Tu código\n    }\n}",
            "Gestiona productos y cantidades y calcula subtotal y cantidad total correctamente.",
            "Modela cada item de forma consistente, valida cantidades y recorre el estado sin duplicar cálculos.",
            [
                new("Subtotal", "const c = new Carrito(); c.agregarProducto({precio:100},2); c.calcularSubtotal();", "200"),
                new("Cantidad", "const c = new Carrito(); c.agregarProducto({precio:100},2); c.agregarProducto({precio:50},3); c.cantidadTotal();", "5"),
                new("Inválida", "const c = new Carrito(); c.agregarProducto({precio:100},0); c.cantidadTotal();", "0"),
            ],
            [
                "Cada item debe relacionar producto y cantidad.",
                "El subtotal suma precio por cantidad.",
                "Una cantidad cero o negativa no debe agregarse.",
            ],
            [
                "class",
                "arrays de objetos",
                "acumuladores",
                "validación",
            ]),
        new(
            "Clases y objetos",
            "Sistema de biblioteca",
            "Modelá Libro con disponible y Biblioteca con colección. Implementá agregarLibro, prestar(isbn) y devolver(isbn).",
            NivelDificultad.Desafio, 8,
            "class Libro {\n    constructor(isbn, titulo) {\n        // Tu código\n    }\n}\n\nclass Biblioteca {\n    constructor() {\n        // Tu código\n    }\n\n    agregarLibro(libro) {\n        // Tu código\n    }\n\n    prestar(isbn) {\n        // Tu código\n    }\n\n    devolver(isbn) {\n        // Tu código\n    }\n}",
            "Gestiona alta, préstamo y devolución respetando existencia y disponibilidad.",
            "Distribuye responsabilidades entre clases, localiza libros por ISBN y conserva estados coherentes ante operaciones inválidas.",
            [
                new("Préstamo", "const b=new Biblioteca(); b.agregarLibro(new Libro(\"1\",\"A\")); b.prestar(\"1\");", "true"),
                new("Doble préstamo", "const b=new Biblioteca(); b.agregarLibro(new Libro(\"1\",\"A\")); b.prestar(\"1\"); b.prestar(\"1\");", "false"),
                new("Devolución", "const b=new Biblioteca(); b.agregarLibro(new Libro(\"1\",\"A\")); b.prestar(\"1\"); b.devolver(\"1\");", "true"),
                new("Inexistente", "new Biblioteca().prestar(\"9\")", "false"),
            ],
            [
                "Libro debe conocer su disponibilidad.",
                "Biblioteca mantiene una colección de libros.",
                "Antes de cambiar estado, buscá por ISBN y validá la operación.",
            ],
            [
                "múltiples clases",
                "colecciones",
                "estado",
                "búsqueda",
            ]),
    ];

    private sealed record LogroDef(
        string Nombre,
        string Descripcion,
        string Icono,
        TipoLogro Tipo,
        int ValorObjetivo);

    private sealed record TemaDef(string Nombre, string Descripcion, int Orden);
    private sealed record CasoDef(string Nombre, string Entrada, string SalidaEsperada);
    private sealed record DefinicionEjercicio(
        string Tema, string Titulo, string Consigna, NivelDificultad Dificultad,
        int Orden, string CodigoInicial, string CriterioFuncional,
        string CriterioTecnico, CasoDef[] Casos, string[] Pistas,
        string[] Conceptos, string[]? ConceptosNoPermitidos = null);
}