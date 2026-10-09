using Encryption.DTO;
using Encryption.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Encryption.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinksController : ControllerBase
{
    private readonly ILinkService _linkService;
    private readonly ILogger<LinksController> _logger;

    public LinksController(ILinkService linkService, ILogger<LinksController> logger)
    {
        _linkService = linkService;
        _logger = logger;
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateLinkResponseDto>> CreateLink([FromBody] CreateLinkRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Pokus o vytvorenie linku bez autentifikácie");
                return Unauthorized(new { message = "Musíš byť prihlásený" });
            }

            var response = await _linkService.CreateEncryptedLinkAsync(dto, userId);

            _logger.LogInformation("Vytvorený nový šifrovaný odkaz {ShortCode} užívateľom {UserId}",
                response.ShortCode, userId);

            return CreatedAtAction(nameof(DecryptLink), new { shortCode = response.ShortCode }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba pri vytváraní odkazu");
            return BadRequest(new { message = "Chyba pri vytváraní odkazu", error = ex.Message });
        }
    }

    [HttpPost("{shortCode}/decrypt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecryptLinkResponseDto>> DecryptLink(
        [FromRoute] string shortCode,
        [FromBody] DecryptLinkRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                return BadRequest(new { message = "Short kód je povinný" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string userAgent = Request.Headers.UserAgent.ToString() ?? "Unknown";

            var response = await _linkService.DecryptLinkAsync(shortCode, dto, clientIp, userAgent);

            _logger.LogInformation("Úspešne dešifrovaný odkaz {ShortCode}", shortCode);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Odkaz nenájdený: {ShortCode}", shortCode);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operácia pri dešifrovaní {ShortCode}: {Message}", shortCode, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba pri dešifrovaní odkazu {ShortCode}", shortCode);
            return BadRequest(new { message = "Chyba pri dešifrovaní", error = ex.Message });
        }
    }

    [HttpGet("my-links")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CreateLinkResponseDto>>> GetMyLinks()
    {
        try
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Pokus o získanie linkov bez autentifikácie");
                return Unauthorized(new { message = "Musíš byť prihlásený" });
            }

            var links = await _linkService.GetUserLinksAsync(userId);

            return Ok(links);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba pri získavaní odkazov");
            return BadRequest(new { message = "Chyba pri získavaní odkazov", error = ex.Message });
        }
    }
}