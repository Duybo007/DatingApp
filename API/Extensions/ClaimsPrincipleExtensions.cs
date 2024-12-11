using System.Security.Claims; // Provides classes for creating and managing user claims

namespace API.Extensions
{
    // This extension class adds utility methods to ClaimsPrincipal for working with user claims
    public static class ClaimsPrincipleExtensions
    {
        // Retrieves the username from the claims of the current user (typically from a JWT token)
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // Extracts the value of the NameIdentifier claim from the user's token
            var username = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("Cannot get username from token");

            // Returns the username if found, otherwise throws an exception
            return username;
        }
    }
}