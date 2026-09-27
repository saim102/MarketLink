
using IdentityDemoApp.Data;
using IdentityDemoApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;

namespace IdentityDemoApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


       

        [HttpGet]
        public IActionResult CustomerRegister()
        {
            return View();
        }


        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegister(
            CustomerRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                return View(model);
            }


        
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = false,
                FullName = model.FullName,
                UserType = "Customer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }


           
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Customer");

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                await _userManager.DeleteAsync(user);

                return View(model);
            }


           

            var customerProfile = new CustomerProfile
            {
                UserId = user.Id,
                FullName = model.FullName,
                ContactNumber = model.ContactNumber,
                Address = model.Address,
                City = model.City,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerProfiles.Add(customerProfile);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
              

                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    "",
                    "Unable to create customer profile. Please try again.");

                return View(model);
            }


          

            TempData["RegisterSuccess"] =
                "Your account has been created successfully. Please wait for Admin approval.";

            return RedirectToAction(nameof(Login));
        }


       

        [HttpGet]
        public IActionResult VendorRegister()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VendorRegister(
            VendorRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                return View(model);
            }


           
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = false,

                FullName = model.Name,
                UserType = "Vendor",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }


           

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Vendor");

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                await _userManager.DeleteAsync(user);

                return View(model);
            }


          

            var vendorProfile = new VendorProfile
            {
                UserId = user.Id,

                FarmName = model.StallBusinessName,
                Address = model.Address,

                City = string.Empty,
                Province = string.Empty,
                FarmDescription = string.Empty,

               
                IsApproved = false,

                CreatedAt = DateTime.UtcNow
            };

            _context.VendorProfiles.Add(vendorProfile);

            await _context.SaveChangesAsync();


          

            Response.Cookies.Append(
                "MarketLinkVendorEmail",
                model.Email,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });


            TempData["RegisterSuccess"] =
                "Vendor account created successfully. Please wait for admin approval.";

            return RedirectToAction(nameof(Login));
        }


        

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(
                "GoogleResponse",
                "Account");

            var properties =
                _signInManager.ConfigureExternalAuthenticationProperties(
                    "Google",
                    redirectUrl);

            properties.Items["prompt"] = "select_account";

            return Challenge(
                properties,
                "Google");
        }



        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                TempData["LoginError"] =
                    "Google login failed.";

                return RedirectToAction(nameof(Login));
            }

            var result =
                await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false);

            if (result.Succeeded)
            {
                var loggedInUser =
                    await _userManager.FindByLoginAsync(
                        info.LoginProvider,
                        info.ProviderKey);

                if (loggedInUser != null)
                {
                    return await RedirectToRoleDashboard(
                        loggedInUser);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var email =
                info.Principal.FindFirst(
                    System.Security.Claims.ClaimTypes.Email)
                ?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["LoginError"] =
                    "Google account email not found.";

                return RedirectToAction(nameof(Login));
            }

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,

                    FullName = email,
                    UserType = "Customer",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult =
                    await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        TempData["LoginError"] =
                            error.Description;
                    }

                    return RedirectToAction(
                        nameof(Login));
                }

                await _userManager.AddToRoleAsync(
                    user,
                    "Customer");
            }

            var loginResult =
                await _userManager.AddLoginAsync(
                    user,
                    info);

            if (!loginResult.Succeeded)
            {
                TempData["LoginError"] =
                    "Unable to connect Google account.";

                return RedirectToAction(
                    nameof(Login));
            }

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return await RedirectToRoleDashboard(
                user);
        }



        [HttpGet]
        public async Task<IActionResult> Login()
        {
           

            var vendorEmail =
                Request.Cookies["MarketLinkVendorEmail"];

            if (!string.IsNullOrEmpty(vendorEmail))
            {
                var vendorUser =
                    await _userManager.FindByEmailAsync(vendorEmail);

                if (vendorUser != null &&
                    vendorUser.UserType == "Vendor")
                {
                    var vendorProfile =
                        await _context.VendorProfiles
                            .FirstOrDefaultAsync(
                                v => v.UserId == vendorUser.Id);

                    if (vendorProfile != null &&
                        vendorProfile.IsApproved)
                    {
                        TempData["RegisterSuccess"] =
                            "Admin approved your account successfully. You can now login.";

                        Response.Cookies.Delete(
                            "MarketLinkVendorEmail");
                    }
                }
            }

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (user == null)
            {
                TempData["LoginError"] =
                    "Invalid email or password!";

                return View(model);
            }

            if (!user.IsActive)
            {
                TempData["LoginError"] =
                    "Your account is currently inactive.";

                return View(model);
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    false,
                    false);

            if (result.Succeeded)
            {

                
                if (user.UserType == "Vendor")
                {
                    Response.Cookies.Append(
                        "MarketLinkVendorEmail",
                        user.Email!,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(30)
                        });

                    return await RedirectToRoleDashboard(user);
                }


              

                if (user.UserType == "Customer")
                {
                    var customerProfile =
                        await _context.CustomerProfiles
                            .FirstOrDefaultAsync(
                                c => c.UserId == user.Id);


                  

                    if (customerProfile == null)
                    {
                        await _signInManager.SignOutAsync();

                        TempData["LoginError"] =
                            "Customer profile was not found.";

                        return RedirectToAction(
                            nameof(Login));
                    }


                    

                    if (!customerProfile.IsApproved)
                    {
                        await _signInManager.SignOutAsync();

                        TempData["LoginError"] =
                            "Your customer account is waiting for Admin approval. Please try again after your account has been approved.";

                        return RedirectToAction(
                            nameof(Login));
                    }



                    TempData["CustomerApprovedMessage"] =
                        "Your account has been approved by Admin successfully. Welcome to MarketLink!";

                    return await RedirectToRoleDashboard(user);
                }


                return await RedirectToRoleDashboard(user);
            }

            TempData["LoginError"] =
                "Invalid email or password!";

            return View(model);
        }


       
        private async Task<IActionResult> RedirectToRoleDashboard(
            ApplicationUser user)
        {
            var roles =
                await _userManager.GetRolesAsync(user);


          
            if (roles.Contains("Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }


           

            if (roles.Contains("Vendor"))
            {
                var vendorProfile =
                    _context.VendorProfiles
                        .FirstOrDefault(v =>
                            v.UserId == user.Id);

                if (vendorProfile == null)
                {
                    await _signInManager.SignOutAsync();

                    TempData["LoginError"] =
                        "Vendor profile was not found.";

                    return RedirectToAction(
                        nameof(Login));
                }

                if (!vendorProfile.IsApproved)
                {
                    await _signInManager.SignOutAsync();

                    TempData["LoginError"] =
                        "Your vendor account is waiting for admin approval.";

                    return RedirectToAction(
                        nameof(Login));
                }

                return RedirectToAction(
                    "Dashboard",
                    "Vendor");
            }


            

            if (roles.Contains("Customer"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Customer");
            }


           
            await _signInManager.SignOutAsync();

            TempData["LoginError"] =
                "No valid role is assigned to this account.";

            return RedirectToAction(
                nameof(Login));
        }


      

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter your email address.");

                return View();
            }

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["ForgotPasswordMessage"] =
                    "If an account with this email exists, a password reset link has been sent.";

                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);

            var resetLink =
                Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token = token
                    },
                    protocol: Request.Scheme);


            using (var smtp =
                   new SmtpClient(
                       "smtp.gmail.com",
                       587))
            {
                smtp.EnableSsl = true;

                smtp.Credentials =
                    new NetworkCredential(
                        "YOUR_EMAIL",
                        "YOUR_APP_PASSWORD");

                var mail =
                    new MailMessage
                    {
                        From =
                            new MailAddress(
                                "YOUR_EMAIL"),

                        Subject =
                            "Reset Your Password",

                        Body =
                            $@"
                        <h2>Password Reset</h2>

                        <p>
                            You requested to reset your password.
                        </p>

                        <p>
                            Click the button below to create
                            a new password:
                        </p>

                        <p>
                            <a href='{resetLink}'
                               style='background:#198754;
                                      color:white;
                                      padding:12px 20px;
                                      text-decoration:none;
                                      border-radius:5px;'>
                                Reset Password
                            </a>
                        </p>

                        <p>
                            If you did not request this,
                            you can ignore this email.
                        </p>
                        ",

                        IsBodyHtml = true
                    };

                mail.To.Add(email);

                await smtp.SendMailAsync(mail);
            }

            TempData["ForgotPasswordMessage"] =
                "Password reset link has been sent to your email.";

            return RedirectToAction(
                nameof(ForgotPassword));
        }


      
        [HttpGet]
        public IActionResult ResetPassword(
            string userId,
            string token)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(token))
            {
                return BadRequest(
                    "Invalid password reset link.");
            }

            var model =
                new ResetPasswordViewModel
                {
                    UserId = userId,
                    Token = token
                };

            return View(model);
        }


     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userManager.FindByIdAsync(
                    model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    model.Token,
                    model.Password);

            if (result.Succeeded)
            {
                TempData["ResetPasswordSuccess"] =
                    "Your password has been reset successfully.";

                return RedirectToAction(
                    nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View(model);
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Login",
                "Account");
        }


     

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    }
}
