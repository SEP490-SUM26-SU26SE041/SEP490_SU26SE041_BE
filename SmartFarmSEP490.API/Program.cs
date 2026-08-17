using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Npgsql.NameTranslation;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Model.Validators;
using SmartFarmSEP490.API.Middleware;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Areas;
using SmartFarmSEP490.Repository.Interfaces.Auth;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using SmartFarmSEP490.Repository.Interfaces.CropVarieties;
using SmartFarmSEP490.Repository.Interfaces.Crops;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.ExperimentDesigns;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using SmartFarmSEP490.Repository.Interfaces.ProcedureTemplates;
using SmartFarmSEP490.Repository.Implementations.Areas;
using SmartFarmSEP490.Repository.Implementations.Auth;
using SmartFarmSEP490.Repository.Implementations.Batches;
using SmartFarmSEP490.Repository.Implementations.Beds;
using SmartFarmSEP490.Repository.Implementations.CareSchedules;
using SmartFarmSEP490.Repository.Implementations.CropVarieties;
using SmartFarmSEP490.Repository.Implementations.Crops;
using SmartFarmSEP490.Repository.Implementations.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Implementations.ExperimentDesigns;
using SmartFarmSEP490.Repository.Implementations.ExperimentGroups;
using SmartFarmSEP490.Repository.Implementations.ExperimentRequests;
using SmartFarmSEP490.Repository.Implementations.ExperimentStages;
using SmartFarmSEP490.Repository.Implementations.Experiments;
using SmartFarmSEP490.Repository.Implementations.Farms;
using SmartFarmSEP490.Repository.Implementations.MeasurementDefinitions;
using SmartFarmSEP490.Repository.Implementations.ProcedureTemplates;
using SmartFarmSEP490.Service.Interfaces.Auth;
using SmartFarmSEP490.Service.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Service.Interfaces.Helpers;
using SmartFarmSEP490.Service.Interfaces.Resources;
using SmartFarmSEP490.Service.Services.Auth;
using SmartFarmSEP490.Service.Services.Experiments;
using SmartFarmSEP490.Service.Services.ExperimentRequests;
using SmartFarmSEP490.Service.Services.Helpers;
using SmartFarmSEP490.Service.Services.Resources;
using SmartFarmSEP490.Repository.Interfaces.SystemLogs;
using SmartFarmSEP490.Repository.Implementations.SystemLogs;
using SmartFarmSEP490.Service.Interfaces.SystemLogs;
using SmartFarmSEP490.Service.Services.SystemLogs;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Repository.Implementations.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;
using SmartFarmSEP490.Service.Services.Tasks;
using SmartFarmSEP490.Repository.Interfaces.Notifications;
using SmartFarmSEP490.Repository.Implementations.Notifications;
using SmartFarmSEP490.Service.Interfaces.Notifications;
using SmartFarmSEP490.Service.Services.Notifications;
using SmartFarmSEP490.Service.WebSockets;
using SmartFarmSEP490.Repository.Interfaces.Sensors;
using SmartFarmSEP490.Repository.Implementations.Sensors;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using SmartFarmSEP490.Repository.Implementations.Alerts;
using SmartFarmSEP490.Service.Interfaces.Dashboard;
using SmartFarmSEP490.Service.Services.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Allow Npgsql to map .NET DateTime (Kind=Unspecified) to Postgres "timestamp without time zone"
// (DbContext columns for CreatedAt/UpdatedAt are mapped as `timestamp without time zone`).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Prevent JWT handler from auto-mapping short claim names to long URIs
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var enumNameTranslator = new NpgsqlNullNameTranslator();
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.AIReviewStatus>("AIReviewStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.AllocationStatus>("AllocationStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.AlertSeverity>("AlertSeverity", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.BatchStatus>("BatchStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.DesignType>("DesignType", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.DocumentStatus>("DocumentStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.ExperimentStageType>("ExperimentStageType", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.ExperimentStatus>("ExperimentStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.GroupType>("GroupType", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.LocationStatus>("LocationStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.RequestStatus>("RequestStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.ReviewResult>("ReviewResult", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.SensorType>("SensorType", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.TaskAssignmentStatus>("TaskAssignmentStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.TaskStatus>("TaskStatus", enumNameTranslator);
dataSourceBuilder.MapEnum<SmartFarmSEP490.Model.Enums.TaskType>("TaskType", enumNameTranslator);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<SmartFarmDbContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IExperimentRequestRepository, ExperimentRequestRepository>();
builder.Services.AddScoped<IRequestReviewRepository, RequestReviewRepository>();
builder.Services.AddScoped<IExperimentRequestService, ExperimentRequestService>();

builder.Services.AddScoped<IExperimentRepository, ExperimentRepository>();
builder.Services.AddScoped<IExperimentStageRepository, ExperimentStageRepository>();
builder.Services.AddScoped<IExperimentGroupRepository, ExperimentGroupRepository>();
builder.Services.AddScoped<IExperimentDesignRepository, ExperimentDesignRepository>();
builder.Services.AddScoped<IMeasurementDefinitionRepository, MeasurementDefinitionRepository>();
builder.Services.AddScoped<IProcedureTemplateRepository, ProcedureTemplateRepository>();
builder.Services.AddScoped<ICareScheduleRepository, CareScheduleRepository>();
builder.Services.AddScoped<IExperimentService, ExperimentService>();
builder.Services.AddScoped<IExperimentStageService, ExperimentStageService>();
builder.Services.AddScoped<IExperimentGroupService, ExperimentGroupService>();
builder.Services.AddScoped<IExperimentDesignService, ExperimentDesignService>();
builder.Services.AddScoped<IMeasurementDefinitionService, MeasurementDefinitionService>();
builder.Services.AddScoped<IProcedureTemplateService, ProcedureTemplateService>();
builder.Services.AddScoped<ICareScheduleService, CareScheduleService>();

builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IBedRepository, BedRepository>();
builder.Services.AddScoped<IExperimentBedAssignmentRepository, ExperimentBedAssignmentRepository>();
builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<ICropVarietyRepository, CropVarietyRepository>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IBedService, BedService>();
builder.Services.AddScoped<IExperimentBedAssignmentService, ExperimentBedAssignmentService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<ICropVarietyService, CropVarietyService>();
builder.Services.AddScoped<ISystemLogRepository, SystemLogRepository>();
builder.Services.AddScoped<ISystemLogService, SystemLogService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Cloudinary - file upload service
builder.Services.Configure<SmartFarmSEP490.Model.DTOs.CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<SmartFarmSEP490.Service.Interfaces.Commons.ICloudinaryService,
                          SmartFarmSEP490.Service.Services.Commons.CloudinaryService>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
builder.Services.AddScoped<ITaskSkillRequirementRepository, TaskSkillRequirementRepository>();
builder.Services.AddScoped<ITaskReportRepository, TaskReportRepository>();
builder.Services.AddScoped<IMeasurementRecordRepository, MeasurementRecordRepository>();
builder.Services.AddScoped<IPlantImageRepository, PlantImageRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskReportService, TaskReportService>();
builder.Services.AddScoped<IMeasurementRecordService, MeasurementRecordService>();
builder.Services.AddScoped<IMeasurementStatisticsService, MeasurementStatisticsService>();
builder.Services.AddScoped<ITaskImageService, TaskImageService>();

// Dashboard Services
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IExperimentReportRepository, ExperimentReportRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IComparisonService, ComparisonService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();

// Overdue sweep (idempotent) — gọi 2 nơi:
//   1. Lazy ở đầu các TaskService.Get*() để user thấy status = Overdue ngay khi GET
//   2. Background service chạy đúng 00:00 ICT mỗi ngày (cron tự động)
builder.Services.AddScoped<IOverdueTaskService, OverdueTaskService>();
builder.Services.AddHostedService<OverdueTaskSweepBackgroundService>();

// Reminder: nhắc nhở hằng ngày task chưa hoàn thành trong ngày
builder.Services.AddScoped<IReminderTaskService, ReminderTaskService>();
builder.Services.AddHostedService<ReminderSweepBackgroundService>();

// Skills / UserSkills / Task Count
builder.Services.AddScoped<SmartFarmSEP490.Repository.Interfaces.Skills.ISkillRepository,
                          SmartFarmSEP490.Repository.Implementations.Skills.SkillRepository>();
builder.Services.AddScoped<SmartFarmSEP490.Repository.Interfaces.Skills.IUserSkillRepository,
                          SmartFarmSEP490.Repository.Implementations.Skills.UserSkillRepository>();
builder.Services.AddScoped<SmartFarmSEP490.Service.Interfaces.Skills.ISkillService,
                          SmartFarmSEP490.Service.Services.Skills.SkillService>();
builder.Services.AddScoped<SmartFarmSEP490.Service.Interfaces.Skills.IUserSkillService,
                          SmartFarmSEP490.Service.Services.Skills.UserSkillService>();
builder.Services.AddScoped<SmartFarmSEP490.Service.Interfaces.Skills.ITaskCountService,
                          SmartFarmSEP490.Service.Services.Skills.TaskCountService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken)
                && (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/ws")))
            {
                context.Token = accessToken;
            }

            // Hỗ trợ query "?token=" cho WebSocket endpoint
            var wsToken = context.Request.Query["token"];
            if (!string.IsNullOrEmpty(wsToken) && path.StartsWithSegments("/ws"))
            {
                context.Token = wsToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine($"[JWT FAIL] {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            var roles = ctx.Principal?.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value).ToList() ?? new();
            var sub = ctx.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                   ?? ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[JWT OK] sub={sub} roles=[{string.Join(',', roles)}]");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddMemoryCache();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SmartFarmSEP490.Service.WebSockets.IWebSocketConnectionManager, SmartFarmSEP490.Service.WebSockets.WebSocketConnectionManager>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.UseInlineDefinitionsForEnums();
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

try
{
    var app = builder.Build();



    // if (app.Environment.IsDevelopment())
    // {
    //if (app.Environment.IsDevelopment())
    //{
        app.UseSwagger();
        app.UseSwaggerUI();
    // }
    //}

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();

    app.Use(async (context, next) =>
    {
        try { await next(); }
        catch (Exception ex)
        {
            Console.WriteLine($"[REQUEST ERROR] {context.Request.Method} {context.Request.Path}");
            Console.WriteLine($"[REQUEST ERROR] {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    });

    app.UseExceptionHandler();
    app.UseRateLimiting();
    app.UseIdempotency();

    // WebSocket raw: bật UseWebSockets + custom middleware (auth JWT qua query ?token=)
    app.UseWebSockets(new Microsoft.AspNetCore.Builder.WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(30)
    });
    app.UseMiddleware<SmartFarmSEP490.Service.WebSockets.WebSocketMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
 //   app.MapHub<NotificationHub>("/notificationHub");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL ERROR] Application failed to start: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
    throw;
}
