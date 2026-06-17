using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Auth;
using SmartFarmSEP490.Repository.Interfaces.SystemLogs;
using SmartFarmSEP490.Service.Interfaces.Auth;
using SmartFarmSEP490.Service.Interfaces.Helpers;
using BCrypt.Net;
using Task = System.Threading.Tasks.Task;
namespace SmartFarmSEP490.Service.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISystemLogRepository _systemLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService, ISystemLogRepository systemLogRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
            _systemLogRepository = systemLogRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            try
            {
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return null;
                }
            }
            catch (Exception)
            {
                // Trả về null nếu hash trong DB không đúng định dạng (ví dụ đang là plain text)
                return null;
            }

            var token = GenerateJwtToken(user);

            string roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "Student";

            var response = new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = roleName,
                FullName = user.FullName
            };

            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
            var metadataStr = System.Text.Json.JsonSerializer.Serialize(new { method = "local_auth", role = roleName });

            await _systemLogRepository.AddLogAsync(new SystemLog
            {
                UserId = user.Id,
                Action = "LOGIN",
                EntityName = "Users",
                EntityId = user.Id,
                Description = $"Người dùng {user.FullName} ({user.Email}) đăng nhập thành công.",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Metadata = System.Text.Json.JsonDocument.Parse(metadataStr)
            });

            return response;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null) return false;

            string requestedRole = request.Role ?? "Student";
            var role = await _userRepository.GetRoleByNameAsync(requestedRole);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            if (role != null)
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id, AssignedAt = DateTime.UtcNow });
            }

            await _userRepository.AddUserAsync(user);
            return true;
        }

        public async Task<LoginResponse?> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();
                var googleResponse = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={request.Token}");
                
                if (!googleResponse.IsSuccessStatusCode)
                    return null;

                var content = await googleResponse.Content.ReadAsStringAsync();
                var googleUser = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
                    return null;

                var user = await _userRepository.GetUserByEmailAsync(googleUser.Email);

                if (user == null)
                {
                    user = new User
                    {
                        FullName = googleUser.Name ?? "Google User",
                        Email = googleUser.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
                    };
                    var role = await _userRepository.GetRoleByNameAsync("Student");
                    if (role != null)
                    {
                        user.UserRoles.Add(new UserRole { RoleId = role.Id, AssignedAt = DateTime.UtcNow });
                    }
                    await _userRepository.AddUserAsync(user);
                }

                var token = GenerateJwtToken(user);
                string roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "Student";

                var response = new LoginResponse
                {
                    Token = token,
                    Email = user.Email,
                    Role = roleName,
                    FullName = user.FullName
                };

                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
                var metadataStr = System.Text.Json.JsonSerializer.Serialize(new { method = "google_auth", role = roleName });

                await _systemLogRepository.AddLogAsync(new SystemLog
                {
                    UserId = user.Id,
                    Action = "LOGIN",
                    EntityName = "Users",
                    EntityId = user.Id,
                    Description = $"Người dùng {user.FullName} ({user.Email}) đăng nhập qua Google thành công.",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Metadata = System.Text.Json.JsonDocument.Parse(metadataStr)
                });

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class GoogleUserInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email.Trim());
            if (user == null) return false;

            var code = new Random().Next(100000, 999999).ToString();
            // Database First: The DB doesn't have ResetCode/Expires anymore, so this logic is disabled.
            // user.ResetCode = code;
            // user.ResetCodeExpires = DateTime.Now.AddMinutes(10);
            return false;

            await _userRepository.UpdateUserAsync(user);

            var subject = "☘️ Smart Farm - Mã xác nhận khôi phục mật khẩu";
            var body = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; border-radius: 16px; overflow: hidden; background-color: #f7fee7; border: 1px solid #d9f99d;'>
                    <div style='background: linear-gradient(135deg, #059669 0%, #166534 100%); padding: 30px; text-align: center;'>
                        <div style='font-size: 40px; margin-bottom: 10px;'>🌿</div>
                        <h1 style='color: white; margin: 0; font-size: 28px; letter-spacing: 1px; font-weight: 800;'>SMART FARM</h1>
                        <p style='color: #d1fae5; margin: 5px 0 0 0; font-size: 14px;'>Nông nghiệp thông minh - Tương lai bền vững</p>
                    </div>
                    
                    <div style='padding: 40px 30px; background-color: #ffffff; text-align: center; position: relative;'>
                        <div style='position: absolute; top: 10px; right: 10px; font-size: 40px; opacity: 0.1;'>🍃</div>
                        <div style='position: absolute; bottom: 10px; left: 10px; font-size: 40px; opacity: 0.1;'>🌱</div>
                        
                        <h2 style='color: #166534; margin-bottom: 20px;'>Xác thực tài khoản</h2>
                        <p style='color: #4b5563; font-size: 16px; line-height: 1.6;'>Chào bạn, một mầm non mới đang chờ được chăm sóc! <br/> Vui lòng nhập mã dưới đây để tiếp tục hành trình của bạn.</p>
                        
                        <div style='margin: 35px 0; padding: 25px 40px; background-color: #f0fdf4; border: 2px solid #bbf7d0; border-radius: 50px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);'>
                            <span style='font-size: 42px; font-weight: 900; color: #059669; letter-spacing: 8px;'>{code}</span>
                        </div>
                        
                        <p style='color: #dc2626; font-size: 14px; margin-top: 20px;'>
                            <strong>Cảnh báo:</strong> Mã này sẽ héo úa sau 10 phút.
                        </p>
                    </div>
                    
                    <div style='background-color: #ecfccb; padding: 20px; text-align: center; border-top: 1px solid #d9f99d;'>
                        <p style='color: #3f6212; font-size: 13px; margin: 0;'>Cảm ơn bạn đã đồng hành cùng cộng đồng nông nghiệp xanh!</p>
                        <div style='margin-top: 10px; color: #65a30d; font-size: 11px;'>
                            © 2026 Smart Farm Project • Vì một hành tinh xanh hơn
                        </div>
                    </div>
                </div>";
            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR]: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }

            return true;
        }

        public async Task<bool> VerifyResetCodeAsync(string email, string code)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;
            
            // Not supported in DB First schema
            return false;

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            // Database First: Missing ResetCode support
            // user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // user.ResetCode = null;
            // user.ResetCodeExpires = null;
            
            return false;
            
            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "YourSecretKeyForJWT_MustBeLongEnough"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            string roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "Student";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("FullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
