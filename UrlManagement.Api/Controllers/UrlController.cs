using Microsoft.AspNetCore.Mvc;
using UrlManagement.Api.Utils;

namespace UrlManagement.Api.Controllers;

[ApiController]
[Route("api/url")]
public class UrlController : ControllerBase
{
    [HttpPost("shorten")]
    public IActionResult ShortenUrl([FromBody] long id)
    {
        
        var code = ShortCodeGenerator.Encode(id);
        return Ok(new { ShortCode = code });
    }
}