// Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;
using System;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace TaskManager.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            // 打印关键配置值
            Console.WriteLine($"AWS:Region = {Configuration["AWS:Region"]}");
            Console.WriteLine($"AWS:UserPoolId = {Configuration["AWS:UserPoolId"]}");
            Console.WriteLine($"AWS:UserPoolClientId = {Configuration["AWS:UserPoolClientId"]}");
        }

        // 注册服务
        public void ConfigureServices(IServiceCollection services)
        {
            // 启用详细的JWT错误日志
            IdentityModelEventSource.ShowPII = true;
            
            // 添加控制器
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // 添加数据库上下文
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );

            // 添加CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            // 硬编码 Cognito JWKS
            var jwksJson = @"{""keys"":[{""alg"":""RS256"",""e"":""AQAB"",""kid"":""yZC12VXvA2UZoNrW0nJTvulEosOJ4m9huGB4zwGQPB4="",""kty"":""RSA"",""n"":""vPhmyN4c4HUfWzqpOOuh3e1nMA2JnbSRZoNvutUWw47eeEyD_oxWQGEAVbJlbE5NgZSXyZaiZFynkAFIgETVhYyzb-Pn-KxznL_lS3ckZeAcITORN0B5BnQv-crigWnP8hA4Ff_SuQrTJR9HuHP4VdYR_pbG8Z_iaAk2zCbzC4TjIyWLfRXwYQCbMs2DhoYg9APSSohJUJfl_epsw_a8keIaCkbpnBKZQXY0Oq_PBWefyHnBe2Z19nsTixV7h2W4mOHT9nKWrbnDkhF-EOvNxmP7WXwEWKF4IEMwvdJ93ImgVBrk-7GkP_6ax8khGGrQ1ZdKKA124sX17Ia6Z7LptQ"",""use"":""sig""},{""alg"":""RS256"",""e"":""AQAB"",""kid"":""JRrn72KZQ9SjJfDVbyzkXdhmZzpJ32ou84hyOsLN7RU="",""kty"":""RSA"",""n"":""2OA2AGSyF0ccNKvp7x4TRxIAQKAa9uG2FS1c9HvVOJYUTk4gTfCHnNVllEqYF7Bgg9Gt_msDg_6ZYyTXX2gPOPtHMnXvLQo1g7tzVQ9LzEAInbdTwuNKkda8NqLAmsLPjO0PQZnpTJRaO6006BkgAS618j8fFhD5CXW7BlSyPzUKoA39TBiZfUJ7iBmWkBsQmR5sJYuPGPX747EiuobDVfTHoBP2ZA8PSHaC-wfXIATStXoYmwQZDTppgbQVUtVo2VDUUXMfgL0U47BECmPQ57YSvINrxgG9aRGtNvwTuf-11taRWTHYm9LReypSP-O_I_4rFhzQYkTyB4yFj1O_Lw"",""use"":""sig""}]}";
            
            Console.WriteLine("使用硬编码的 JWKS 数据");
            
            JsonWebKeySet jwks = null;
            try
            {
                jwks = new JsonWebKeySet(jwksJson);
                Console.WriteLine($"JWKS 密钥数量: {jwks.Keys.Count}");
                
                foreach (var key in jwks.Keys)
                {
                    Console.WriteLine($"加载的密钥ID: {key.Kid}, 类型: {key.Kty}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析硬编码 JWKS 失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
            }

            // 添加认证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var region = Configuration["AWS:Region"];
                var userPoolId = Configuration["AWS:UserPoolId"];
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = jwks?.Keys,  // 使用硬编码的签名密钥
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Startup>>();
                        var auth = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (auth != null)
                        {
                            logger.LogInformation(">>> OnMessageReceived: Authorization = {Auth}", 
                                auth.Substring(0, Math.Min(50, auth.Length)) + "...");
                        }
                        else
                        {
                            logger.LogInformation(">>> OnMessageReceived: Authorization 头为空");
                        }
                        
                        if (jwks == null)
                        {
                            logger.LogWarning("JWKS 为空，JWT验证可能会失败");
                        }
                        else
                        {
                            logger.LogInformation("JWKS 已加载，密钥数量: {Count}", jwks.Keys.Count);
                            foreach (var key in jwks.Keys)
                            {
                                logger.LogInformation("使用的密钥ID: {Kid}", key.Kid);
                            }
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Startup>>();
                        logger.LogError(ctx.Exception, "JWT验证失败: {Message}", ctx.Exception.Message);
                        
                        if (ctx.Exception.InnerException != null)
                        {
                            logger.LogError("内部异常: {Message}", ctx.Exception.InnerException.Message);
                        }
                        
                        // 尝试检查令牌头部
                        try
                        {
                            var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                            if (authHeader != null && authHeader.StartsWith("Bearer "))
                            {
                                var token = authHeader.Substring("Bearer ".Length);
                                var handler = new JwtSecurityTokenHandler();
                                
                                if (handler.CanReadToken(token))
                                {
                                    var jwtToken = handler.ReadJwtToken(token);
                                    var kid = jwtToken.Header.Kid;
                                    logger.LogError("令牌请求的 Kid: {Kid}", kid);
                                    
                                    if (jwks != null)
                                    {
                                        var foundKey = false;
                                        foreach (var key in jwks.Keys)
                                        {
                                            logger.LogInformation("JWKS 密钥 ID: {Kid}", key.Kid);
                                            if (key.Kid == kid)
                                            {
                                                foundKey = true;
                                                logger.LogInformation("找到匹配的密钥ID!");
                                            }
                                        }
                                        
                                        if (!foundKey)
                                        {
                                            logger.LogError("JWKS 中未找到匹配的密钥ID");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "解析令牌头部失败");
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnChallenge = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Startup>>();
                        logger.LogError("❌ JWT Challenge: Error={Error}, Description={Desc}",
                            ctx.Error, ctx.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Startup>>();
                        logger.LogInformation("✔ OnTokenValidated, claims count: {Count}",
                            ctx.Principal.Claims.Count());
                        // 将 sub Claim 加为 NameIdentifier
                        var sub = ctx.Principal.FindFirst("sub")?.Value;
                        if (sub != null)
                        {
                            ((ClaimsIdentity)ctx.Principal.Identity!)
                                .AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // 注册仓储
            services.AddScoped<ITaskRepository, TaskRepository>();

            // 添加Cognito服务
            services.AddAWSService<Amazon.CognitoIdentityProvider.IAmazonCognitoIdentityProvider>();
            services.AddScoped<IAuthService, CognitoAuthService>();
        }

        // 配置HTTP管道
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (ctx, next) =>
            {
                var logger = ctx.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation("┌─── Incoming Request ───");
                logger.LogInformation("{Method} {Path}", ctx.Request.Method, ctx.Request.Path);
                foreach (var h in ctx.Request.Headers)
                {
                    logger.LogInformation("Header: {Key} = {Value}", h.Key, h.Value.ToString());
                }
                logger.LogInformation("└─────────────────────────");
                await next();
            });
            
            if (env.IsDevelopment())
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

            app.UseRouting(); // 添加这行

            app.UseAuthentication();
            app.UseAuthorization();

            // 修改这部分代码
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            TestDbConnection(app.ApplicationServices);
        }
        
        private void TestDbConnection(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("🟡 Testing DB connection...");

            try
            {
                db.Database.OpenConnection();  // 注意，这里用的是同步打开
                Console.WriteLine("✅ DB connection successful!");
                db.Database.CloseConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed to connect to DB:");
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}