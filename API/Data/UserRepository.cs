using API.DTOs;
using API.Entities; // For the `AppUser` entity representing users in the application
using API.Helpers;
using API.Interfaces; // For the `IUserRepository` interface definition
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore; // For database operations like querying and saving data

namespace API.Data
{
    // Repository implementation for user-related data access
    public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
    {
        public async Task<MemberDto?> GetMemberAsync(string username)
        {
            return await context.Users
                    .Where(x => x.UserName == username)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable();

            query = query.Where(x => x.UserName != userParams.CurrentUsername);

            if(userParams.Gender != null)
            {
                query = query.Where(x=> x.Gender == userParams.Gender);
            }

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where( x=> x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive),
            };

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
        }

        // Retrieve a user by their unique ID
        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            // Finds a user with the specified ID in the database
            return await context.Users.FindAsync(id);
        }

        // Retrieve a user by their username
        public async Task<AppUser?> GetUserByNameAsync(string username)
        {
            // Finds a user matching the provided username
            return await context.Users.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == username);
        }

        // Retrieve all users in the database
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            // Returns a list of all users
            return await context.Users.Include(x => x.Photos).ToListAsync();
        }

        // Save changes made to the database
        public async Task<bool> SaveAllSync()
        {
            // Saves any pending changes and checks if at least one record was affected
            return await context.SaveChangesAsync() > 0;
        }

        // Update an existing user entity
        public void Update(AppUser user)
        {
            // Marks the user entity as modified so it will be updated in the database
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
