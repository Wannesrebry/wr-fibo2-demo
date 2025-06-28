# Passkey Login Use Case

This use case handles the authentication process using previously registered FIDO2 credentials (passkeys). It implements the WebAuthn assertion flow for user authentication.

## Flow Overview

1. **Initiate Login**
    - The client requests assertion options from the server
    - The server generates challenge and options including:
        - Challenge
        - Relying Party ID
        - Allowed credential IDs (if any)
        - User verification preferences

2. **Assertion Process**
    - The client receives options and prompts for the passkey
    - The authenticator signs the challenge using the private key
    - Creates an assertion response

3. **Verify Authentication**
    - Server validates the assertion response
    - Verifies the signature using stored public key
    - Checks and updates the signature counter
    - Establishes user session upon successful verification

## Security Features

- Challenge-response mechanism prevents replay attacks
- Signature counter helps detect cloned authenticators
- User verification can be required (e.g., PIN, biometric)
- Secure session management after successful authentication

## Important Parameters

- **AssertionResponse**: Contains the signed challenge and authenticator data
- **AssertionOptions**: Server-generated options for the authentication request
- **Allowed Credentials**: Optional list of acceptable credential IDs
- **User Verification**: Requirements for additional user verification

The implementation provides secure passwordless authentication while maintaining compatibility with FIDO2 WebAuthn standards.