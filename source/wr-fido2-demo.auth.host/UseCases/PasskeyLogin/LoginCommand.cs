using MediatR;
using wr_fido2_demo.auth.host.Controllers;
using wr_fido2_demo.auth.host.Development;

namespace wr_fido2_demo.auth.host.UseCases.PasskeyLogin;

public sealed record LoginCommand : IRequest<VerifyAssertionResult>
{
    public required AuthenticatorAssertionRawResponse Assertion { get; init; }
}

internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, VerifyAssertionResult>
{
    private readonly IFido2 _fido2;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DevelopmentInMemoryStore DemoStore = MyController.DemoStorage;
    public LoginCommandHandler(IFido2 fido2, IHttpContextAccessor httpContextAccessor)
    {
        _fido2 = fido2;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<VerifyAssertionResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the assertion options we sent the client
        var jsonOptions = _httpContextAccessor.HttpContext?.Session.GetString("fido2.assertionOptions");
        if (jsonOptions is null)
        {
            throw new ArgumentNullException();
        }
        var options = AssertionOptions.FromJson(jsonOptions);

        // 2. Get registered credential from database
        var creds = DemoStore.GetCredentialById(request.Assertion.RawId) ?? throw new Exception("Unknown credentials");

        // 3. Get credential counter from database
        var storedCounter = creds.SignCount;

        // 4. Create callback to check if the user handle owns the credentialId
        IsUserHandleOwnerOfCredentialIdAsync callback = static async (args, cancellationToken) =>
        {
            var storedCreds = await MyController.DemoStorage.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };

        // 5. Make the assertion
        var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = request.Assertion,
            OriginalOptions = options,
            StoredPublicKey = creds.PublicKey,
            StoredSignatureCounter = storedCounter,
            IsUserHandleOwnerOfCredentialIdCallback = callback
        }, cancellationToken: cancellationToken);

        // 6. Store the updated counter
        DemoStore.UpdateCounter(res.CredentialId, res.SignCount);

        return res;
    }
}