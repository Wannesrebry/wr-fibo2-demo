using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using wr_fido2_demo.auth.host.Controllers;
using wr_fido2_demo.auth.host.Development;

namespace wr_fido2_demo.auth.host.UseCases.PasskeyCredentialCreation;

public sealed record GetRegistrationOptionsQuery : IRequest<CredentialCreateOptions>
{
    [FromForm] public required string Username { get; init; }
    [FromForm] public required string DisplayName { get; init; }
    [FromForm] public string? AttestationType { get; init; }
    [FromForm] public string? AuthenticatorType { get; init; }
    [FromForm] public string? ResidentKey { get; init; }
    [FromForm] public string? UserVerification { get; init; }
}

internal sealed class
    GetRegistrationOptionsQueryHandler : IRequestHandler<GetRegistrationOptionsQuery, CredentialCreateOptions>
{
    private readonly IFido2 _fido2;
    private readonly DevelopmentInMemoryStore _inMemoryStore;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetRegistrationOptionsQueryHandler(IFido2 fido2, IHttpContextAccessor httpContextAccessor)
    {
        _fido2 = fido2;
        _inMemoryStore = DevelopmentInMemoryStore.Instance;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CredentialCreateOptions> Handle(GetRegistrationOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var username = request.Username;
        if (string.IsNullOrEmpty(username))
        {
            username = $"{request.DisplayName} (Usernameless user created at {DateTime.UtcNow})";
        }

        // 1. Get user from DB by username (auto create missing users)
        var user = _inMemoryStore.GetOrAddUser(username, () => new Fido2User
        {
            DisplayName = request.DisplayName,
            Name = username,
            Id = Encoding.UTF8.GetBytes(username)
        });

        // 2. Get user existing keys by username
        var existingKeys = _inMemoryStore.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

        // 3. Create options
        var authenticatorSelection = new AuthenticatorSelection
        {
            ResidentKey = request.ResidentKey?.ToEnum<ResidentKeyRequirement>() ?? ResidentKeyRequirement.Preferred,
            UserVerification = request.UserVerification?.ToEnum<UserVerificationRequirement>() ??
                               UserVerificationRequirement.Preferred
        };

        if (!string.IsNullOrEmpty(request.AuthenticatorType))
            authenticatorSelection.AuthenticatorAttachment =
                request.AuthenticatorType.ToEnum<AuthenticatorAttachment>();

        var exts = new AuthenticationExtensionsClientInputs()
        {
            Extensions = true,
            UserVerificationMethod = true,
            CredProps = true
        };

        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = user,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = authenticatorSelection,
            AttestationPreference = request.AttestationType?.ToEnum<AttestationConveyancePreference>() ??
                                    AttestationConveyancePreference.Direct,
            Extensions = exts
        });

        // 4. Store options in session
        _httpContextAccessor.HttpContext?.Session.SetString("fido2.attestationOptions", options.ToJson());

        return await Task.FromResult(options);
    }
}