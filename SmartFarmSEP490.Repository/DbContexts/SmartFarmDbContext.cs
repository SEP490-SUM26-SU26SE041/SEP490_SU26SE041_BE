using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using Task = SmartFarmSEP490.Model.Task;

namespace SmartFarmSEP490.Repository.DbContexts;

public partial class SmartFarmDbContext : DbContext
{
    public SmartFarmDbContext()
    {
    }

    public SmartFarmDbContext(DbContextOptions<SmartFarmDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AitaskAssignmentSuggestion> AitaskAssignmentSuggestions { get; set; }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Bed> Beds { get; set; }

    public virtual DbSet<CareSchedule> CareSchedules { get; set; }

    public virtual DbSet<Crop> Crops { get; set; }

    public virtual DbSet<CropVariety> CropVarieties { get; set; }

    public virtual DbSet<Experiment> Experiments { get; set; }

    public virtual DbSet<ExperimentBedAssignment> ExperimentBedAssignments { get; set; }

    public virtual DbSet<ExperimentDesign> ExperimentDesigns { get; set; }

    public virtual DbSet<ExperimentGroup> ExperimentGroups { get; set; }

    public virtual DbSet<ExperimentReport> ExperimentReports { get; set; }

    public virtual DbSet<ExperimentRequest> ExperimentRequests { get; set; }

    public virtual DbSet<ExperimentStage> ExperimentStages { get; set; }

    public virtual DbSet<Farm> Farms { get; set; }

    public virtual DbSet<KnowledgeDocument> KnowledgeDocuments { get; set; }

    public virtual DbSet<KnowledgeDocumentChunk> KnowledgeDocumentChunks { get; set; }

    public virtual DbSet<MeasurementDefinition> MeasurementDefinitions { get; set; }

    public virtual DbSet<MeasurementRecord> MeasurementRecords { get; set; }

    public virtual DbSet<PlantHealthAssessment> PlantHealthAssessments { get; set; }

    public virtual DbSet<PlantImage> PlantImages { get; set; }

    public virtual DbSet<ProcedureTemplate> ProcedureTemplates { get; set; }

    public virtual DbSet<ProcedureTemplateStep> ProcedureTemplateSteps { get; set; }

    public virtual DbSet<RequestReview> RequestReviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sensor> Sensors { get; set; }

    public virtual DbSet<SensorDatum> SensorData { get; set; }

    public virtual DbSet<SensorThresholdRule> SensorThresholdRules { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAssignment> TaskAssignments { get; set; }

    public virtual DbSet<TaskReport> TaskReports { get; set; }

    public virtual DbSet<TaskSkillRequirement> TaskSkillRequirements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSkill> UserSkills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("AIReviewStatus", new[] { "Suggested", "Accepted", "Rejected", "Adjusted" })
            .HasPostgresEnum("AlertSeverity", new[] { "Low", "Medium", "High", "Critical" })
            .HasPostgresEnum("BatchStatus", new[] { "Planned", "Growing", "Harvested", "Discarded", "Completed" })
            .HasPostgresEnum("DesignType", new[] { "CompletelyRandomized", "RandomizedCompleteBlock", "Factorial", "Observational", "Other" })
            .HasPostgresEnum("DocumentStatus", new[] { "Draft", "Indexed", "Archived" })
            .HasPostgresEnum("ExperimentStageType", new[] { "Nursery", "Care", "Growth", "Harvest", "Evaluation", "Other" })
            .HasPostgresEnum("ExperimentStatus", new[] { "Draft", "Approved", "Active", "Completed", "Cancelled" })
            .HasPostgresEnum("GroupType", new[] { "Control", "Treatment" })
            .HasPostgresEnum("LocationStatus", new[] { "Available", "InUse", "Maintenance", "Unavailable" })
            .HasPostgresEnum("RequestStatus", new[] { "Pending", "Approved", "Rejected", "Cancelled" })
            .HasPostgresEnum("ReviewResult", new[] { "Approved", "Rejected" })
            .HasPostgresEnum("SensorType", new[] { "Temperature", "Humidity", "SoilMoisture", "Light", "PH", "Other" })
            .HasPostgresEnum("TaskAssignmentStatus", new[] { "Assigned", "Reassigned", "Resigned", "Completed", "Cancelled" })
            .HasPostgresEnum("TaskStatus", new[] { "Pending", "InProgress", "Completed", "Overdue", "Cancelled" })
            .HasPostgresEnum("TaskType", new[] { "Planting", "Watering", "Fertilizing", "Observation", "Inspection", "Harvest", "Other" });

        modelBuilder.Entity<AitaskAssignmentSuggestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AITaskAssignmentSuggestions_pkey");

            entity.ToTable("AITaskAssignmentSuggestions");

            entity.HasIndex(e => e.TaskId, "IX_AITaskAssignmentSuggestions_TaskId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.MatchScore).HasPrecision(5, 4);

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.AitaskAssignmentSuggestionReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("AITaskAssignmentSuggestions_ReviewedBy_fkey");

            entity.HasOne(d => d.SuggestedUser).WithMany(p => p.AitaskAssignmentSuggestionSuggestedUsers)
                .HasForeignKey(d => d.SuggestedUserId)
                .HasConstraintName("AITaskAssignmentSuggestions_SuggestedUserId_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.AitaskAssignmentSuggestions)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("AITaskAssignmentSuggestions_TaskId_fkey");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Alerts_pkey");

            entity.HasIndex(e => new { e.ExperimentId, e.IsResolved }, "IX_Alerts_Experiment_Resolved");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsResolved).HasDefaultValue(false);
            entity.Property(e => e.ResolvedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Batch).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Alerts_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Alerts_ExperimentId_fkey");

            entity.HasOne(d => d.Sensor).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.SensorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Alerts_SensorId_fkey");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Areas_pkey");

            entity.HasIndex(e => e.FarmId, "IX_Areas_FarmId");

            entity.HasIndex(e => new { e.FarmId, e.AreaCode }, "UQ_Areas_Farm_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AreaCode).HasMaxLength(50);
            entity.Property(e => e.AreaName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.EnvironmentType).HasMaxLength(50);
            entity.Property(e => e.TotalArea).HasPrecision(10, 2);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Farm).WithMany(p => p.Areas)
                .HasForeignKey(d => d.FarmId)
                .HasConstraintName("Areas_FarmId_fkey");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Batches_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_Batches_ExperimentId");

            entity.HasIndex(e => e.GroupId, "IX_Batches_GroupId");

            entity.HasIndex(e => new { e.ExperimentId, e.BatchCode }, "UQ_Batches_Experiment_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BatchCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.CropVariety).WithMany(p => p.Batches)
                .HasForeignKey(d => d.CropVarietyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Batches_CropVarietyId_fkey");

            entity.HasOne(d => d.ExperimentBedAssignment).WithMany(p => p.Batches)
                .HasForeignKey(d => d.ExperimentBedAssignmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Batches_ExperimentBedAssignmentId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.Batches)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("Batches_ExperimentId_fkey");

            entity.HasOne(d => d.Group).WithMany(p => p.Batches)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Batches_GroupId_fkey");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Beds_pkey");

            entity.HasIndex(e => e.AreaId, "IX_Beds_AreaId");

            entity.HasIndex(e => new { e.AreaId, e.BedCode }, "UQ_Beds_Area_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BedCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Length).HasPrecision(10, 2);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Width).HasPrecision(10, 2);

            entity.HasOne(d => d.Area).WithMany(p => p.Beds)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("Beds_AreaId_fkey");
        });

        modelBuilder.Entity<CareSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CareSchedules_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_CareSchedules_ExperimentId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Batch).WithMany(p => p.CareSchedules)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("CareSchedules_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.CareSchedules)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("CareSchedules_ExperimentId_fkey");

            entity.HasOne(d => d.ExperimentStage).WithMany(p => p.CareSchedules)
                .HasForeignKey(d => d.ExperimentStageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("CareSchedules_ExperimentStageId_fkey");
        });

