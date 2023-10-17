using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Migrations
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
            
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }
        //This method retrieves a `UserLike` object from the database that represents the like relationship between two users. It uses the `FindAsync` method of the `_context.Likes` object to retrieve the `UserLike` object.


        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked")
            {
                likes = likes.Where(l => l.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            };

            if(likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(l => l.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            };

            var likedUsers =  users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }
        //This method retrieves a list of `LikeDto` objects from the database that represent the like relationships between a user and other users. It uses the `Where` method of the `_context.Likes` object to filter the likes based on the `predicate` parameter. The `Select` method is then used to project the filtered likes into a list of `LikeDto` objects.

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
        // This method retrieves an `AppUser` object from the database that represents a user and includes their liked users. It uses the `Include` method of the `_context.Users` object to include the liked users in the retrieved `AppUser` object.
    }
}