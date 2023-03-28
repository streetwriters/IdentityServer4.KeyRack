using System.Collections.Generic;
using System.Threading.Tasks;

using Streetwriters.IdentityServer4.KeyRack.Model;

namespace Streetwriters.IdentityServer4.KeyRack
{
    /// <summary>
    /// Key management facade
    /// </summary>
    public interface IKeyService
    {
        /// <summary>Returns the current active signing key</summary>
        Task<RsaKey> GetCurrentKeyAsync();

        /// <summary>Returns all keys that could be used for validation</summary>
        Task<IEnumerable<RsaKey>> GetAllKeysAsync();
    }
}