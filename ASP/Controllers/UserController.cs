using ASP.Data.Entities;
using ASP.Data;
using ASP.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ASP.Services.Random;
using ASP.Services.Kdf;

namespace ASP.Controllers
{
    public class UserController(IRandomService randomService, IKdfService kdfService, DataContext context) : Controller
    {
        private readonly IRandomService _randomService = randomService;
        private readonly IKdfService _kdfService = kdfService;
        private readonly DataContext _dataContext = context;
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
            if (String.IsNullOrEmpty(model.UserName))
            {
                errors[nameof(model.UserName)] = "Name must not be empty!";
            }
            if (String.IsNullOrEmpty(model.UserEmail))
            {
                errors[nameof(model.UserEmail)] = "Email must not be empty!";
            }
            if (String.IsNullOrEmpty(model.UserLogin))
            {
                errors[nameof(model.UserLogin)] = "Login must  not be empty!";
            }
            else
            {
                if (model.UserLogin.Contains(":"))
                {
                    errors[nameof(model.UserLogin)] = "Login mustn't  contains ':'!";
                }
            }
            #endregion
            if(errors.Count == 0)
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
                _dataContext.Users.Add(user);
                _dataContext.UserAccesses.Add(userAccess);
                _dataContext.SaveChanges();

            }
            return errors;
        }
    }
}
