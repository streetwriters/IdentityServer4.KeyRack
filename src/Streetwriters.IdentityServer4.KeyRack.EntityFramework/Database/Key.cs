using System;

namespace Streetwriters.IdentityServer4.KeyRack.EntityFramework
{
    public class Key
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}