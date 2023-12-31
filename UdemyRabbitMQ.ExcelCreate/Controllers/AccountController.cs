﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace UdemyRabbitMQ.ExcelCreate.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string Email, string Password)
    {
        var hasUser = await userManager.FindByEmailAsync(Email);

        if(hasUser == null)
        {
            return View();
        }

        var signInResult = await signInManager.PasswordSignInAsync(hasUser, Password, true, false);

        if(!signInResult.Succeeded)
        {
            return View();
        }



        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
