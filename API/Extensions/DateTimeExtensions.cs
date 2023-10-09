using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly dob)   //because it is an extension method, need to specify what it is that we extending
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var age = today.Year - dob.Year;

            if ( dob > today.AddYears(-age) ) age--;    //if not DOB yet for this year

            return age;
        }
    }
}