        modelBuilder.Entity<Crop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Crops_pkey");

            entity.HasIndex(e => e.CropName, "Crops_CropName_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CropName).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ScientificName).HasMaxLength(150);
        });

        modelBuilder.Entity<CropVariety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CropVarieties_pkey");

            entity.HasIndex(e => e.CropId, "IX_CropVarieties_CropId");

            entity.HasIndex(e => new { e.CropId, e.VarietyName }, "UQ_CropVarieties_Crop_Name").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Origin).HasMaxLength(100);
            entity.Property(e => e.VarietyName).HasMaxLength(100);

            entity.HasOne(d => d.Crop).WithMany(p => p.CropVarieties)
                .HasForeignKey(d => d.CropId)
                .HasConstraintName("CropVarieties_CropId_fkey");
        });

        modelBuilder.Entity<Experiment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Experiments_pkey");

            entity.HasIndex(e => e.ExperimentCode, "Experiments_ExperimentCode_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExperimentCode).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.CropVariety).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.CropVarietyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Experiments_CropVarietyId_fkey");

            entity.HasOne(d => d.Farm).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.FarmId)
                .HasConstraintName("Experiments_FarmId_fkey");

            entity.HasOne(d => d.ProcedureTemplate).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.ProcedureTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Experiments_ProcedureTemplateId_fkey");

            entity.HasOne(d => d.Request).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Experiments_RequestId_fkey");

            entity.HasOne(d => d.Researcher).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.ResearcherId)
                .HasConstraintName("Experiments_ResearcherId_fkey");
        });

        modelBuilder.Entity<ExperimentBedAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentBedAssignments_pkey");

            entity.HasIndex(e => e.BedId, "IX_ExperimentBedAssignments_BedId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.Bed).WithMany(p => p.ExperimentBedAssignments)
                .HasForeignKey(d => d.BedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ExperimentBedAssignments_BedId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentBedAssignments)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("ExperimentBedAssignments_ExperimentId_fkey");
        });

        modelBuilder.Entity<ExperimentDesign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentDesigns_pkey");

            entity.HasIndex(e => e.ExperimentId, "ExperimentDesigns_ExperimentId_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DesignParameters).HasColumnType("jsonb");

            entity.HasOne(d => d.Experiment).WithOne(p => p.ExperimentDesign)
                .HasForeignKey<ExperimentDesign>(d => d.ExperimentId)
                .HasConstraintName("ExperimentDesigns_ExperimentId_fkey");
        });

        modelBuilder.Entity<ExperimentGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentGroups_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_ExperimentGroups_ExperimentId");

            entity.HasIndex(e => new { e.ExperimentId, e.GroupName }, "UQ_ExperimentGroups_Name").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.GroupName).HasMaxLength(100);

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentGroups)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("ExperimentGroups_ExperimentId_fkey");
        });

        modelBuilder.Entity<ExperimentReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentReports_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_ExperimentReports_ExperimentId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExportFormat).HasMaxLength(20);
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Summary'::character varying");
            entity.Property(e => e.ResultData).HasColumnType("jsonb");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ExperimentReports)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ExperimentReports_CreatedBy_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentReports)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("ExperimentReports_ExperimentId_fkey");
        });

        modelBuilder.Entity<ExperimentRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentRequests_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.MonitoringPlan).HasColumnType("jsonb");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.CropVariety).WithMany(p => p.ExperimentRequests)
                .HasForeignKey(d => d.CropVarietyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ExperimentRequests_CropVarietyId_fkey");

            entity.HasOne(d => d.Farm).WithMany(p => p.ExperimentRequests)
                .HasForeignKey(d => d.FarmId)
                .HasConstraintName("ExperimentRequests_FarmId_fkey");

            entity.HasOne(d => d.ProcedureTemplate).WithMany(p => p.ExperimentRequests)
                .HasForeignKey(d => d.ProcedureTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ExperimentRequests_ProcedureTemplateId_fkey");

            entity.HasOne(d => d.Researcher).WithMany(p => p.ExperimentRequests)
                .HasForeignKey(d => d.ResearcherId)
                .HasConstraintName("ExperimentRequests_ResearcherId_fkey");
        });

        modelBuilder.Entity<ExperimentStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ExperimentStages_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_ExperimentStages_ExperimentId");

            entity.HasIndex(e => new { e.ExperimentId, e.StageOrder }, "UQ_ExperimentStages_Order").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ResultData).HasColumnType("jsonb");
            entity.Property(e => e.StageName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentStages)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("ExperimentStages_ExperimentId_fkey");
        });

        modelBuilder.Entity<Farm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Farms_pkey");

            entity.HasIndex(e => e.FarmCode, "Farms_FarmCode_key").IsUnique();

            entity.HasIndex(e => e.ManagerId, "IX_Farms_ManagerId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FarmCode).HasMaxLength(50);
            entity.Property(e => e.FarmName).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Manager).WithMany(p => p.Farms)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Farms_ManagerId_fkey");
        });

        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("KnowledgeDocuments_pkey");

            entity.HasIndex(e => e.CropVarietyId, "IX_KnowledgeDocuments_CropVarietyId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CropVariety).WithMany(p => p.KnowledgeDocuments)
                .HasForeignKey(d => d.CropVarietyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("KnowledgeDocuments_CropVarietyId_fkey");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.KnowledgeDocuments)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("KnowledgeDocuments_UploadedBy_fkey");
        });

        modelBuilder.Entity<KnowledgeDocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("KnowledgeDocumentChunks_pkey");

            entity.HasIndex(e => e.DocumentId, "IX_KnowledgeDocumentChunks_DocumentId");

            entity.HasIndex(e => new { e.DocumentId, e.ChunkIndex }, "UQ_KnowledgeDocumentChunks_Index").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            entity.HasOne(d => d.Document).WithMany(p => p.KnowledgeDocumentChunks)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("KnowledgeDocumentChunks_DocumentId_fkey");
        });

        modelBuilder.Entity<MeasurementDefinition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MeasurementDefinitions_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_MeasurementDefinitions_ExperimentId");

            entity.HasIndex(e => new { e.ExperimentId, e.GroupId, e.MetricName }, "UQ_MeasurementDefinitions_Scope").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.MetricName).HasMaxLength(100);
            entity.Property(e => e.TargetValue).HasPrecision(12, 4);
            entity.Property(e => e.Unit).HasMaxLength(30);

            entity.HasOne(d => d.Experiment).WithMany(p => p.MeasurementDefinitions)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("MeasurementDefinitions_ExperimentId_fkey");

            entity.HasOne(d => d.Group).WithMany(p => p.MeasurementDefinitions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("MeasurementDefinitions_GroupId_fkey");
        });

        modelBuilder.Entity<MeasurementRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MeasurementRecords_pkey");

            entity.HasIndex(e => new { e.BatchId, e.MeasuredAt }, "IX_MeasurementRecords_Batch_MeasuredAt");

            entity.HasIndex(e => e.MeasurementDefinitionId, "IX_MeasurementRecords_DefinitionId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExtraData).HasColumnType("jsonb");
            entity.Property(e => e.MeasuredAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Value).HasPrecision(12, 4);

            entity.HasOne(d => d.Batch).WithMany(p => p.MeasurementRecords)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("MeasurementRecords_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.MeasurementRecords)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("MeasurementRecords_ExperimentId_fkey");

            entity.HasOne(d => d.ExperimentStage).WithMany(p => p.MeasurementRecords)
                .HasForeignKey(d => d.ExperimentStageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("MeasurementRecords_ExperimentStageId_fkey");

            entity.HasOne(d => d.MeasuredByNavigation).WithMany(p => p.MeasurementRecords)
                .HasForeignKey(d => d.MeasuredBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("MeasurementRecords_MeasuredBy_fkey");

            entity.HasOne(d => d.MeasurementDefinition).WithMany(p => p.MeasurementRecords)
                .HasForeignKey(d => d.MeasurementDefinitionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("MeasurementRecords_MeasurementDefinitionId_fkey");
        });

        modelBuilder.Entity<PlantHealthAssessment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PlantHealthAssessments_pkey");

            entity.HasIndex(e => e.BatchId, "IX_PlantHealthAssessments_BatchId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Aiconfidence)
                .HasPrecision(5, 4)
                .HasColumnName("AIConfidence");
            entity.Property(e => e.AimodelName)
                .HasMaxLength(100)
                .HasColumnName("AIModelName");
            entity.Property(e => e.Aisuggestion).HasColumnName("AISuggestion");
            entity.Property(e => e.AssessmentData).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssessedByNavigation).WithMany(p => p.PlantHealthAssessments)
                .HasForeignKey(d => d.AssessedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("PlantHealthAssessments_AssessedBy_fkey");

            entity.HasOne(d => d.Batch).WithMany(p => p.PlantHealthAssessments)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PlantHealthAssessments_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.PlantHealthAssessments)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("PlantHealthAssessments_ExperimentId_fkey");

            entity.HasOne(d => d.Image).WithMany(p => p.PlantHealthAssessments)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("PlantHealthAssessments_ImageId_fkey");
        });

        modelBuilder.Entity<PlantImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PlantImages_pkey");

            entity.HasIndex(e => e.BatchId, "IX_PlantImages_BatchId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CapturedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Batch).WithMany(p => p.PlantImages)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PlantImages_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.PlantImages)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("PlantImages_ExperimentId_fkey");

            entity.HasOne(d => d.TaskReport).WithMany(p => p.PlantImages)
                .HasForeignKey(d => d.TaskReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("PlantImages_TaskReportId_fkey");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.PlantImages)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("PlantImages_UploadedBy_fkey");
        });

        modelBuilder.Entity<ProcedureTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProcedureTemplates_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.TemplateName).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProcedureTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ProcedureTemplates_CreatedBy_fkey");

            entity.HasOne(d => d.CropVariety).WithMany(p => p.ProcedureTemplates)
                .HasForeignKey(d => d.CropVarietyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ProcedureTemplates_CropVarietyId_fkey");
        });

        modelBuilder.Entity<ProcedureTemplateStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProcedureTemplateSteps_pkey");

            entity.HasIndex(e => new { e.TemplateId, e.StepOrder }, "UQ_ProcedureTemplateSteps_Order").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Template).WithMany(p => p.ProcedureTemplateSteps)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("ProcedureTemplateSteps_TemplateId_fkey");
        });

        modelBuilder.Entity<RequestReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RequestReviews_pkey");

            entity.HasIndex(e => e.RequestId, "IX_RequestReviews_RequestId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReviewedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestReviews)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("RequestReviews_RequestId_fkey");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.RequestReviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("RequestReviews_ReviewerId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pkey");

            entity.HasIndex(e => e.RoleName, "Roles_RoleName_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Sensors_pkey");

            entity.HasIndex(e => e.SensorCode, "Sensors_SensorCode_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SensorCode).HasMaxLength(50);
        });

        modelBuilder.Entity<SensorDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SensorData_pkey");

            entity.HasIndex(e => new { e.ExperimentId, e.BatchId }, "IX_SensorData_Experiment_Batch");

            entity.HasIndex(e => new { e.SensorId, e.RecordedAt }, "IX_SensorData_Sensor_RecordedAt");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Unit).HasMaxLength(30);
            entity.Property(e => e.Value).HasPrecision(12, 4);

            entity.HasOne(d => d.Batch).WithMany(p => p.SensorData)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("SensorData_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.SensorData)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("SensorData_ExperimentId_fkey");

            entity.HasOne(d => d.Sensor).WithMany(p => p.SensorData)
                .HasForeignKey(d => d.SensorId)
                .HasConstraintName("SensorData_SensorId_fkey");
        });

        modelBuilder.Entity<SensorThresholdRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SensorThresholdRules_pkey");

            entity.HasIndex(e => e.ExperimentId, "IX_SensorThresholdRules_ExperimentId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxValue).HasPrecision(12, 4);
            entity.Property(e => e.MinValue).HasPrecision(12, 4);

            entity.HasOne(d => d.Batch).WithMany(p => p.SensorThresholdRules)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("SensorThresholdRules_BatchId_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.SensorThresholdRules)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("SensorThresholdRules_ExperimentId_fkey");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Skills_pkey");

            entity.HasIndex(e => e.SkillName, "Skills_SkillName_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SkillName).HasMaxLength(100);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Tasks_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DueDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TaskAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Tasks_AssignedTo_fkey");

            entity.HasOne(d => d.Batch).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Tasks_BatchId_fkey");

            entity.HasOne(d => d.CareSchedule).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.CareScheduleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Tasks_CareScheduleId_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TaskCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Tasks_CreatedBy_fkey");

            entity.HasOne(d => d.Experiment).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ExperimentId)
                .HasConstraintName("Tasks_ExperimentId_fkey");

            entity.HasOne(d => d.ExperimentStage).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ExperimentStageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Tasks_ExperimentStageId_fkey");
        });

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaskAssignments_pkey");

            entity.HasIndex(e => e.TaskId, "IX_TaskAssignments_TaskId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.EndedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TaskAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TaskAssignments_AssignedBy_fkey");

            entity.HasOne(d => d.Assignee).WithMany(p => p.TaskAssignmentAssignees)
                .HasForeignKey(d => d.AssigneeId)
                .HasConstraintName("TaskAssignments_AssigneeId_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskAssignments)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("TaskAssignments_TaskId_fkey");
        });

        modelBuilder.Entity<TaskReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaskReports_pkey");

            entity.HasIndex(e => e.TaskId, "IX_TaskReports_TaskId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReportedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ResultData).HasColumnType("jsonb");

            entity.HasOne(d => d.Reporter).WithMany(p => p.TaskReports)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("TaskReports_ReporterId_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskReports)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("TaskReports_TaskId_fkey");
        });

        modelBuilder.Entity<TaskSkillRequirement>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.SkillId }).HasName("TaskSkillRequirements_pkey");

            entity.Property(e => e.RequiredLevel).HasDefaultValue(1);

            entity.HasOne(d => d.Skill).WithMany(p => p.TaskSkillRequirements)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("TaskSkillRequirements_SkillId_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskSkillRequirements)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("TaskSkillRequirements_TaskId_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "IX_Users_Email_Active");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ResetCode).HasMaxLength(10);
            entity.Property(e => e.ResetCodeExpires).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Student'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("UserRoles_pkey");

            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("UserRoles_RoleId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserRoles_UserId_fkey");
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SkillId }).HasName("UserSkills_pkey");

            entity.HasIndex(e => e.SkillId, "IX_UserSkills_SkillId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ProficiencyLevel).HasDefaultValue(1);

            entity.HasOne(d => d.Skill).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("UserSkills_SkillId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserSkills_UserId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
