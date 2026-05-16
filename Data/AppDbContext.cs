// ============================================================
//  AppDbContext — ERP RRHH Guatemala
//  Mapeo explícito PascalCase → snake_case para MySQL
// ============================================================
using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<CatTipoRetencion> CatTiposRetencion { get; set; }
        public DbSet<CatRetencionFiscal> CatRetencionesFiscales { get; set; }
        public DbSet<CatIsrTramo> CatIsrTramos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Nomina> Nominas { get; set; }
        public DbSet<NominaDetalleEmpleado> NominaDetalleEmpleados { get; set; }
        public DbSet<NominaRetencionAplicada> NominaRetencioneAplicadas { get; set; }
        public DbSet<NominaAlerta> NominaAlertas { get; set; }
        public DbSet<CatDepartamento> CatDepartamentos { get; set; }
        public DbSet<CatPuesto> CatPuestos { get; set; }
        public DbSet<EmpleadoDocumento> EmpleadoDocumentos { get; set; }
        public DbSet<EmpleadoContactoEmergencia> ContactosEmergencia { get; set; }
        public DbSet<EmpleadoHistorial> EmpleadoHistorial { get; set; }
        public DbSet<OnboardingChecklist> OnboardingChecklist { get; set; }
        public DbSet<OnboardingPlantilla> OnboardingPlantilla { get; set; }
        public DbSet<Vacante> Vacantes { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<CandidatoHistorialEtapa> CandidatoHistorialEtapas { get; set; }
        public DbSet<ReclutamientoComunicacion> ReclutamientoComunicaciones { get; set; }
        public DbSet<ReclutamientoPlantillaEmail> PlantillasEmail { get; set; }
        public DbSet<CatNormativa> CatNormativas { get; set; }
        public DbSet<CumplimientoAlerta> CumplimientoAlertas { get; set; }
        public DbSet<CumplimientoReporte> CumplimientoReportes { get; set; }
        public DbSet<NormativaHistorialCambio> NormativaHistorialCambios { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<EmpleadoHorario> EmpleadoHorarios { get; set; }
        public DbSet<AsistenciaRegistro> AsistenciaRegistros { get; set; }
        public DbSet<HorasExtrasSolicitud> HorasExtrasSolicitudes { get; set; }
        public DbSet<Ausencia> Ausencias { get; set; }
        public DbSet<EvaluacionCiclo> EvaluacionCiclos { get; set; }
        public DbSet<EvaluacionCriterio> EvaluacionCriterios { get; set; }
        public DbSet<EvaluacionParticipante> EvaluacionParticipantes { get; set; }
        public DbSet<EvaluacionCalificacion> EvaluacionCalificaciones { get; set; }
        public DbSet<EvaluacionResultado> EvaluacionResultados { get; set; }
        public DbSet<EvaluacionRecordatorio> EvaluacionRecordatorios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PortalSolicitud> PortalSolicitudes { get; set; }
        public DbSet<PortalActualizacion> PortalActualizaciones { get; set; }
        public DbSet<PortalNotificacion> PortalNotificaciones { get; set; }
        public DbSet<CatTipoBeneficio> CatTiposBeneficio { get; set; }
        public DbSet<BeneficioPaquete> BeneficioPaquetes { get; set; }
        public DbSet<BeneficioItem> BeneficioItems { get; set; }
        public DbSet<BeneficioAsignacion> BeneficioAsignaciones { get; set; }
        public DbSet<BeneficioAlerta> BeneficioAlertas { get; set; }
        public DbSet<CapacitacionPlan> CapacitacionPlanes { get; set; }
        public DbSet<CapacitacionAsignacion> CapacitacionAsignaciones { get; set; }
        public DbSet<CapacitacionEvidencia> CapacitacionEvidencias { get; set; }
        public DbSet<CapacitacionAlerta> CapacitacionAlertas { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── cat_tipos_retencion ───────────────────────────
            mb.Entity<CatTipoRetencion>(e =>
            {
                e.ToTable("cat_tipos_retencion");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Codigo).HasColumnName("codigo");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.AplicaA).HasColumnName("aplica_a");
                e.Property(x => x.Activo).HasColumnName("activo");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            // ── cat_retenciones_fiscales ──────────────────────
            mb.Entity<CatRetencionFiscal>(e =>
            {
                e.ToTable("cat_retenciones_fiscales");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.TipoRetencionId).HasColumnName("tipo_retencion_id");
                e.Property(x => x.Codigo).HasColumnName("codigo");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.MetodoCalculo).HasColumnName("metodo_calculo");
                e.Property(x => x.TasaPorcentaje).HasColumnName("tasa_porcentaje");
                e.Property(x => x.MontoFijo).HasColumnName("monto_fijo");
                e.Property(x => x.BaseCalculo).HasColumnName("base_calculo");
                e.Property(x => x.ReferenciaLegal).HasColumnName("referencia_legal");
                e.Property(x => x.VigenteDe).HasColumnName("vigente_desde");
                e.Property(x => x.VigenteHasta).HasColumnName("vigente_hasta");
                e.Property(x => x.Activo).HasColumnName("activo");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            // ── cat_isr_tramos ────────────────────────────────
            mb.Entity<CatIsrTramo>(e =>
            {
                e.ToTable("cat_isr_tramos");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.RangoDesdeGtq).HasColumnName("rango_desde_gtq");
                e.Property(x => x.RangoHastaGtq).HasColumnName("rango_hasta_gtq");
                e.Property(x => x.TasaPorcentaje).HasColumnName("tasa_porcentaje");
                e.Property(x => x.CuotaFijaGtq).HasColumnName("cuota_fija_gtq");
                e.Property(x => x.VigenteDe).HasColumnName("vigente_desde");
                e.Property(x => x.VigenteHasta).HasColumnName("vigente_hasta");
                e.Property(x => x.ReferenciaLegal).HasColumnName("referencia_legal");
            });

            // ── empleados ─────────────────────────────────────
            mb.Entity<Empleado>(e =>
            {
                e.ToTable("empleados");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.CodigoEmpleado).HasColumnName("codigo_empleado");
                e.Property(x => x.Nombres).HasColumnName("nombres");
                e.Property(x => x.Apellidos).HasColumnName("apellidos");
                e.Property(x => x.Dpi).HasColumnName("dpi");
                e.Property(x => x.Nit).HasColumnName("nit");
                e.Property(x => x.NoIgss).HasColumnName("no_igss");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
                e.Property(x => x.Genero).HasColumnName("genero");
                e.Property(x => x.Municipio).HasColumnName("municipio");
                e.Property(x => x.Departamento).HasColumnName("departamento");
                e.Property(x => x.Puesto).HasColumnName("puesto");
                e.Property(x => x.DepartamentoArea).HasColumnName("departamento_area");
                e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
                e.Property(x => x.TipoEmpleado).HasColumnName("tipo_empleado");
                e.Property(x => x.Estado).HasColumnName("estado");
                e.Property(x => x.FotoUrl).HasColumnName("foto_url");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
                e.Ignore(x => x.NombreCompleto);
                e.Property(x => x.EstadoCivil).HasColumnName("estado_civil");
                e.Property(x => x.Nacionalidad).HasColumnName("nacionalidad");
                e.Property(x => x.Direccion).HasColumnName("direccion");
            });

            // ── contratos ─────────────────────────────────────
            mb.Entity<Contrato>(e =>
            {
                e.ToTable("contratos");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.CodigoContrato).HasColumnName("codigo_contrato");
                e.Property(x => x.TipoContrato).HasColumnName("tipo_contrato");
                e.Property(x => x.SalarioBase).HasColumnName("salario_base");
                e.Property(x => x.BonificacionDecreto).HasColumnName("bonificacion_decreto");
                e.Property(x => x.OtrasBonificaciones).HasColumnName("otras_bonificaciones");
                e.Property(x => x.Jornada).HasColumnName("jornada");
                e.Property(x => x.HorasSemana).HasColumnName("horas_semana");
                e.Property(x => x.Moneda).HasColumnName("moneda");
                e.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
                e.Property(x => x.FechaFin).HasColumnName("fecha_fin");
                e.Property(x => x.Vigente).HasColumnName("vigente");
                e.Property(x => x.Observaciones).HasColumnName("observaciones");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
                e.Property(x => x.LugarTrabajo).HasColumnName("lugar_trabajo");
            });

            // ── nominas ───────────────────────────────────────
            mb.Entity<Nomina>(e =>
            {
                e.ToTable("nominas");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.CodigoNomina).HasColumnName("codigo_nomina");
                e.Property(x => x.PeriodoInicio).HasColumnName("periodo_inicio");
                e.Property(x => x.PeriodoFin).HasColumnName("periodo_fin");
                e.Property(x => x.TipoPeriodo).HasColumnName("tipo_periodo");
                e.Property(x => x.FechaPago).HasColumnName("fecha_pago");
                e.Property(x => x.Estado).HasColumnName("estado");
                e.Property(x => x.TotalEmpleados).HasColumnName("total_empleados");
                e.Property(x => x.TotalBrutoGtq).HasColumnName("total_bruto_gtq");
                e.Property(x => x.TotalDeduccionesGtq).HasColumnName("total_deducciones_gtq");
                e.Property(x => x.TotalNetoGtq).HasColumnName("total_neto_gtq");
                e.Property(x => x.TotalCuotaPatronal).HasColumnName("total_cuota_patronal");
                e.Property(x => x.Observaciones).HasColumnName("observaciones");
                e.Property(x => x.AprobadoPor).HasColumnName("aprobado_por");
                e.Property(x => x.FechaAprobacion).HasColumnName("fecha_aprobacion");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
                e.Property(x => x.CreatedBy).HasColumnName("created_by");
                e.HasIndex(x => x.CodigoNomina).IsUnique();
                e.HasMany(x => x.Detalles)
                    .WithOne(d => d.Nomina)
                    .HasForeignKey(d => d.NominaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── nomina_detalle_empleado ───────────────────────
            mb.Entity<NominaDetalleEmpleado>(e =>
            {
                e.ToTable("nomina_detalle_empleado");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.NominaId).HasColumnName("nomina_id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.ContratoId).HasColumnName("contrato_id");
                e.Property(x => x.SalarioBase).HasColumnName("salario_base");
                e.Property(x => x.BonificacionDecreto).HasColumnName("bonificacion_decreto");
                e.Property(x => x.OtrasBonificaciones).HasColumnName("otras_bonificaciones");
                e.Property(x => x.HorasExtrasMonto).HasColumnName("horas_extras_monto");
                e.Property(x => x.OtrosIngresos).HasColumnName("otros_ingresos");
                e.Property(x => x.TotalIngresosBruto).HasColumnName("total_ingresos_bruto");
                e.Property(x => x.TotalDeduccionesEmpleado).HasColumnName("total_deducciones_empleado");
                e.Property(x => x.TotalCuotaPatronal).HasColumnName("total_cuota_patronal");
                e.Property(x => x.TotalNeto).HasColumnName("total_neto");
                e.Property(x => x.Estado).HasColumnName("estado");
                e.Property(x => x.Observaciones).HasColumnName("observaciones");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
                e.HasIndex(x => new { x.NominaId, x.EmpleadoId }).IsUnique();
                e.HasMany(x => x.Retenciones)
                    .WithOne(r => r.Detalle)
                    .HasForeignKey(r => r.NominaDetalleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── nomina_retenciones_aplicadas ──────────────────
            mb.Entity<NominaRetencionAplicada>(e =>
            {
                e.ToTable("nomina_retenciones_aplicadas");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.NominaDetalleId).HasColumnName("nomina_detalle_id");
                e.Property(x => x.RetencionId).HasColumnName("retencion_id");
                e.Property(x => x.CodigoRetencion).HasColumnName("codigo_retencion");
                e.Property(x => x.NombreRetencion).HasColumnName("nombre_retencion");
                e.Property(x => x.ReferenciaLegal).HasColumnName("referencia_legal");
                e.Property(x => x.MetodoCalculo).HasColumnName("metodo_calculo");
                e.Property(x => x.TasaAplicada).HasColumnName("tasa_aplicada");
                e.Property(x => x.BaseCalculoMonto).HasColumnName("base_calculo_monto");
                e.Property(x => x.MontoRetenido).HasColumnName("monto_retenido");
                e.Property(x => x.EsDeduccionEmpleado).HasColumnName("es_deduccion_empleado");
                e.Property(x => x.EsCuotaPatronal).HasColumnName("es_cuota_patronal");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
            });

            // ── nomina_alertas ────────────────────────────────
            mb.Entity<NominaAlerta>(e =>
            {
                e.ToTable("nomina_alertas");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.NominaId).HasColumnName("nomina_id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.TipoAlerta).HasColumnName("tipo_alerta");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.Resuelta).HasColumnName("resuelta");
                e.Property(x => x.ResueltaPor).HasColumnName("resuelta_por");
                e.Property(x => x.FechaResolucion).HasColumnName("fecha_resolucion");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── cat_departamentos ─────────────────────────────────────
            mb.Entity<CatDepartamento>(e =>
            {
                e.ToTable("cat_departamentos");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Codigo).HasColumnName("codigo");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.Activo).HasColumnName("activo");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── cat_puestos ───────────────────────────────────────────
            mb.Entity<CatPuesto>(e =>
            {
                e.ToTable("cat_puestos");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.DepartamentoId).HasColumnName("departamento_id");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Codigo).HasColumnName("codigo");
                e.Property(x => x.NivelSalarial).HasColumnName("nivel_salarial");
                e.Property(x => x.SalarioMinimo).HasColumnName("salario_minimo");
                e.Property(x => x.SalarioMaximo).HasColumnName("salario_maximo");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.Activo).HasColumnName("activo");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── empleado_documentos ───────────────────────────────────
            mb.Entity<EmpleadoDocumento>(e =>
            {
                e.ToTable("empleado_documentos");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.TipoDocumento).HasColumnName("tipo_documento");
                e.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo");
                e.Property(x => x.UrlArchivo).HasColumnName("url_archivo");
                e.Property(x => x.FechaVence).HasColumnName("fecha_vence");
                e.Property(x => x.Verificado).HasColumnName("verificado");
                e.Property(x => x.VerificadoPor).HasColumnName("verificado_por");
                e.Property(x => x.Notas).HasColumnName("notas");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── empleado_contactos_emergencia ─────────────────────────
            mb.Entity<EmpleadoContactoEmergencia>(e =>
            {
                e.ToTable("empleado_contactos_emergencia");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.NombreCompleto).HasColumnName("nombre_completo");
                e.Property(x => x.Parentesco).HasColumnName("parentesco");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.TelefonoAlt).HasColumnName("telefono_alt");
                e.Property(x => x.EsPrincipal).HasColumnName("es_principal");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── empleado_historial ────────────────────────────────────
            mb.Entity<EmpleadoHistorial>(e =>
            {
                e.ToTable("empleado_historial");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.TipoCambio).HasColumnName("tipo_cambio");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.ValorAnterior).HasColumnName("valor_anterior");
                e.Property(x => x.ValorNuevo).HasColumnName("valor_nuevo");
                e.Property(x => x.RealizadoPor).HasColumnName("realizado_por");
                e.Property(x => x.FechaCambio).HasColumnName("fecha_cambio").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── onboarding_checklist ──────────────────────────────────
            mb.Entity<OnboardingChecklist>(e =>
            {
                e.ToTable("onboarding_checklist");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.Tarea).HasColumnName("tarea");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.Responsable).HasColumnName("responsable");
                e.Property(x => x.FechaLimite).HasColumnName("fecha_limite");
                e.Property(x => x.Completada).HasColumnName("completada");
                e.Property(x => x.FechaCompletada).HasColumnName("fecha_completada");
                e.Property(x => x.Orden).HasColumnName("orden");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── onboarding_plantilla ──────────────────────────────────
            mb.Entity<OnboardingPlantilla>(e =>
            {
                e.ToTable("onboarding_plantilla");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Tarea).HasColumnName("tarea");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.Responsable).HasColumnName("responsable");
                e.Property(x => x.DiasLimite).HasColumnName("dias_limite");
                e.Property(x => x.Orden).HasColumnName("orden");
                e.Property(x => x.Activa).HasColumnName("activa");
            });
        }
    }
}