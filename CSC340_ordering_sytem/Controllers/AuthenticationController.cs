﻿using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using CSC340_ordering_sytem.DAL;
using CSC340_ordering_sytem.Utilities;
using CSC340_ordering_sytem.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace CSC340_ordering_sytem.Controllers
{
    public class AuthenticationController : Controller
    {
        OrderingSystemDbContext _db = new OrderingSystemDbContext();
        IAuthenticationManager Authentication => HttpContext.GetOwinContext().Authentication;

        // GET: Authentication
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Login(AuthenticationLoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var hashedPassword = SHA256Hasher.Create(viewModel.Password);
            var user = Models.User.FindUserByEmailAndPassword((string)viewModel.Email, hashedPassword, _db);

            if (user != null)
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, viewModel.Email),
                    },
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name, ClaimTypes.Role
                    );

                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));

                Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = viewModel.IsPersistant
                }, identity);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email/password combination.");

            return View(viewModel);
        }


        [HttpGet]
        [Authorize]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

    }
}