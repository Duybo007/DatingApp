using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    // A helper class for handling paginated data, generic for any type <T>.
    public class PagedList<T> : List<T> // PagedList<T> class is inheriting from the generic List<T> class in C#. PagedList<T> is an extended version of List<T>
    {
        // Constructor initializes the paginated list and calculates pagination details.
        // The constructor of PagedList<T> is designed to take an IEnumerable<T> items parameter and add those items to the list using AddRange(items)
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            // The current page being viewed.
            CurrentPage = pageNumber;

            // Total number of pages, calculated by dividing the total item count by the page size.
            // Math.Ceiling ensures partial pages are rounded up to the next whole number.
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // The number of items displayed per page.
            PageSize = pageSize;

            // The total number of items in the dataset.
            TotalCount = count;

            // Add items to the list (this is why items are part of the PagedList object).
            // places the items in the underlying list
            AddRange(items);
        }

        // The current page number.
        public int CurrentPage { get; set; }

        // The total number of pages in the dataset.
        public int TotalPages { get; set; }

        // The number of items to display on each page.
        public int PageSize { get; set; }

        // The total count of items in the entire dataset.
        public int TotalCount { get; set; }

        // Static method to create a paginated list from an IQueryable source.
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            // Counts the total number of items in the source dataset.
            var count = await source.CountAsync();

            // Retrieves the items for the current page using Skip and Take.
            // Skip skips over items from previous pages. Take selects the items for the current page.
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Returns a new PagedList with the current page's items and pagination details.
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}

// The items (MemberDto) are added to the PagedList object because it inherits from List<T>
// The AddRange(items) method in the constructor places the items in the underlying list.

// The PagedList<T> adds additional properties (CurrentPage, TotalPages, etc.) to describe pagination details, which are not part of the base List<T>

//The PagedList<T> combines both the list of items and the pagination metadata into a single object