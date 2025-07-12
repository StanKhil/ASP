using ASP.Data.Entities;
using ASP.Data;
using ASP.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ASP.Services.Random;
using ASP.Services.Kdf;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;


namespace ASP.Controllers
{
    public class UserController(IRandomService randomService, IKdfService kdfService, DataContext context, ILogger<UserController> logger) : Controller
    {
        private readonly IRandomService _randomService = randomService;
        private readonly IKdfService _kdfService = kdfService;
        private readonly DataContext _dataContext = context;
        private readonly ILogger<UserController> _logger = logger;
        private readonly Regex _passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!?@$&*])[A-Za-z\d@$!%*?&]{12,}$");

        [HttpGet]
        public JsonResult SignIn()
        {
            String authHeader = Request.Headers.Authorization.ToString();
            if (String.IsNullOrEmpty(authHeader))
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Missing 'Authorization' header"
                });
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                return Json(new
                {
                    Status = 401,
                    Data = $"Authorization scheme error: '{authScheme}' only"
                });
            }
            String credentials = authHeader[authScheme.Length..];
            String decoded;
            try
            {
                decoded = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(credentials));
            }
            catch (Exception ex)
            {
                _logger.LogError("Sign In: {ex}", ex.Message);
                return Json(new
                {
                    Status = 401,
                    Data = "Authorization credentials decode error"
                });
            }
            String[] parts = decoded.Split(':', 2);
            if(parts.Length != 2)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Authorization credentials decompose error"
                });
            }
            String login = parts[0];
            String password = parts[1];
            var userAccess = _dataContext
                .UserAccesses
                .AsNoTracking()
                .Include(ua => ua.UserData)
                .Include(ua => ua.UserRole)
                .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Authorization credentials rejected"
                });
            }
            if(_kdfService.Dk(password, userAccess.Salt) != userAccess.Dk)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Authorization credentials rejected."
                });
            }

            HttpContext.Session.SetString("userAccess",
                JsonSerializer.Serialize(userAccess));
            return Json(new
            {
                Status = 200,
                Data = "OK"
            });

        }

        public ViewResult SignUp()
        {
            UserSignupPageModel model = new();
            if (HttpContext.Session.Keys.Contains("UserSignupFormModel"))
            {
                model.FormModel = JsonSerializer.
                    Deserialize<UserSignupFormModel>(
                        HttpContext.Session.
                        GetString(
                            "UserSignupFormModel")!);
                model.FormErrors = ProcessSignupData(model.FormModel!);

                HttpContext.Session.Remove("UserSignupFormModel");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<RedirectToActionResult> Register(UserSignupFormModel model)
        {
            HttpContext.Session.SetString(
                "UserSignupFormModel", JsonSerializer.Serialize(model)
            );
            return RedirectToAction(nameof(SignUp));
        }

        private Dictionary<String,String> ProcessSignupData(UserSignupFormModel model)
        {
            Dictionary<String, String> errors = [];
            #region Validation
            if (String.IsNullOrEmpty(model.UserName)) errors[nameof(model.UserName)] = "Name must not be empty!";
            if (String.IsNullOrEmpty(model.UserEmail)) errors[nameof(model.UserEmail)] = "Email must not be empty!";
            if (String.IsNullOrEmpty(model.UserLogin)) errors[nameof(model.UserLogin)] = "Login must not be empty!";
            else
            {
                if (model.UserLogin.Contains(":")) errors[nameof(model.UserLogin)] = "Login must not contain ':'!";
                else
                {
                    if (_dataContext.UserAccesses.Any(ua => ua.Login == model.UserLogin)) errors[nameof(model.UserLogin)] = "Login already in use";
                }
            }
            //if (string.IsNullOrEmpty(model.UserPassword))
            //{
            //    errors[nameof(model.UserPassword)] = "Password cannot be empty";
            //    errors[nameof(model.UserRepeat)] = "Invalid original password";
            //}
            //else
            //{
            //    if (!_passwordRegex.IsMatch(model.UserPassword))
            //    {
            //        errors[nameof(model.UserPassword)] = "Password must be at least 12 characters long and contain lower, upper case letters, at least one number and at least one special character";
            //        errors[nameof(model.UserRepeat)] = "Invalid original password";
            //    }
            //    else
            //    {
            //        if (model.UserRepeat != model.UserPassword) errors[nameof(model.UserRepeat)] = "Passwords must match";
            //    }
            //}
            //if (!model.Agree) errors[nameof(model.Agree)] = "You must agree with policies!";
            //MyS3cur3P@ssw0rd! user2


            #endregion

            if (errors.Count == 0)
            {
                Guid userId = Guid.NewGuid();

                UserData user = new()
                {
                    Id = userId,
                    Name = model.UserName,
                    Email = model.UserEmail,
                    Birthdate = model.Birthdate,
                    RegisteredAt = DateTime.Now,

                };

                String salt = _randomService.Otp(12);
                UserAccess userAccess = new()

                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Login = model.UserLogin,
                    Salt = salt,
                    Dk = _kdfService.Dk(model.UserPassword, salt),
                    RoleId = "SelfRegistered"
                };

                _dataContext.Database.BeginTransaction();

                _dataContext.Users.Add(user);
                _dataContext.UserAccesses.Add(userAccess);

                try
                {
                    _dataContext.SaveChanges();
                    _dataContext.Database.CommitTransaction();
                }catch(Exception ex)
                {
                    _logger.LogError("ProcessSignupData: {ex}", ex.Message);
                    _dataContext.Database.RollbackTransaction();
                    errors["500"] = "Problem with saving data";
                }
                

            }
            return errors;
        }
    }
}
