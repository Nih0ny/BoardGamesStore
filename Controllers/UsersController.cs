using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;

namespace BoardGamesStore.Controllers;

public class UsersController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    // GET: get all users (only admin)
    // DELETE: delete user by id (only admin)
}