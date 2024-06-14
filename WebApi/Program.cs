using System.Text;
using System.Text.Json.Serialization;
using BusinessObjects;
using BusinessObjects.Utils;
using Dao;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApi;
using WebApi.Utils.CustomProblemDetails;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddDao();
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment() || builder.Environment.IsProduction();
    options.Map<DbCustomException>(e => new DbCustomProblemDetail()
    {
        Title = e.Title,
        Status = StatusCodes.Status500InternalServerError,
        Detail = e.Detail,
        Type = e.Type,
        Instance = e.Instance, AdditionalInfo = e.AdditionalInfo
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<GiveAwayDbContext>(optionsAction: optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultDB"));
});
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Give Away API", Version = "v1" });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        }
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "AllowAll",
        policy => { policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod(); }
    );
});

string? jwtIssuer = builder.Configuration[Services.Utils.JwtConstants.JwtIssuer];
string? jwtKey = builder.Configuration[Services.Utils.JwtConstants.JwtKey];
string? jwtAudience = builder.Configuration[Services.Utils.JwtConstants.JwtAudience];

builder
    .Services.AddAuthentication()
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration[Services.Utils.GoogleConstants.ClientId]!;
        options.ClientSecret = builder.Configuration[Services.Utils.GoogleConstants.ClientSecret]!;
        options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
        options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});
builder
    .Services.AddControllers()
    .AddJsonOptions(x => { x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseProblemDetails();
app.UseCors("AllowAll");
app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();