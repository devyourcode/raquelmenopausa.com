using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Yourcode.Core.Cms.Models;

namespace Yourcode.Core.Cms.Controllers
{
    //[AuditoriaAtribute]
    //[AuthorizeUser(Module = "~/home")]
    public class NoPermitionController : CustomController
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly Context _context;

        public NoPermitionController(ILogger<DashboardController> logger, Context context) 
            : base(logger, context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Admin/Dashboard/
        public IActionResult Index()
        {
            return View();
        }

    }
}
