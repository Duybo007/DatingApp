using System.IdentityModel.Tokens.Jwt; // Provides classes for creating and reading JWT tokens
using System.Security.Claims; // Enables creating and handling user claims
using System.Text; // Provides methods for encoding strings
using API.Entities; // Reference to the AppUser entity
using API.Interfaces; // Reference to the ITokenService interface
using Microsoft.IdentityModel.Tokens; // Provides classes for signing and validating tokens

namespace API.Services
{
    // TokenService is responsible for generating JSON Web Tokens (JWTs) for user authentication
    public class TokenService(IConfiguration config) : ITokenService
    {
        // CreateToken generates a JWT token for the specified user
        public string CreateToken(AppUser user)
        {
            // Retrieves the token key from app settings
            var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from app settings");

            // Ensures the token key is sufficiently long for security
            if (tokenKey.Length < 60) throw new Exception("Token key needs to be longer");

            // Encodes the token key into a symmetric security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            // Defines the claims to include in the token; here, it includes the user's username
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserName) // Claim: User's unique identifier
            };

            // Creates signing credentials using the token key and HMAC-SHA512 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Configures the token's properties, such as claims, expiration, and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Attaches claims to the token
                Expires = DateTime.UtcNow.AddDays(7), // Sets the token to expire in 7 days
                SigningCredentials = creds // Applies the signing credentials
            };

            // Initializes a handler for creating and managing JWT tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            // Creates the JWT token based on the descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Serializes the token to a string format to return to the client
            return tokenHandler.WriteToken(token);
        }
    }
}
