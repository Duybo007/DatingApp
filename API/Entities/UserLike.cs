namespace API.Entities
{
    public class UserLike
    {
        public AppUser SourceUser { get; set; }
        public int SourceUserId { get; set; }
        public AppUser TargetUser { get; set; } //who being liked by SourceUser
        public int TargetUserId { get; set; }
    }
}