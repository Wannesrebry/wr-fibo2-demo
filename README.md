# KeyPass Authentication Proof of Concept

This project demonstrates a proof of concept (PoC) for passwordless authentication using FIDO2/WebAuthn standards, often referred to as "keypass" authentication. The goal is to show how users can register and log in securely using hardware or platform authenticators (such as security keys, Windows Hello, or Touch ID) instead of traditional passwords.

## Features
- User registration and login with FIDO2/WebAuthn
- In-memory credential storage for demonstration purposes
- Minimal .NET backend and JavaScript frontend integration

## How it works
1. **Registration:**
   - The user registers a new credential using a keypass device.
   - The backend stores the credential information in memory.
2. **Login:**
   - The user authenticates using their registered keypass device.
   - The backend verifies the authentication response.

## Limitations
- This is a PoC and not production-ready.
- Credentials are not persisted (in-memory only).
- Security hardening and advanced error handling are not implemented.

## Project Structure
- `Controllers/` – API endpoints for registration and login
- `Development/` – In-memory credential store
- `wwwroot/js/` – Client-side WebAuthn logic

## Getting Started
1. Build and run the .NET project.
2. Open the login page in your browser.
3. Register a new credential and test login with your keypass device.

For more details, see the code and comments in the respective files.

## References

- [FIDO2 .NET Library Examples](https://github.com/passwordless-lib/fido2-net-lib#examples)
