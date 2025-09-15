using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Yourcode.Core.Cms.Models;

namespace Yourcode.Core.Cms.Controllers
{
    public class HomeController2 : Controller
    {
        private readonly ILogger<HomeController2> _logger;
        private readonly Context _context;

        public HomeController2(ILogger<HomeController2> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var query = await _context.Usuarios.Where(x => (bool)x.Ativo).ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
