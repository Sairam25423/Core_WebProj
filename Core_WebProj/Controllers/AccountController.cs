using Core_WebProj.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Core_WebProj.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RazorViewToStringRenderer _razorViewToStringRenderer;
        private readonly PdfService _pdfService;
        private readonly IEmailService _emailService;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RazorViewToStringRenderer razorViewToStringRenderer, PdfService pdfService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _pdfService = pdfService;
            _emailService = emailService;
        }
        public IActionResult Example()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Age = model.Age
                };
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    await using var memoryStream = new MemoryStream();
                    await model.ProfilePicture.CopyToAsync(memoryStream);
                    user.ProfilePicture = memoryStream.ToArray();
                }
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
            }
            return View(model);
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var reserLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);
                    return RedirectToAction("ResetPasswordConfirmation", "Account", new { email = model.Email, resetLink = reserLink });
                }
            }
            return View(model);
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Token))
                {
                    ModelState.AddModelError(string.Empty, "Invalid password reset token.");
                    return View(model);
                }
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var model = new ProfileViewModel
                {
                    FullName = user.FullName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Age = user.Age,
                    ProfilePicture = user.ProfilePicture ?? Array.Empty<byte>(),
                };
                return View(model);
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (model.ProfilePictureFile != null)
                    {
                        await using var memoryStream = new MemoryStream();
                        await model.ProfilePictureFile.CopyToAsync(memoryStream);
                        user.ProfilePicture = memoryStream.ToArray();
                    }

                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Age = model.Age;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Profile");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        public async Task<IActionResult> PrintProfile()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user != null)
            {
                var model = new ProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Age = user.Age,
                    ProfilePicture = user.ProfilePicture ?? Array.Empty<byte>()
                };

                var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync(this, "Profile", model);
                var pdf = _pdfService.GeneratePdfFromHtml(htmlContent);
                return File(pdf, "application/pdf", $"{user.FullName}.pdf");
            }
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> SendPasswordResetLink(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return RedirectToAction("ForgotPassword");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken, email }, Request.Scheme);

            // if you want to send an OTP instead of a reset link
            //var otp = new Random().Next(100000, 999999).ToString();
            //TempData["OTP"] = otp;
            //await _emailService.SendEmailAsync(user.Email, "Your OTP Code", $"Your OTP is <strong>{otp}</strong>");


            var emailBody = $"<p>Click the link below to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>";
            await _emailService.SendEamilAsync(email, "Password Reset", emailBody);

            return RedirectToAction("ForgotPasswordConfirmation", "Account", new { email });
        }
        public async Task<IActionResult> DownloadProfilePdf()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Age = user.Age,
                ProfilePicture = user.ProfilePicture ?? Array.Empty<byte>()
            };

            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync(this, "ProfilePdf", model);
            var pdfBytes = _pdfService.GeneratePdfFromHtml(htmlContent);

            return File(pdfBytes, "application/pdf", $"{user.FullName}.pdf");
        }
    }
}
