using MediatR;
using Microsoft.AspNetCore.Mvc;
using wr_fido2_demo.auth.host.Controllers;
using wr_fido2_demo.auth.host.Development;

namespace wr_fido2_demo.auth.host.UseCases.PasskeyLogin;

public sealed record GetLoginOptionsQuery : IRequest<AssertionOptions>
{
    [FromForm] public required string UserName { get; init; }
    [FromForm] public string? UserVerification { get; init; } = null;
}

internal sealed class GetLoginOptionsQueryHandler : IRequestHandler<GetLoginOptionsQuery, AssertionOptions>
{
    private readonly IFido2 _fido2;
    private readonly DevelopmentInMemoryStore _inMemoryStore;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public GetLoginOptionsQueryHandler(IFido2 fido2, IHttpContextAccessor httpContextAccessor)
    {
        _inMemoryStore = DevelopmentInMemoryStore.Instance;
        _fido2 = fido2;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<AssertionOptions> Handle(GetLoginOptionsQuery request, CancellationToken cancellationToken)
    {
        var username = request.UserName;
        var userVerification = request.UserVerification;
        
        var existingCredentials = new List<PublicKeyCredentialDescriptor>();
        if (!string.IsNullOrEmpty(username))
        {
            // 1. Get user from DB
            var user = _inMemoryStore.GetUser(username) ?? throw new ArgumentException("Username was not registered");

            // 2. Get registered credentials from database
            existingCredentials = _inMemoryStore.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();
        }
        
        var exts = new AuthenticationExtensionsClientInputs()
        {
            Extensions = true,
            UserVerificationMethod = true
        };
        
        // 3. Create options
        var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams()
        {
            AllowedCredentials = existingCredentials,
            UserVerification = uv,
            Extensions = exts
        });
        
        // 4. Temporarily store options, session/in-memory cache/redis/db
        _httpContextAccessor.HttpContext?.Session.SetString("fido2.assertionOptions", options.ToJson());

        return await Task.FromResult(options);
    }
}