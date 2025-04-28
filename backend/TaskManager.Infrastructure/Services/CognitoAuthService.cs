using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManager.Core.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace TaskManager.Infrastructure.Services
{
    public class CognitoAuthService : IAuthService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CognitoAuthService> _logger;
        private readonly string _clientId;
        private readonly string _userPoolId;

        public CognitoAuthService(
            IAmazonCognitoIdentityProvider cognitoClient,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<CognitoAuthService> logger)
        {
            _cognitoClient = cognitoClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            
            _clientId = _configuration["AWS:UserPoolClientId"];
            _userPoolId = _configuration["AWS:UserPoolId"];
            
            _logger.LogInformation("CognitoAuthService initialized with UserPoolId: {UserPoolId}, ClientId: {ClientId}", 
                _userPoolId, _clientId);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registering user with username: {Username}, email: {Email}", 
                    registerDto.Username, registerDto.Email);
                
                var signUpRequest = new SignUpRequest
                {
                    ClientId = _clientId,
                    Password = registerDto.Password,
                    Username = registerDto.Username,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = registerDto.Email },
                        new AttributeType { Name = "name", Value = registerDto.Username }
                    }
                };

                var response = await _cognitoClient.SignUpAsync(signUpRequest);
                
                _logger.LogInformation("User registered in Cognito with sub: {UserSub}", response.UserSub);
                
                // 注册成功后立即创建本地用户记录
                await SyncLocalUserAsync(
                    response.UserSub, 
                    registerDto.Email,
                    registerDto.Username
                );
                
                _logger.LogInformation("Local user record synchronized successfully");
                
                return new AuthResponse
                {
                    Successful = true,
                    UserId = response.UserSub,
                    Username = registerDto.Username,
                    Message = "Registration successful. Please check your email for verification code."
                };
            }
            catch (UsernameExistsException ex)
            {
                _logger.LogWarning("Username already exists: {Message}", ex.Message);
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Username already registered"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return new AuthResponse
                {
                    Successful = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Attempting login for email: {Email}", loginDto.Email);
                
                var authRequest = new AdminInitiateAuthRequest
                {
                    ClientId = _clientId,
                    UserPoolId = _userPoolId,
                    AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", loginDto.Email },
                        { "PASSWORD", loginDto.Password }
                    }
                };

                var authResponse = await _cognitoClient.AdminInitiateAuthAsync(authRequest);
                
                _logger.LogInformation("Login successful, retrieving user info");
                
                // 获取用户信息
                var userInfoRequest = new GetUserRequest
                {
                    AccessToken = authResponse.AuthenticationResult.AccessToken
                };
                
                var userInfo = await _cognitoClient.GetUserAsync(userInfoRequest);
                string userId = userInfo.UserAttributes.Find(a => a.Name == "sub")?.Value;
                string username = userInfo.UserAttributes.Find(a => a.Name == "name")?.Value ?? loginDto.Email;
                string email = userInfo.UserAttributes.Find(a => a.Name == "email")?.Value ?? loginDto.Email;

                _logger.LogInformation("User info retrieved - UserId: {UserId}, Username: {Username}, Email: {Email}", 
                    userId, username, email);
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unable to retrieve user ID from Cognito response");
                    return new AuthResponse
                    {
                        Successful = false,
                        Message = "Unable to retrieve user information"
                    };
                }

                // 登录成功后更新本地用户记录
                await SyncLocalUserAsync(userId, email, username);
                
                _logger.LogInformation("Local user record synchronized successfully");

                return new AuthResponse
                {
                    Successful = true,
                    Token = authResponse.AuthenticationResult.IdToken,
                    UserId = userId,
                    Username = username,
                    Message = "Login successful"
                };
            }
            catch (NotAuthorizedException ex)
            {
                _logger.LogWarning("Login failed - not authorized: {Message}", ex.Message);
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Invalid email or password"
                };
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("Login failed - user not found: {Message}", ex.Message);
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Invalid email or password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return new AuthResponse
                {
                    Successful = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<UserEntity> GetUserByIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting user by ID: {UserId}", userId);
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found in local database: {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("User found: {Username}", user.Username);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                throw; // 重新抛出异常以便调用者处理
            }
        }

        public async Task<bool> VerifyUserOwnershipAsync(string userId, int taskId)
        {
            try
            {
                _logger.LogInformation("Verifying ownership - UserId: {UserId}, TaskId: {TaskId}", userId, taskId);
                var task = await _context.Tasks.FindAsync(taskId);
                bool isOwner = task != null && task.UserId == userId;
                
                _logger.LogInformation("Ownership verification result: {IsOwner}", isOwner);
                return isOwner;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user ownership - UserId: {UserId}, TaskId: {TaskId}", userId, taskId);
                throw; // 重新抛出异常以便调用者处理
            }
        }
        
        // 添加同步本地用户方法的实现
        public async Task SyncLocalUserAsync(string cognitoUserId, string email, string username)
        {
            if (string.IsNullOrEmpty(cognitoUserId))
            {
                _logger.LogError("Cannot sync user with empty ID");
                throw new ArgumentException("Cognito user ID cannot be null or empty", nameof(cognitoUserId));
            }
            
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email is empty for user {UserId}", cognitoUserId);
                // 使用一个默认值或用户ID作为邮箱
                email = $"{cognitoUserId}@example.com";
            }
            
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Username is empty for user {UserId}", cognitoUserId);
                // 使用一个默认值或邮箱前缀作为用户名
                username = email.Split('@')[0];
            }
            
            try 
            {
                _logger.LogInformation("Syncing local user - UserId: {UserId}, Email: {Email}, Username: {Username}", 
                    cognitoUserId, email, username);
                
                // 尝试查找现有用户
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == cognitoUserId);
                
                if (existingUser == null)
                {
                    _logger.LogInformation("Creating new local user record for {UserId}", cognitoUserId);
                    
                    // 创建新用户
                    var newUser = new UserEntity
                    {
                        Id = cognitoUserId,
                        Username = username,
                        Email = email,
                        PasswordHash = "[MANAGED BY COGNITO]",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        Role = "User"
                    };
                    
                    _context.Users.Add(newUser);
                }
                else
                {
                    _logger.LogInformation("Updating existing local user record for {UserId}", cognitoUserId);
                    
                    // 更新现有用户
                    existingUser.Username = username;
                    existingUser.Email = email;
                    existingUser.EmailConfirmed = true;
                    
                    _context.Users.Update(existingUser);
                }
                
                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation("Database save completed with {SavedEntities} entities affected", saveResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing local user - UserId: {UserId}", cognitoUserId);
                throw; // 重新抛出异常以便调用者处理
            }
        }
    }
}