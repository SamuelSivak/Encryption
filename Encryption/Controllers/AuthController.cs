using Encryption.DTO;
using Encryption.Models.SQL;
using Encryption.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Encryption.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return CreatedAtAction(nameof(Register), new { message = "Register successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when registering");
            return BadRequest(new { message = "Error in registering", error = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new { message = "Bad email or password" });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(4),
                Email = user.Email ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when logging in");
            return BadRequest(new { message = "Error when logging in" });
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }
}