using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using Microsoft.SqlServer.Server;

namespace GuideMe.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IDataProtector _protector;

        public AuthenticationController(GuideMeContext context,DataSecurity key, IDataProtectionProvider _provider)
        {
            _context = context;
            _protector = _provider.CreateProtector(key.key);
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult LogIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(UserEdit e)
        {
            var user = _context.Users.FirstOrDefault(z => z.Email.ToLower() == e.Email.ToLower());
            if(user != null && (_protector.Unprotect(user.Password) == e.Password))
            {
                user.LastLogin = DateTime.UtcNow;
                _context.SaveChanges();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Name", user.Name)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Please enter correct credentials.");
            return View(e);
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(UserEdit forgetpassword)
        {
            if(forgetpassword.Email != null)
            {
                Random random = new Random();
                HttpContext.Session.SetString("token", random.Next(99999).ToString());
                var token = HttpContext.Session.GetString("token");

                var user= _context.Users.Where(e=>e.Email == forgetpassword.Email).FirstOrDefault();
                if (user != null) 
                {
                    try
                    {
                        SmtpClient smtp = new SmtpClient()
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential("np05cp4a220079@iic.edu.np", "wyxj yjtj fawe wqss"),
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network
                        };

                        MailMessage mail = new()
                        {
                            From = new MailAddress("np05cp4a220079@iic.edu.np"),
                            Subject = "Plz enter the Token",

                            //$"dkdk:{}" [string interposition]
                            Body = $"<a href='' style= 'background-color: green; color:white; padding:10px; width:200px; border-radius:4px; '>Reset Password </a> <p> Forget Password Token:</p>{token}",
                            IsBodyHtml = true
                        };
                        HttpContext.Session.SetString("email", user.Email);
                        mail.To.Add(user.Email);
                        smtp.Send(mail);
                        return RedirectToAction("VerifyToken");
                    }

                    catch (Exception ex)
                    {
                        return Json(ex);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Entered Email is not registered");
                    return View(forgetpassword);
                }
            }
            return View(forgetpassword);
        }

        [HttpGet]
        public IActionResult VerifyToken()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyToken(UserEdit u)
        {
           var verifytoken = HttpContext.Session.GetString("token");

           if (verifytoken == u.EmailToken)
           {
              var emailtoken = _protector.Protect(u.EmailToken!);
               return RedirectToAction("ResetPassword", new {id = emailtoken});
           }
           else
           {
                return Json("failed");
           }
        }

        [HttpGet]
        public IActionResult ResetPassword(string id)
        {
            try
            {
                var resettoken = HttpContext.Session.GetString("token");

                var emailtoken = _protector.Unprotect(id);

                if (resettoken == emailtoken)
                {
                    return View(new ChangePassword { EmailToken = id });
                }
                else
                {
                    return RedirectToAction("ForgetPassword");
                }

            }
            catch
            {
                return RedirectToAction("ForgetPassword");
            }
        }

        [HttpPost]
        public IActionResult ResetPassword(ChangePassword cp)
        {
           
            var token = HttpContext.Session.GetString("token");
            var emailtoken = _protector.Unprotect(cp.EmailToken!);

            if (token == emailtoken)
            {
                var emailaddress = HttpContext.Session.GetString("email");
                var ue = _context.Users.Where(a => a.Email == emailaddress).FirstOrDefault();

                if (ue != null)
                {
                    if (cp.NewPassword == cp.ConfirmPassword)
                    {
                        ue.Password = _protector.Protect(cp.NewPassword);
                        _context.Users.Update(ue);
                        _context.SaveChanges();
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Confirm Password does not matched.");
                        return View(cp);
                    }
                }

                else
                {
                    ModelState.AddModelError("", "Email failed");
                    return View(cp);
                }
            }
            else
            {
                return RedirectToAction("ForgotPassword");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePassword cp)
        {
              if (ModelState.IsValid)
    {
        var userId = Convert.ToInt32(User.Identity!.Name); 
        var user = _context.Users.SingleOrDefault(h => h.UserId == userId);
        
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(cp);
        }

        
        if (_protector.Unprotect(user.Password) != cp.CurrentPassword)
        {
            ModelState.AddModelError("", "The current password is incorrect.");
            return View(cp);
        }

        
        if (cp.NewPassword == cp.ConfirmPassword)
        {
            user.Password = _protector.Protect(cp.NewPassword); 
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("LogIn", "Authentication");
        }
        else
        {
            ModelState.AddModelError("", "New password and confirmation do not match.");
            return View(cp);
        }
    }

    ModelState.AddModelError("", "Failed to change password.");
    return View(cp);
        }

        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Static");
        }
    }
}
