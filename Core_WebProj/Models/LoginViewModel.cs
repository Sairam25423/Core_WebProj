﻿using System.ComponentModel.DataAnnotations;

namespace Core_WebProj.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
