# FIDO2 Explained

FIDO2 is an open standard for passwordless authentication using public-key cryptography, enabling secure, phishing-resistant logins via passkeys.

## Terminology

- **RP or Relying Party**: The backend server or service that verifies user authentication (e.g., your .NET app).
- **WebAuthn**: JavaScript API in browsers for creating and retrieving credentials during registration and authentication.
- **CTAP2**: Client-to-Authenticator Protocol 2, defining how clients (e.g., browsers) communicate with authenticators over transports like USB, NFC, or Bluetooth.
- **Passkey**: A FIDO2-compliant credential stored on a device (e.g., phone or security key), consisting of asymmetric key pairs (private key never leaves the device; public key shared with RP).
- **Assertion**: Signed proof of private key possession sent during authentication, including a challenge signature for verification.
- **Attestation**: Metadata and signature from the authenticator during registration, proving its trustworthiness and the credential's origin.

## Key Flows

### Registration (Creating a Passkey)
1. RP sends a challenge and options to the client (browser).
2. Client invokes WebAuthn to prompt the authenticator (via CTAP2) to generate a key pair and user gesture (e.g., biometric).
3. Authenticator returns attestation, public key, and credential ID to client.
4. Client forwards to RP, which stores the public key and verifies attestation.

### Authentication (Logging In)
1. RP sends a challenge and allowed credential IDs to the client.
2. Client invokes WebAuthn; authenticator signs the challenge with private key after user gesture.
3. Authenticator returns assertion to client.
4. Client forwards to RP, which verifies the signature using the stored public key.

## Benefits
- **Security**: Phishing-resistant; no shared secrets; private keys device-bound.
- **Usability**: Passwordless with biometrics/PIN; syncable across devices.
- **Interoperability**: Works with .NET libraries like Fido2.Net for server-side implementation.
