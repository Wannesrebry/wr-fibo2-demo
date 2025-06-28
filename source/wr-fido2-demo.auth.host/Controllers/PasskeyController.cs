using MediatR;
using Microsoft.AspNetCore.Mvc;
using wr_fido2_demo.auth.host.UseCases.PasskeyLogin;

namespace wr_fido2_demo.auth.host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasskeyController: ControllerBase
{
    private readonly IMediator _mediator;
    public PasskeyController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("login/options")]
    public async Task<ActionResult<AssertionOptions>> GetLoginOptions(GetLoginOptionsQuery query)
    {
        var options = await _mediator.Send(query);
        return new OkObjectResult(options);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<VerifyAssertionResult>> Login([FromBody] AuthenticatorAssertionRawResponse clientResponse)
    {
        var command = new LoginCommand()
        {
            Assertion = clientResponse
        };
        var result = await _mediator.Send(command);
        return new OkObjectResult(result);
    }
}