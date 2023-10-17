using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source , int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();  //count the number of items available from source
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();    //get items using skip and take
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}

// This code defines a class called `PagedList<T>` that inherits from the `List<T>` class. The purpose of this class is to provide a way to handle pagination in APIs.

// Here's a breakdown of the code:

// 1. The constructor takes in the following parameters:
// - `items`: The list of items to be included in the current page.
// - `count`: The total number of items available from the source.
// - `pageNumber`: The current page number.
// - `pageSize`: The maximum number of items to be included in a page.

// 2. The constructor initializes the following properties:
// - `CurrentPage`: The current page number.
// - `TotalPages`: The total number of pages available.
// - `PageSize`: The maximum number of items to be included in a page.
// - `TotalCount`: The total number of items available from the source.

// 3. The `CreateAsync` method is a static method that takes in the following parameters:
// - `source`: The source of data to be paginated.
// - `pageNumber`: The current page number.
// - `pageSize`: The maximum number of items to be included in a page.

// 4. Inside the `CreateAsync` method, the following steps are performed:
// - The total number of items available from the source is counted using the `CountAsync` method.
// - The items for the current page are retrieved using the `Skip` and `Take` methods. The `Skip` method is used to skip the items from the previous pages, and the `Take` method is used to limit the number of items to the current page size.
// - A new instance of the `PagedList<T>` class is created with the retrieved items, the total count of items, the current page number, the total number of pages, and the page size.

// 5. The `CreateAsync` method returns the created `PagedList<T>` instance.

// This class can be used in APIs to handle pagination by creating a new instance of the `PagedList<T>` class with the appropriate parameters. The `CreateAsync` method can be used to simplify the process of creating a `PagedList<T>` instance.</s