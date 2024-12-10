using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Calculates the age based on a given Date of Birth (DOB).
        /// </summary>
        /// <param name="dob">The DateOnly instance representing the person's date of birth.</param>
        /// <returns>The calculated age as an integer.</returns>
        public static int CalculateAge(this DateOnly dob) // 'this' keyword makes it an extension method for DateOnly
        {
            // Get today's date as a DateOnly instance
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Calculate initial age difference by subtracting the years
            var age = today.Year - dob.Year;

            // Check if the birthday hasn't occurred yet this year
            // If so, subtract 1 from the age
            if (dob > today.AddYears(-age)) age--;

            return age; // Return the final calculated age
        }
    }
}