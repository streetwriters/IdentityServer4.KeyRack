using Streetwriters.IdentityServer4.KeyRack;

namespace Streetwriters.IdentityServer4.KeyRack.IntegrationTests
{
    public interface IKeyServiceIntegrationFactory
    {
        IKeyService CreateService(KeyRackOptions options);
    }
}