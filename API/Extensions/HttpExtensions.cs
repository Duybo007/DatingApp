using System.Text.Json; // Provides JSON serialization capabilities.
using API.Helpers; // Importing the PaginationHeader class and PagedList helper.

namespace API.Extensions
{
    // Static class for extending HTTP response capabilities.
    public static class HttpExtensions
    {
        // Extension method to add a custom pagination header to the HTTP response.
        // This is especially useful for APIs to include pagination details in the response headers.
        public static void AddPaginationHeader<T>(this HttpResponse response, PagedList<T> data)
        {
            // Create a new instance of PaginationHeader to store pagination details.
            // This takes the current page, page size, total count of items, and total pages.
            var paginationHeader = new PaginationHeader(
                data.CurrentPage,
                data.PageSize,
                data.TotalCount,
                data.TotalPages
            );

            // Define JSON serialization options to ensure the JSON uses camelCase naming.
            // CamelCase is the preferred naming convention for JSON keys in most APIs.
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Serialize the pagination header object to a JSON string.
            // The JSON string will include pagination details in camelCase format.
            var paginationJson = JsonSerializer.Serialize(paginationHeader, jsonOptions);

            // Add the serialized JSON string as a custom header named "Pagination" in the HTTP response.
            response.Headers.Append("Pagination", paginationJson);

            // Add an additional header to expose the "Pagination" header to the client.
            // Without this, some browsers and clients may block the "Pagination" header for security reasons.
            response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
