using Microsoft.AspNetCore.Identity;

namespace Core_WebProj.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public required int Age { get; set; }
        public  byte[]? ProfilePicture { get; set; }
    }
}
