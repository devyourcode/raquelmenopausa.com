using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using NuGet.Packaging;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using RaquelMenopausa.Cms.Models.Dto;
using System.Collections.Generic;
using System.Linq.Dynamic.Core.Tokenizer;
using X.PagedList;
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

        public async Task<IActionResult> Index(int? page, string search, string status, string tag)
        {
            int pageSize = 10;
            int pageIndex = page ?? 1;

            int skip = (pageIndex - 1) * pageSize;
            int take = pageSize;


            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU"; // futuramente pegar do login
            var statusOptions = await _cmsService.GetArticleStatusOptionsAsync(token);

            ViewBag.StatusOptions = statusOptions;
            var tagsResponse = await _cmsService.GetTagsAsync(token);

            var allTags = new List<dynamic>();

            if (tagsResponse?.ArticleCategories != null)
            {
                allTags.AddRange(tagsResponse.ArticleCategories.Select(x => new
                {
                    x.Label,
                    x.Value
                }));
            }

            if (tagsResponse?.SymptomCategories != null)
            {
                allTags.AddRange(tagsResponse.SymptomCategories.Select(x => new
                {
                    x.Label,
                    x.Value
                }));
            }

            if (tagsResponse?.Solutions != null)
            {
                allTags.AddRange(tagsResponse.Solutions.Select(x => new
                {
                    x.Label,
                    x.Value
                }));
            }

            ViewBag.Tags = allTags;


            var result = await _cmsService.GetArtigosAsync(skip, take, search, status, tag, token: token);

            ViewBag.Indicators = await _cmsService.GetIndicators(token, search, status, tag);

            var pagedList = new StaticPagedList<ConteudoDto>(
            result, pageIndex, pageSize, result.Count
            );

            ViewBag.PageCount = pagedList.PageCount;

            return View(pagedList);
        }

        //[HttpGet]
        //[AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-listar")]
        //public async Task<IActionResult> Index(int? page) { int usuarioLogadoId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value); var usuarioLogado = _db.Usuarios.Include(u => u.Permissao).FirstOrDefault(u => u.Id == usuarioLogadoId); bool usuarioLogadoEhMaster = usuarioLogado.Email.EndsWith("@yourcode.com.br"); IQueryable<Usuario> query = _db.Usuarios.Where(o => o.Situacao).Include(o => o.Permissao); if (!usuarioLogadoEhMaster) { query = query.Where(o => !o.Email.EndsWith("@yourcode.com.br")); } var model = query.OrderBy(o => o.Nome).ToList();

        //    string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU";
        //    var statusOptions = await _cmsService.GetArticleStatusOptionsAsync(token);

        //    ViewBag.StatusOptions = statusOptions;


        //    int pageSize = 12; int pageIndex = page ?? 1; var emp = model.ToPagedList(pageIndex, pageSize); ViewBag.PageCount = emp.PageCount; return View(emp); }


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
        public async Task<IActionResult> Edit(string id)
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU";

            var conteudoedit = await _cmsService.GetArticleAsync(token, id);
            var tags = await _cmsService.GetTagsAsync(token);

            return PartialView("Edit", conteudoedit);
        }

        public async Task<IActionResult> GetArticleStatus()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjAwMDVlNWM1LWUxOTEtNGE0ZS04M2ZlLTNjMTFjMDg4MGM3OCIsInVzZXJuYW1lIjoiVXNlciAzIiwiZW1haWwiOiJ1c2VyM0BlbWFpbC5jb20iLCJ0eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzU5MzQyMjQxLCJleHAiOjE3NTk5NDcwNDF9.OTNA-NiIsOkdiy0fvI8AgzgGkoUF-7Q3CRwrAp1GWQU";
            var result = await _cmsService.GetArticleStatusOptionsAsync(token);
            return Ok(result);
        }

    }
}