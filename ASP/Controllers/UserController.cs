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
using ASP.Services.Time;


namespace ASP.Controllers
{
    public class UserController(IRandomService randomService, IKdfService kdfService, DataContext context, ILogger<UserController> logger, ITimeService timeService) : Controller
    {
        private readonly IRandomService _randomService = randomService;
        private readonly IKdfService _kdfService = kdfService;
        private readonly DataContext _dataContext = context;
        private readonly ILogger<UserController> _logger = logger;
        private readonly ITimeService _timeService = timeService;
        private readonly Regex _passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!?@$&*])[A-Za-z\d@$!%*?&]{12,}$");

        private UserAccess Authenticate()
        {
            // Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String authHeader = Request.Headers.Authorization.ToString();  // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            _logger.LogInformation(authHeader);
            if (String.IsNullOrEmpty(authHeader))
            {
                throw new Exception("Missing 'Authorization' header");
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                throw new Exception($"Authorization scheme error: '{authScheme}' only");
            }
            String credentials = authHeader[authScheme.Length..];  // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String decoded;
            try
            {
                decoded = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch (Exception ex)
            {
                _logger.LogError("SignIn: {ex}", ex.Message);
                throw new Exception($"Authorization credentials decode error");
            }
            String[] parts = decoded.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new Exception($"Authorization credentials decompose error");
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
                throw new Exception($"Authorization credentials rejected");
            }
            if (_kdfService.Dk(password, userAccess.Salt) != userAccess.Dk)
            {
                throw new Exception($"Authorization credentials rejected.");
            }
            return userAccess;
        }

        [HttpGet]
        public JsonResult SignIn()
        {
            UserAccess userAccess;
            try
            {
                userAccess = Authenticate();
            }catch(Exception ex)
            {
                return Json(new
                {
                    Status = 401,
                    Data = ex.Message
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


        [HttpGet]
        public JsonResult LogIn()
        {
            UserAccess userAccess;
            try
            {
                userAccess = Authenticate();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = 401,
                    Data = ex.Message
                });
            }

            // Токени.
            // цифрові "посвідчення", що несуть інформацію про користувача
            // За прямою наявністю інформації токени поділяють на 
            //  JWT - з наявністю інформації
            //  Bearer - лише з ідентифікатором токена
            AccessToken accessToken = new()
            {
                Jti = Guid.NewGuid().ToString(),
                Sub = userAccess.Id,
                Iat = _timeService.Timestamp().ToString(),
                //Exp = (_timeService.Timestamp() + (long)(1e5)).ToString(),
                Exp = (_timeService.Timestamp() + 30000).ToString(),
                Iss = nameof(ASP),
                Aud = userAccess.RoleId
            };
            return Json(new
            {
                Status = 200,
                Data = accessToken
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
