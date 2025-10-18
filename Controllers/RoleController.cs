using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        // Тепер використовуємо RoleManager для вашої кастомної моделі Role
        private readonly RoleManager<Role> _roleManager;

        public RoleController(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        // GET: api/roles/5
        // ID тепер має тип int
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            // FindByIdAsync приймає ID у вигляді рядка, тому конвертуємо int
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                return NotFound($"Роль з ID '{id}' не знайдено.");
            }

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> Create(RoleDto roleDto)
        {
            if (await _roleManager.RoleExistsAsync(roleDto.Name))
            {
                return BadRequest("Роль з такою назвою вже існує.");
            }

            // Створюємо екземпляр вашої кастомної ролі
            var newRole = new Role { Name = roleDto.Name };

            // Нормалізуємо ім'я для коректного пошуку
            newRole.NormalizedName = _roleManager.NormalizeKey(newRole.Name);

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = newRole.Id }, newRole);
            }

            return BadRequest(result.Errors);
        }

        // PUT: api/roles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, RoleDto roleDto)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                return NotFound($"Роль з ID '{id}' не знайдено.");
            }

            // Оновлюємо стандартну властивість Name
            role.Name = roleDto.Name;
            role.NormalizedName = _roleManager.NormalizeKey(role.Name);

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/roles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                return NotFound($"Роль з ID '{id}' не знайдено.");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }
    }

    // --- DTO (Data Transfer Object) ---
    // Можна використовувати один DTO для створення та оновлення
    public class RoleDto
    {
        [Required]
        public required string Name { get; set; }
    }

}
