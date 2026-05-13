using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces;
using SmartFarmSEP490.Service.Interfaces;

namespace SmartFarmSEP490.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            if (user == null || user.PasswordHash != request.Password)
            {
                return null;
            }

            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null) return false;

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = request.Role ?? "Student"
            };

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
                        PasswordHash = Guid.NewGuid().ToString(),
                        Role = "Student"
                    };
                    await _userRepository.AddUserAsync(user);
                }

                var token = GenerateJwtToken(user);
                return new LoginResponse
                {
                    Token = token,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName
                };
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
            user.ResetCode = code;
            user.ResetCodeExpires = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateUserAsync(user);

            var subject = "Mã xác nhận khôi phục mật khẩu - Smart Farm";
            var body = $"<h3>Mã xác nhận của bạn là: <b style='color:red'>{code}</b></h3><p>Mã này sẽ hết hạn sau 10 phút.</p>";
            await _emailService.SendEmailAsync(email, subject, body);

            return true;
        }

        public async Task<bool> VerifyResetCodeAsync(string email, string code)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.ResetCode != code || user.ResetCodeExpires < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.ResetCode != code || user.ResetCodeExpires < DateTime.UtcNow)
                return false;

            user.PasswordHash = newPassword;
            user.ResetCode = null;
            user.ResetCodeExpires = null;
            
            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "YourSecretKeyForJWT_MustBeLongEnough"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
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
