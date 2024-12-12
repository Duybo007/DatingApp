using API.DTOs; // Importing Data Transfer Objects for User and Photo-related operations
using API.Entities; // Importing the AppUser and Photo entities
using API.Extensions; // For the User.GetUsername() extension method
using API.Interfaces; // Importing repository and service interfaces
using AutoMapper; // For mapping between entities and DTOs
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using Microsoft.AspNetCore.Mvc; // For controller-related classes

namespace API.Controllers;

// This controller handles user-related API requests, such as retrieving, updating users, and managing photos
[Authorize] // Ensures all endpoints require authentication
public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    // GET: /api/users
    [HttpGet] // Defines an HTTP GET endpoint
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        // Calls the repository to retrieve all users as MemberDto objects
        var users = await userRepository.GetMembersAsync();

        // Returns the list of users with a 200 OK response
        return Ok(users);
    }

    // GET: /api/users/{username}
    [HttpGet("{username}")] // Defines an HTTP GET endpoint with a route parameter "username"
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        // Retrieves a single user by their username from the repository
        var user = await userRepository.GetMemberAsync(username);

        // Returns a 404 Not Found response if the user does not exist
        if (user == null) return NotFound();

        // Returns the user as a MemberDto with a 200 OK response
        return user;
    }

    // PUT: /api/users
    [HttpPut] // Defines an HTTP PUT endpoint
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        // Retrieves the currently authenticated user using the User.GetUsername() extension
        var user = await userRepository.GetUserByNameAsync(User.GetUsername());

        // If the user is not found, return a 400 Bad Request response
        if (user == null) return BadRequest("Could not find user");

        // Maps the incoming MemberUpdateDto data onto the existing user entity
        mapper.Map(memberUpdateDto, user);

        // Saves the changes to the database, and if successful, returns a 204 No Content response
        if (await userRepository.SaveAllSync()) return NoContent();

        // If saving changes fails, return a 400 Bad Request response
        return BadRequest("Failed to update the user");
    }

    // POST: /api/users/add-photo
    [HttpPost("add-photo")] // Defines an HTTP POST endpoint for adding photos
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        // Retrieves the currently authenticated user using the User.GetUsername() extension
        var user = await userRepository.GetUserByNameAsync(User.GetUsername());

        // If the user is not found, return a 400 Bad Request response
        if (user == null) return BadRequest("Cannot find user");

        // Calls the photo service to upload the photo to Cloudinary or another storage
        var result = await photoService.AddPhotoAsync(file);

        // If the photo service returns an error, return a 400 Bad Request response with the error message
        if (result.Error != null) return BadRequest(result.Error.Message);

        // Creates a new Photo object to represent the uploaded photo
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri, // Stores the photo's URL
            PublicId = result.PublicId // Stores the Cloudinary public ID for the photo
        };

        if (user.Photos.Count == 0) photo.IsMain = true;

        // Adds the new photo to the user's photo collection
        user.Photos.Add(photo);

        // If saving changes to the database is successful, return a 201 Created response
        // CreatedAtAction generates a response that includes a "Location" header pointing to the resource's URL
        // Parameters:
        // - nameof(GetUser): Specifies the name of the action method ("GetUser") that retrieves the resource
        // - new { username = user.UserName }: Provides the route parameters required for the "GetUser" method, 
        //   ensuring the generated URL is correct. The username parameter is populated with the current user's username
        // - mapper.Map<PhotoDto>(photo): The newly created photo is returned as the response body in a DTO format
        //
        // This is done to adhere to RESTful standards, which recommend returning a 201 Created status 
        // with a "Location" header pointing to the newly created resource, 
        // even though in this case the resource being pointed to is the user profile rather than the photo itself.
        if (await userRepository.SaveAllSync())
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));

        // If saving changes fails, return a 400 Bad Request response
        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]   //  api/users/set-main-photo/photoId
    public async Task<ActionResult> SetmainPhoto(int photoId)
    {
        // Retrieves the currently authenticated user using the User.GetUsername() extension
        var user = await userRepository.GetUserByNameAsync(User.GetUsername());

        // If the user is not found, return a 400 Bad Request response
        if (user == null) return BadRequest("Cannot find user");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Cannot user this as main photo");

        var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
        photo.IsMain = true;

        if (await userRepository.SaveAllSync()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        // Retrieves the currently authenticated user using the User.GetUsername() extension
        var user = await userRepository.GetUserByNameAsync(User.GetUsername());

        // If the user is not found, return a 400 Bad Request response
        if (user == null) return BadRequest("Cannot find user");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await userRepository.SaveAllSync()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}
