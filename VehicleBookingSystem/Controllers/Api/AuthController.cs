using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VehicleBookingSystem.Contracts.Auth;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Options;

namespace VehicleBookingSystem.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtOptions _jwtOptions;
    private readonly IHostEnvironment _environment;
    private readonly ApiProblemDetailsFactory _problemDetailsFactory;

    public AuthController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IOptions<JwtOptions> jwtOptions,
        IHostEnvironment environment,
        ApiProblemDetailsFactory problemDetailsFactory)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
        _environment = environment;
        _problemDetailsFactory = problemDetailsFactory;
    }

    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> IssueToken([FromBody] TokenRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(_problemDetailsFactory.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "Thông tin đăng nhập không hợp lệ."));
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return Unauthorized(_problemDetailsFactory.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "Thông tin đăng nhập không hợp lệ."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes);
<<<<<<< HEAD
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.GetSigningKey(_environment)));
=======
        // Jwt options are validated during startup in Program.ValidateJwtOptions.
        var key = _jwtOptions.Key!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
>>>>>>> origin/dev
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return Ok(ApiResponse<TokenResponse>.Ok(new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expires
        }));
    }
}
