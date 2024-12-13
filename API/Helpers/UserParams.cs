namespace API.Helpers
{
    // The UserParams class is a helper class that defines parameters used for paginating user data.
    public class UserParams
    {
        // Constant for the maximum allowable page size. This prevents clients from requesting
        // an excessively large number of records in a single page.
        private const int MaxPageSize = 50;

        // Property to specify the current page number in the pagination request.
        // By default, this will be 1 unless explicitly set by the client.
        public int PageNumber { get; set; } = 1;

        // Private field to hold the page size value. 
        // This value is capped by the MaxPageSize constant.
        private int _pageSize = 10; // Default page size if none is specified by the client.

        // Public property to get or set the page size.
        public int PageSize
        {
            get => _pageSize; // Return the current page size value.
            
            set
            {
                // If the value specified by the client exceeds MaxPageSize,
                // cap it at MaxPageSize; otherwise, use the value specified.
                _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        public string? Gender { get; set; }
        public string? CurrentUsername { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
    }
}
