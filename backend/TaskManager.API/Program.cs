using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using TaskManager.Infrastructure.Services;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AWS Lambda
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000") // frontend URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

Console.WriteLine("开始测试 JWKS 获取...");
var region = builder.Configuration["AWS:Region"];
var userPoolId = builder.Configuration["AWS:UserPoolId"];
var jwksUri = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json";

Console.WriteLine($"JWKS URI: {jwksUri}");

try 
{
    using var httpClient = new HttpClient();
    httpClient.Timeout = TimeSpan.FromSeconds(10);
    var jwksResponse = httpClient.GetStringAsync(jwksUri).Result;
    Console.WriteLine($"成功获取 JWKS: {jwksResponse}");
    
    // 也可以解析并打印各个组件
    var jwks = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksResponse);
    foreach (var key in jwks.Keys)
    {
        Console.WriteLine($"密钥ID (kid): {key.Kid}");
        Console.WriteLine($"算法 (alg): {key.Alg}");
        Console.WriteLine($"密钥类型 (kty): {key.Kty}");
        Console.WriteLine($"指数 (e): {key.E}");
        Console.WriteLine($"模数 (n): {key.N}");
        Console.WriteLine($"用途 (use): {key.Use}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"获取 JWKS 失败: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部异常: {ex.InnerException.Message}");
    }
}

// Add Cognito configration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var region = builder.Configuration["AWS:Region"];
    var userPoolId = builder.Configuration["AWS:UserPoolId"];
    
    options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}"
    };
	
	options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // 确保 name 声明映射到 NameIdentifier
            var nameClaim = context.Principal.FindFirst("sub");
            if (nameClaim != null)
            {
                var identity = context.Principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nameClaim.Value));
                }
            }
            
            return Task.CompletedTask;
        }
    };

});

// add repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Register AuthService
// builder.Services.AddScoped<IAuthService, AuthService>();

// Add Cognito Service
builder.Services.AddAWSService<Amazon.CognitoIdentityProvider.IAmazonCognitoIdentityProvider>();
builder.Services.AddScoped<IAuthService, CognitoAuthService>();

builder.Logging.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

var app = builder.Build();

// set Initialize database
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(options =>
{
    options.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();