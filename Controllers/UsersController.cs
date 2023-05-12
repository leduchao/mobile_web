﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobileWeb.Data;
using MobileWeb.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using MobileWeb.Services;

namespace MobileWeb.Controllers
{
  public class UsersController : Controller
  {
    private readonly MobileWebContext _context;
    private readonly CartService _cartService;

    public UsersController(MobileWebContext context, CartService cartService)
    {
      _context = context;
      _cartService = cartService;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
      return _context.User != null ?
                  View(await _context.User.ToListAsync()) :
                  Problem("Entity set 'MobileWebContext.User'  is null.");
    }

    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
      if (ModelState.IsValid)
      {

        var user = _context.User?.Where(u => u.Name == username && u.Password == password)
                               .FirstOrDefault();
        if (user == null || _context.User == null)
        {
          return View();
        }

        if (user.Role == "admin")
          return RedirectToAction("Index", "Admin");

        var claims = new List<Claim>
        {
          new Claim(ClaimTypes.Name, user.Name!),
          new Claim(ClaimTypes.Role, user.Role!),
          //new Claim(ClaimTypes., user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));



        return RedirectToAction("Index", "Home");
      }
      else
      {
        return View();
      }
    }

    public IActionResult Logout()
    {
      HttpContext.SignOutAsync();
      _cartService.ClearCart();
      return View(nameof(Login));
    }

    public IActionResult Signup()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignupAccount([Bind("Id,Name,Email,Password,Role")] User user)
    {
      if (ModelState.IsValid)
      {
        var checkEmail = _context.User?.FirstOrDefault(u => u.Email == user.Email || u.Name == user.Name);
        if (checkEmail == null)
        {
          _context.Add(user);
          await _context.SaveChangesAsync();
          return RedirectToAction(nameof(Login));
        }
        else
        {
          ViewBag.Error = "Email hoặc tên người dùng đã tồn tại!";
          return View(nameof(Signup));
        }
      }
      return View(nameof(Signup));
    }

    //public IActionResult Cart()
    //{
    //  /*if (id == null || _context.User == null)
    //  {
    //    return NotFound();
    //  }

    //  var user = _context.User.FirstOrDefault(m => m.Id == id);
    //  if (user == null)
    //  {
    //    return View("UserInvalid");
    //  }
    //  return View(user);*/
    //  return View();
    //}

    public IActionResult ShowProfile(string username)
    {
      //tim user theo ten
      var user = _context.User!.FirstOrDefault(u => u.Name == username);

      //tra ve user phu hop
      return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([FromQuery] string username, [Bind("Id,Name,Email,Address,Firstname,Lastname,Birthday,Phone")] User user)
    {

      if (username != user.Name)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(user);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!UserExists(user.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction("Index", "Shop");
      }
      return View("ShowProfile", user);
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null || _context.User == null)
      {
        return NotFound();
      }

      var user = await _context.User
          .FirstOrDefaultAsync(m => m.Id == id);
      if (user == null)
      {
        return NotFound();
      }

      return View(user);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Users/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id", "Name", "Email", "Password", "Address", "AvatarUrl", "Role")] User user)
    {
      if (ModelState.IsValid)
      {
        _context.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(user);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null || _context.User == null)
      {
        return NotFound();
      }

      var user = await _context.User.FindAsync(id);
      if (user == null)
      {
        return NotFound();
      }
      return View(user);
    }

    // POST: Users/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,Address,AvatarUrl,Role")] User user)
    {
      if (id != user.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(user);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!UserExists(user.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }
      return View(user);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null || _context.User == null)
      {
        return NotFound();
      }

      var user = await _context.User
          .FirstOrDefaultAsync(m => m.Id == id);
      if (user == null)
      {
        return NotFound();
      }

      return View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      if (_context.User == null)
      {
        return Problem("Entity set 'MobileWebContext.User'  is null.");
      }
      var user = await _context.User.FindAsync(id);
      if (user != null)
      {
        _context.User.Remove(user);
      }

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id)
    {
      return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
