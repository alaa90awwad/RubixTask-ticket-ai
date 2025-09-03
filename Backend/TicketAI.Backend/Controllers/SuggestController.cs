using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketAI.Backend.Models;
using TicketAI.Backend.Services;

namespace TicketAI.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestController : ControllerBase
    {
        private readonly ICategorySuggester _suggester;
        private readonly ILogger<SuggestController> _logger;

        public SuggestController(ICategorySuggester suggester, ILogger<SuggestController> logger)
        {
            _suggester = suggester;
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult<SuggestResponse>> Post([FromBody] SuggestRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { error = "Title and Description are required." });

            try
            {
                var result = await _suggester.SuggestAsync(request, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling OpenAI");
                return StatusCode(502, new { error = "Upstream OpenAI error", detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
