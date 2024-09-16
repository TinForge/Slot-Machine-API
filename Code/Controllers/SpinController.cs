using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SpinController : ControllerBase
{
    private readonly SpinService _spinService;

    public SpinController(SpinService spinService)
    {
        _spinService = spinService;
    }

    [HttpPost("configure")]
    public async Task<IActionResult> ConfigureMatrix([FromBody] MatrixRequest request)
    {
        try
        {
            var newMatrix = await _spinService.ConfigureMatrixAsync(request.Width, request.Height);
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
            var result = await _spinService.SpinAsync(request.BetAmount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
