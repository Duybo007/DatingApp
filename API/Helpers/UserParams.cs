namespace API.Helpers
{
    public class UserParams : PaginationParams
    {        
        public string CurrentUsername { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
    }
}

// The purpose of this class is to encapsulate the parameters required for pagination in a user-friendly manner.

// Here's a breakdown of the code:

// 1. The `UserParams` class has three properties: `PageNumber`, `_pageSize`, and `PageSize`.

// 2. The `PageNumber` property is an integer that represents the current page number. It has a default value of 1.

// 3. The `_pageSize` property is a private integer that represents the maximum number of items to display on a page. It has a default value of 10.

// 4. The `PageSize` property is a public integer that represents the maximum number of items to display on a page. It has a default value of 10.

// 5. The `PageSize` property has a setter that checks if the value being assigned is greater than the `MaxPageSize` constant (50). If it is, the setter assigns the value of `MaxPageSize` to the `_pageSize` field. Otherwise, it assigns the value being assigned to the `_pageSize` field.

// In summary, the `UserParams` class is used to store the pagination parameters for a user. It provides a default value for the `PageNumber` property and ensures that the `PageSize` property cannot exceed the `MaxPageSize` constant. This class can be used in conjunction with other classes and methods to implement pagination in a user-friendly and efficient manner.