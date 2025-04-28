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

//builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        // Log the actual configuration values being used
//        Console.WriteLine($"JWT Secret: {builder.Configuration["JWT:Secret"]}");
//    
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"])),
//            ValidateIssuer = false,
//            ValidateAudience = false,
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.Zero
//        };
//    
//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                return Task.CompletedTask;
//            },
//            OnMessageReceived = context =>
//            {
//                return Task.CompletedTask;
//            },
//            OnTokenValidated = context =>
//            {
//                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
//                return Task.CompletedTask;
//            }
//        };
//    });

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
        ValidateLifetime = true
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

builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// set Initialize database
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();