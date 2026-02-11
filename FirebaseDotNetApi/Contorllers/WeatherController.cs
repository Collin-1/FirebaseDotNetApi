using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FirebaseDotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    // ✅ This endpoint requires a valid Firebase ID token
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        // These claims are extracted from the verified JWT
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var email = User.FindFirstValue("email") ?? "no-email-claim";

        // Return something simple to prove auth works
        return Ok(new
        {
            message = "You are authenticated by Firebase ✅",
            uid,
            email,
            serverTime = DateTime.UtcNow
        });
    }

    // ✅ Public endpoint (no auth)
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
    {
        return Ok(new { message = "Anyone can access this." });
    }
}
