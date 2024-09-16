using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BettingController : ControllerBase
{
    private readonly BettingService _bettingService;

    public BettingController(BettingService bettingService)
    {
        _bettingService = bettingService;
    }

    [HttpPost("bet")]
    public async Task<IActionResult> PlaceBet([FromBody] BetRequest request)
    {
        try
        {
            var newBalance = await _bettingService.PlaceBetAsync(request.BetAmount);
            return Ok(new { Balance = newBalance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
