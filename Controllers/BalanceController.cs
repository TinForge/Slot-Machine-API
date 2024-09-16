using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly BalanceService _balanceService;

    public BalanceController(BalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckBalance()
    {
        try
        {
            var balance = await _balanceService.GetBalanceAsync();
            return Ok(new { Balance = balance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBalance([FromBody] BalanceRequest request)
    {
        try
        {
            var newBalance = await _balanceService.UpdateBalanceAsync(request.BalanceAmount);
            return Ok(new { NewBalance = newBalance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
