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

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            // Note: In production, use BCrypt or similar for password hashing
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
                PasswordHash = request.Password, // In prod: Hash this!
                Role = request.Role
            };

            await _userRepository.AddUserAsync(user);
            return true;
        }

        public async Task<LoginResponse?> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            try
            {
                // Gọi API của Google để lấy thông tin người dùng từ access_token
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
                    // Tự động tạo User mới nếu chưa có trong hệ thống
                    user = new User
                    {
                        FullName = googleUser.Name ?? "Google User",
                        Email = googleUser.Email,
                        PasswordHash = Guid.NewGuid().ToString(), // Password giả cho acc Google
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
