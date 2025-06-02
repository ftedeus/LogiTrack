using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}

