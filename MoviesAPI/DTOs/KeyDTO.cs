using MoviesAPI.Entities;

namespace MoviesAPI.DTOs
{
    public class KeyDTO
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public bool Active { get; set; }
        public required string KeyType { get; set; }
        public List<DomainRestrictionDTO> DomainRestrictions { get; set; } = [];
        public List<IpRestrictionDTO> IpRestrictions { get; set; } = [];
    }

    public class CreateKeyDTO
    {
        public required KeyType KeyType { get; set; }
    }

    public class UpdateKeyDTO
    {
        public bool UpdateKey { get; set; }
        public bool Active { get; set; }
    }
}
