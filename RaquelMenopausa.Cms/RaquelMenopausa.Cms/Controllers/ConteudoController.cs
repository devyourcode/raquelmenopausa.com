using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using RaquelMenopausa.Cms.Models.Dto;
using X.PagedList.Extensions;
using Yourcode.Core.Utilities;

namespace RaquelMenopausa.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo")]
    public class ConteudoController : CustomController
    {
        private readonly ILogger<ConteudoController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;
        private readonly CmsService _cmsService;

        public ConteudoController(ILogger<ConteudoController> logger, Context db, IWebHostEnvironment env, CmsService cmsService)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _cmsService = cmsService;
        }



        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-listar")]
        public IActionResult Index(int? page)
        {
            int usuarioLogadoId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var usuarioLogado = _db.Usuarios
                .Include(u => u.Permissao)
                .FirstOrDefault(u => u.Id == usuarioLogadoId);

            bool usuarioLogadoEhMaster = usuarioLogado.Email.EndsWith("@yourcode.com.br");

            IQueryable<Usuario> query = _db.Usuarios
                .Where(o => o.Situacao)
                .Include(o => o.Permissao);

            if (!usuarioLogadoEhMaster)
            {
                query = query.Where(o => !o.Email.EndsWith("@yourcode.com.br"));
            }

            var model = query
                .OrderBy(o => o.Nome)
                .ToList();

            int pageSize = 12;
            int pageIndex = page ?? 1;

            var emp = model.ToPagedList(pageIndex, pageSize);
            ViewBag.PageCount = emp.PageCount;

            return View(emp);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-criar")]
        public async Task<IActionResult> Create()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU"; // futuramente pegar do login
            var tags = await _cmsService.GetTagsAsync(token);

            return PartialView("Create", tags);
        }

        

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-editar")]
        public IActionResult Edit()
        {
            return PartialView("Edit");
        }

        public async Task<IActionResult> GetArticleStatus()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU";
            var result = await _cmsService.GetArticleStatusOptionsAsync(token);
            return Ok(result);
        }

    }
}