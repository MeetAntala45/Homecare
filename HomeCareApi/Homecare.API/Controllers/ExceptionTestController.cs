using Homecare.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Dev;


[ApiController]
[Route("api/dev/test-exceptions")]
public class ExceptionTestController : ControllerBase
{

    [HttpGet("500")]
    public IActionResult Throw500()
    {
        throw new Exception("Simulated unhandled server error — 500");
    }

    [HttpGet("500-inner")]
    public IActionResult Throw500WithInner()
    {
        try
        {
            int[] arr = new int[3];
            _ = arr[99];
        }
        catch (Exception inner)
        {
            throw new Exception("Outer message wrapping inner exception", inner);
        }

        return Ok();
    }

    [HttpGet("404")]
    public IActionResult Throw404()
    {
        throw new KeyNotFoundException("Simulated resource not found — 404");
    }

    [HttpGet("400")]
    public IActionResult Throw400()
    {
        throw new ArgumentException("Simulated bad argument — 400");
    }

    [HttpGet("409")]
    public IActionResult Throw409()
    {
        throw new InvalidOperationException("Simulated conflict — 409");
    }

    [HttpGet("401")]
    public IActionResult Throw401()
    {
        throw new UnauthorizedException("Simulated unauthorized — 401");
    }

    [HttpGet("null-ref")]
    public IActionResult ThrowNullRef()
    {
        string? s = null;
        _ = s!.Length;
        return Ok();
    }

    [HttpGet("divide-by-zero")]
    public IActionResult ThrowDivideByZero()
    {
        int x = 0;
        int y = 1 / x;
        return Ok(y);
    }


    [HttpGet("overflow")]
    public IActionResult ThrowOverflow()
    {
        checked
        {
            int max = int.MaxValue;
            int result = max + 1;
            return Ok(result);
        }
    }

    [HttpGet("format")]
    public IActionResult ThrowFormat()
    {
        int.Parse("not-a-number");
        return Ok();
    }

    [HttpGet("custom-message")]
    public IActionResult ThrowCustomMessage([FromQuery] string msg = "Custom test exception")
    {
        throw new Exception(msg);
    }
}