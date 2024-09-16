using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly SlotsService _slotsService;

    public SlotsController(SlotsService spinService)
    {
        _slotsService = spinService;
    }

    [HttpPost("configure")]
    public async Task<IActionResult> ConfigureMatrix([FromBody] MatrixRequest request)
    {
        try
        {
            var newMatrix = await _slotsService.ConfigureMatrixAsync(request.Width, request.Height);
            return Ok(new { Width = newMatrix[0], Height = newMatrix[1] });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("spin")]
    public async Task<IActionResult> Spin([FromBody] SpinRequest request)
    {
        try
        {
            var result = await _slotsService.SpinAsync(request.BetAmount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
