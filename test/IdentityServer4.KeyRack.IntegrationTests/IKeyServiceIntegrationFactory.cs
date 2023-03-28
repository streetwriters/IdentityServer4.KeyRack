using IdentityServer4.KeyRack;

namespace IdentityServer4.KeyRack.IntegrationTests
{
    public interface IKeyServiceIntegrationFactory
    {
        IKeyService CreateService(KeyRackOptions options);
    }
}