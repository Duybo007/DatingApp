using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // AdminController inherits from BaseApiController.
    // This controller handles admin-related operations like managing roles and moderating photos.
    public class AdminController(UserManager<AppUser> userManager) : BaseApiController
    {

        // [Authorize] ensures only users with the "RequireAdminRole" policy can access this endpoint.
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            // Fetch all users from the database and order them by username.
            // Then, project each user into a new object with their Id, Username, and a list of their assigned roles.
            var users = await userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.Id, // The user's unique identifier.
                    Username = x.UserName, // The user's username.
                    Roles = x.UserRoles.Select(r => r.Role.Name).ToList() // Fetch the roles associated with the user.
                })
                .ToListAsync();

            // Return the users with roles as a JSON response.
            return Ok(users);
        }

        // [Authorize] ensures only users with the "RequireAdminRole" policy can access this endpoint.
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            // If no roles are provided, return a bad request response.
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            // Split the roles string (comma-separated) into an array of individual roles.
            var selectedRoles = roles.Split(",").ToArray();

            // Find the user by their username.
            var user = await userManager.FindByNameAsync(username);

            // If the user doesn't exist, return a bad request response.
            if (user == null) return BadRequest("User not found");

            // Get the current roles assigned to the user.
            var userRoles = await userManager.GetRolesAsync(user);

            // Add the selected roles to the user, but only those roles that the user doesn't already have.
            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            // If adding roles fails, return a bad request response.
            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            // Remove roles from the user that are no longer in the selected roles list.
            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            // If removing roles fails, return a bad request response.
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            // Return the updated list of roles assigned to the user.
            return Ok(await userManager.GetRolesAsync(user));
        }

        // [Authorize] ensures only users with the "ModeratePhotoRole" policy can access this endpoint.
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            // This is a placeholder for photo moderation functionality.
            // Only users with the "ModeratePhotoRole" policy can see this message.
            return Ok("Only Admin can see this");
        }
    }
}
