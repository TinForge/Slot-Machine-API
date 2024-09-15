using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet]
    public TestGet Get()
    {
        return new TestGet() { Summary = "TEST TEST TEST" };
    }
}

public class TestGet
{
    public string Summary { get; set; }

}