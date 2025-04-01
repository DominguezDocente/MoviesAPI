namespace MoviesAPI.Entities
{
    public class APIRequest
    {
        public int Id { get; set; }

        public int APIKeyId { get; set; }

        public APIKey APIKey { get; set; }

        public DateTime RequestDate { get; set; }

        public string? RemoteIp { get; internal set; }
    }
}
