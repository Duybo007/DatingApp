using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
        
    }
}

// This code is an extension method for the `HttpResponse` class in ASP.NET Core. It adds a custom header called "Pagination" to the response, which contains information about the pagination of the data returned by the API.

// Here's a breakdown of the code:

// 1. The `HttpExtensions` class is defined. This class is used to group extension methods for the `HttpResponse` class.

// 2. The `AddPaginationHeader` method is defined as a public static method. This method takes an `HttpResponse` object and a `PaginationHeader` object as parameters.

// 3. Inside the `AddPaginationHeader` method, a new instance of `JsonSerializerOptions` is created. This object is used to configure the JSON serializer. In this case, the `PropertyNamingPolicy` property is set to `JsonNamingPolicy.CamelCase`, which means that the property names in the JSON output will be in camel case.

// 4. The `PaginationHeader` object is serialized into a JSON string using the `JsonSerializer.Serialize` method. The serialized JSON string is then added as the value of the "Pagination" header in the HTTP response.

// 5. The "Access-Control-Expose-Headers" header is added to the HTTP response with the value "Pagination". This header is used to indicate which headers can be exposed as part of the response by the server. In this case, it allows the "Pagination" header to be exposed.

// To use this extension method, you can simply call the `AddPaginationHeader` method on an `HttpResponse` object, passing in a `PaginationHeader` object as a parameter. For example:


// csharp
// var paginationHeader = new PaginationHeader(10, 20, 100);
// response.AddPaginationHeader(paginationHeader);
// ​


// This will add the "Pagination" header to the HTTP response, containing the pagination information in JSON format.