_Note: This is a fork of [Usemam.IdentityServer4.KeyRack](https://github.com/usemam/Usemam.IdentityServer4.KeyRack)_

## About IdentityServer4.KeyRack

KeyRack is a seamless token signing key management extension for [IdentityServer4](https://github.com/IdentityServer/IdentityServer4). Its mission is to allow for keys to be created and rotated fully automatically - without any manual intervention.

### Packages

- [IdentityServer4.KeyRack](https://www.nuget.org/packages/IdentityServer4.KeyRack/) - core package
- [IdentityServer4.KeyRack.DataProtection](https://www.nuget.org/packages/IdentityServer4.KeyRack.DataProtection/) - adds data protection for keys using [Microsoft.AspNetCore.DataProtection.Abstractions](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection.Abstractions/)
- [IdentityServer4.KeyRack.EntityFramework](https://www.nuget.org/packages/IdentityServer4.KeyRack.EntityFramework/) - adds database persistence for keys
