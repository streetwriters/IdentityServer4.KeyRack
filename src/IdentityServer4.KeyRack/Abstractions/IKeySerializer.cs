using IdentityServer4.KeyRack.Model;

namespace IdentityServer4.KeyRack
{
    /// <summary>Interface to model key serialization/deserialization aspect</summary>
    public interface IKeySerializer
    {
        SerializedKey Serialize(RsaKey key);

        RsaKey Deserialize(SerializedKey serializedKey);
    }
}