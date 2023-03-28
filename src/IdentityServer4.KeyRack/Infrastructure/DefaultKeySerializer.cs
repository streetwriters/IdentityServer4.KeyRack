using IdentityServer4.KeyRack.Model;

namespace IdentityServer4.KeyRack
{
    /// <summary>Plain JSON key serializer</summary>
    public class DefaultKeySerializer : IKeySerializer
    {
        public RsaKey Deserialize(SerializedKey serializedKey) =>
            JsonSerializer.Deserialize<RsaKey>(serializedKey.Data);

        public SerializedKey Serialize(RsaKey key) =>
            new SerializedKey(key, JsonSerializer.Serialize(key));
    }
}