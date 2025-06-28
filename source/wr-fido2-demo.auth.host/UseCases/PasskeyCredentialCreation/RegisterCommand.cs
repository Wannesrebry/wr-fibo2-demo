using MediatR;
using wr_fido2_demo.auth.host.Controllers;
using wr_fido2_demo.auth.host.Development;

namespace wr_fido2_demo.auth.host.UseCases.PasskeyCredentialCreation;

public sealed record RegisterCommand : IRequest<RegisteredPublicKeyCredential>
{
    public required AuthenticatorAttestationRawResponse AttestationResponse { get; init; }
}

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisteredPublicKeyCredential>
{
    private readonly IFido2 _fido2;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DevelopmentInMemoryStore _inMemoryStore = DevelopmentInMemoryStore.Instance;

    public RegisterCommandHandler(IFido2 fido2, IHttpContextAccessor httpContextAccessor)
    {
        _fido2 = fido2;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RegisteredPublicKeyCredential> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the options we sent the client
        var jsonOptions = _httpContextAccessor.HttpContext?.Session.GetString("fido2.attestationOptions");
        if (jsonOptions is null)
        {
            throw new ArgumentNullException(nameof(jsonOptions), "No attestation options found in session");
        }

        var options = CredentialCreateOptions.FromJson(jsonOptions);

        // 2. Create callback to check if credential id is unique
        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
        {
            var users = await _inMemoryStore.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            return users.Count == 0;
        };

        // 3. Create the credentials
        var credential = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = request.AttestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = callback
        }, cancellationToken);

        // 4. Store the credentials
        _inMemoryStore.AddCredentialToUser(options.User, new StoredCredential
        {
            Id = credential.Id,
            PublicKey = credential.PublicKey,
            UserHandle = credential.User.Id,
            SignCount = credential.SignCount,
            AttestationFormat = credential.AttestationFormat,
            RegDate = DateTimeOffset.UtcNow,
            AaGuid = credential.AaGuid,
            Transports = credential.Transports,
            IsBackupEligible = credential.IsBackupEligible,
            IsBackedUp = credential.IsBackedUp,
            AttestationObject = credential.AttestationObject,
            AttestationClientDataJson = credential.AttestationClientDataJson
        });

        return credential;
    }
}