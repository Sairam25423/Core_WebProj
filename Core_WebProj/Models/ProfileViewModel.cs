namespace Core_WebProj.Models
{
    public class ProfileViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required int Age { get; set; }
        public byte[] ?ProfilePicture { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
