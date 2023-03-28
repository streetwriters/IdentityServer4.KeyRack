using Streetwriters.IdentityServer4.KeyRack.Model;

namespace Streetwriters.IdentityServer4.KeyRack
{
    /// <summary>Interface to model key serialization/deserialization aspect</summary>
    public interface IKeySerializer
    {
        SerializedKey Serialize(RsaKey key);

        RsaKey Deserialize(SerializedKey serializedKey);
    }
}