using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeReviews.Models
{
    public class AppUser : IdentityUser
    {
        [NotMapped]
        public IList<string> RoleNames { get; set; }
        // The person's full name
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
