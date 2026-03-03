using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class AppUser : IdentityUser
    {
        // The person's full name
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
