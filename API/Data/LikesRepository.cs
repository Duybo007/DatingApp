using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    // This repository handles the "likes" functionality and implements the ILikesRepository interface
    public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
    {
        // Adds a like relationship to the database.
        // `like.SourceUserId`: ID of the user who is liking.
        // `like.TargetUserId`: ID of the user being liked.
        public void AddLike(UserLike like)
        {
            context.Likes.Add(like); // Adds the like relationship to the database context.
        }

        // Removes a like relationship from the database.
        // `like.SourceUserId`: ID of the user who originally liked.
        // `like.TargetUserId`: ID of the user who was liked.
        public void DeleteLike(UserLike like)
        {
            context.Likes.Remove(like); // Removes the like relationship from the database context.
        }

        // Retrieves a list of target user IDs that the current user has liked.
        // `currentUserId`: The ID of the user who is doing the liking.
        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
        {
            return await context.Likes
                .Where(x => x.SourceUserId == currentUserId) // Filter likes where the current user is the one doing the liking.
                .Select(x => x.TargetUserId) // Select the IDs of the users who were liked.
                .ToListAsync();
        }

        // Retrieves a specific like relationship based on the source user (liker) and target user (liked).
        // `sourceUserId`: The ID of the user who is liking.
        // `targetUserId`: The ID of the user being liked.
        public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await context.Likes.FindAsync(sourceUserId, targetUserId); // Finds the like relationship in the database.
        }

        // Retrieves a list of MemberDto objects based on the given predicate and user ID.
        // `predicate`: Determines the type of likes to retrieve ("liked", "likedBy", or mutual likes).
        // `userId`: The ID of the user for whom the likes are being retrieved.
        public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
        {
            var likes = context.Likes.AsQueryable(); // Start querying the Likes table.
            IQueryable<MemberDto> query;

            switch (likesParams.Predicate)
            {
                case "liked":
                    // Retrieve the users that the given user has liked.
                    query = likes
                        .Where(x => x.SourceUserId == likesParams.UserId) // Filter for likes where the given user is the liker.
                        .Select(x => x.TargetUser) // Select the target users (users who were liked).
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider); // Map the users to MemberDto objects.
                    break;
                case "likedBy":
                    // Retrieve the users who have liked the given user.
                    query = likes
                        .Where(x => x.TargetUserId == likesParams.UserId) // Filter for likes where the given user is the liked user.
                        .Select(x => x.SourceUser) // Select the source users (users who did the liking).
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider); // Map the users to MemberDto objects.
                    break;

                default:
                    // Retrieve mutual likes (users who both liked and were liked by the given user).
                    var likeIds = await GetCurrentUserLikeIds(likesParams.UserId); // Get the IDs of users that the given user has liked.

                    query = likes
                        .Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId)) // Filter for mutual likes.
                        .Select(x => x.SourceUser) // Select the source users (users who liked the given user).
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider); // Map the users to MemberDto objects.
                    break;
            }

            return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
        }

        // Saves changes to the database and returns true if successful.
        public async Task<bool> SaveChanges()
        {
            return await context.SaveChangesAsync() > 0; // Returns true if one or more database rows were updated.
        }
    }
}
