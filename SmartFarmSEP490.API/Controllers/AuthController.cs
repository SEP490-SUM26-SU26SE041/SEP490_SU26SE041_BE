using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces;

namespace SmartFarmSEP490.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RegisterAsync(request);

            if (!success)
                return BadRequest(new { message = "Email already exists" });

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginWithGoogleAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid Google token" });

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result) return BadRequest(new { message = "Email không tồn tại" });
            return Ok(new { message = "Mã xác nhận đã được gửi về Email" });
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            var result = await _authService.VerifyResetCodeAsync(request.Email, request.Code);
            if (!result) return BadRequest(new { message = "Mã xác nhận không đúng hoặc đã hết hạn" });
            return Ok(new { message = "Mã xác nhận hợp lệ" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
            if (!result) return BadRequest(new { message = "Khôi phục mật khẩu thất bại" });
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công" });
        }
    }
}
