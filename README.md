# Impersonate

VS 2008 VB.NET class library (.NET 3.5) working copy. ProcessAsUser duplicates explorer.exe's primary token and CreateProcessAsUser-launches a command line on the default WinSta0 desktop. ProcessAsUser2 tries eight LogonUser, DuplicateTokenEx, CreateProcessWithLogonW, and WindowsIdentity.Impersonate methods; sample usernames and passwords are gitignored (see ProcessAsUser2.vb.example). Open `Impersonate.sln`. This is a historical working copy from Dave Robinson / VaderConsulting.

**Source last updated:** 2010-05-03  
**Language:** VB.NET  
**Target:** v3.5  
**Output:** Library

## What it is

VS 2008 VB.NET class library (.NET 3.5) working copy. ProcessAsUser duplicates explorer.exe's primary token and CreateProcessAsUser-launches a command line on the default WinSta0 desktop. ProcessAsUser2 tries eight LogonUser, DuplicateTokenEx, CreateProcessWithLogonW, and WindowsIdentity.Impersonate methods; sample usernames and passwords are gitignored (see ProcessAsUser2.vb.example). Open `Impersonate.sln`. This is a historical working copy from Dave Robinson / VaderConsulting.

## Solution structure

| Project | Language | Path |
|---------|----------|------|
| `Impersonation` | VB.NET | `Impersonate/Impersonation.vbproj` |

## How to open

Open `Impersonate.sln` in Visual Studio.

## Attribution and provenance

- **Assembly company:** Microsoft
- **Assembly copyright:** Copyright © Microsoft 2010

## License

MIT. See `LICENSE`.
