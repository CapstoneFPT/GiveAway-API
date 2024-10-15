using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Quartz;
using Serilog;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Features.Auth.Login;
using WebApi2._0.Infrastructure.ExternalServices.VNPay;
using WebApi2._0.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
IronPdf.License.LicenseKey = builder.Configuration["IronPDF"];
IronPdf.Installation.ChromeGpuMode=IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
IronPdf.Installation.LinuxAndDockerDependenciesAutoConfig = false;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
 
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<GiveAwayDbContext>(option => option.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultDB")
    ));
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddEndpointsApiExplorer();


string? jwtKey = builder.Configuration[JwtConstants.JwtKey];

builder
    .Services.AddAuthenticationJwtBearer(
        options => options.SigningKey = jwtKey)
    .AddAuthorization()
    .AddFastEndpoints()
    .SwaggerDocument();
builder.Services.Configure<VnPaySettings>(builder.Configuration.GetSection("VNPay"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(option =>
    {
        option.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
        option.Endpoints.RoutePrefix = "api";
    })
    .UseSwaggerGen();


app.UseHttpsRedirection();


await app.RunAsync();

