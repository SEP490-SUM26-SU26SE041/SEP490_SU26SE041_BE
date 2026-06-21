using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Npgsql.NameTranslation;
using SmartFarmSEP490.Model.Enums;
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
using SmartFarmSEP490.Service.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Allow Npgsql to map .NET DateTime (Kind=Unspecified) to Postgres "timestamp without time zone"
// (DbContext columns for CreatedAt/UpdatedAt are mapped as `timestamp without time zone`).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Prevent JWT handler from auto-mapping short claim names to long URIs
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<ExperimentStageType>("ExperimentStageType", new NpgsqlNullNameTranslator());
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

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
builder.Services.AddScoped<ITaskSkillRequirementRepository, TaskSkillRequirementRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

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
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
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

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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



    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/notificationHub");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL ERROR] Application failed to start: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
    throw;
}
