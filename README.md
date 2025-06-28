# KeyPass Authentication Proof of Concept

A demonstration project for passwordless authentication using FIDO2/WebAuthn standards. This solution shows how to implement secure authentication using hardware or platform authenticators (security keys, Windows Hello, Touch ID) instead of traditional passwords.

## Project Structure

- `source/`
   - `wr-fido2-demo.auth.host/` - Main authentication service implementation

## Quick Start

1. Build and run the authentication host project
2. Navigate to the login page
3. Register a new credential using your authenticator device
4. Test the login flow

## Use Cases

Detailed documentation for each use case is available in their respective directories:
- [Passkey Registration](source/wr-fido2-demo.auth.host/UseCases/PasskeyRegistration/_README.md) - Creating new passkey credentials
- [Passkey Login](source/wr-fido2-demo.auth.host/UseCases/PasskeyLogin/_README.md) - Authenticating with existing passkeys

## Details

For detailed information about specific components and implementations, refer to the README.md files in respective project directories:
- Authentication Host: See `source/wr-fido2-demo.auth.host/README.md`

## References

- [FIDO2 Overview and Specifications](https://fidoalliance.org/fido2/)
- [FIDO2 .NET Library](https://github.com/passwordless-lib/fido2-net-lib)