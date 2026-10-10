using System.Security.Claims;
using Encryption.DTO;
using Encryption.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [Authorize]
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
                _logger.LogWarning("Attempt to create link without authentication");
                return Unauthorized(new { message = "User must be authenticated." });
            }

            var response = await _linkService.CreateEncryptedLinkAsync(dto, userId);

            _logger.LogInformation("Created new encrypted link {ShortCode} by user {UserId}",
                response.ShortCode, userId);

            return CreatedAtAction(nameof(DecryptLink), new { shortCode = response.ShortCode }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating encrypted link");
            return BadRequest(new { message = "Error creating encrypted link", error = ex.Message });
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
                return BadRequest(new { message = "Short code is required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string userAgent = Request.Headers.UserAgent.ToString() ?? "Unknown";

            var response = await _linkService.DecryptLinkAsync(shortCode, dto, clientIp, userAgent);

            _logger.LogInformation("Successfully decrypted link {ShortCode}", shortCode);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Link not found: {ShortCode}", shortCode);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation when decrypting {ShortCode}: {Message}", shortCode, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting link {ShortCode}", shortCode);
            return BadRequest(new { message = "Error decrypting link", error = ex.Message });
        }
    }

    [Authorize]
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
                _logger.LogWarning("Attempt to retrieve links without authentication");
                return Unauthorized(new { message = "User must be authenticated." });
            }

            var links = await _linkService.GetUserLinksAsync(userId);

            return Ok(links);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving links");
            return BadRequest(new { message = "Error retrieving links", error = ex.Message });
        }
    }
}