using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Infrastructure;
using Ci2dt2.Infrastructure.Persistence;
using Ci2dt2.Infrastructure.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((contexto, servicios, log) => log
    .ReadFrom.Configuration(contexto.Configuration)
    .ReadFrom.Services(servicios)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextoPeticion, ContextoPeticion>();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("baseDatos");

// La IP real del cliente llega en X-Forwarded-For (proxy de Next.js en Vercel y el hosting).
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 2;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

// CORS restringido a los dominios del frontend.
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((o, jwt) =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Value.Emisor,
            ValidAudience = jwt.Value.Audiencia,
            IssuerSigningKey = jwt.Value.ObtenerLlave(),
            NameClaimType = ClaimsCi2dt2.Id,
            RoleClaimType = ClaimsCi2dt2.Rol,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// Matriz de permisos (Anexo A) como políticas. Todo endpoint exige autenticación salvo que se indique lo contrario.
builder.Services.AddAuthorization(o =>
{
    foreach (var (politica, roles) in Politicas.Matriz)
        o.AddPolicy(politica, p => p.RequireRole(roles));
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddOpenApi(o => o.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "CI²DT² API";
    documento.Components ??= new OpenApiComponents();
    documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    };
    documento.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", documento)] = [] }];
    return Task.CompletedTask;
}));

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "CI²DT² API v1"));
}

app.UseCors();
// Las respuestas de la API no se guardan en caché (el botón "atrás" no debe mostrar datos protegidos).
app.Use((http, next) =>
{
    http.Response.Headers.CacheControl = "no-store";
    return next(http);
});
app.UseAuthentication();
app.UseMiddleware<AuditoriaMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapModulos();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<InicializadorBaseDatos>().InicializarAsync();

app.Run();

public partial class Program;
