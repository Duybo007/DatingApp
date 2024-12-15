namespace API.Helpers
{
    // The UserParams class is a helper class that defines parameters used for paginating user data.
    public class UserParams : PaginationParams
    {
        public string? Gender { get; set; }
        public string? CurrentUsername { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
    }
}
