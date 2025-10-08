using DocumentFormat.OpenXml.Bibliography;
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

            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

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

            ViewBag.Indicators = await _cmsService.GetIndicators(token);

            var pagedList = new StaticPagedList<ConteudoDto>(
                result.Items, pageIndex, pageSize, result.TotalCount
            );


            ViewBag.PageCount = pagedList.PageCount;

            return View(pagedList);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-criar")]
        public async Task<IActionResult> Create()
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            var tags = await _cmsService.GetTagsAsync(token);

            return PartialView("Create", tags);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection form, IFormFile arquivoImagem, IFormFile arquivoMidia)
        {
            try
            {
                var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

                var titulo = form["Titulo"];
                var assunto = form["Assunto"];
                var introducao = form["Introducao"];
                var texto = form["Texto"];
                var cor = form["CorConteudo"];
                var referencias = form["Referencias"];

                var acao = form["acao"];
                var status = acao == "rascunho" ? "DRAFT" : "PUBLISHED";

                var categorias = form["Categorias"].Select(int.Parse).ToList();
                var sintomas = form["Sintomas"].Select(int.Parse).ToList();
                var solucoes = form["Solucoes"].Select(int.Parse).ToList();

                await _cmsService.CreateArtigoAsync(
                    titulo, introducao, texto, referencias, cor, status, assunto,
                    categorias, sintomas, solucoes, arquivoImagem, arquivoMidia, token
                );

                TempData["SuccessMessage"] = status == "draft"
                    ? "Artigo salvo como rascunho!"
                    : "Publicação criada com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar artigo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-editar")]
        public async Task<IActionResult> Edit(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var artigo = await _cmsService.GetArticleAsync(token, id);
            var tags = await _cmsService.GetTagsAsync(token);

            artigo.ArticleArticleCategories = tags.ArticleCategories.Select(cat => new ArticleCategoryLinkDto
            {
                ArticleId = artigo.ArticleId,
                ArticleCategoryId = int.Parse(cat.Value),
                ArticleCategory = new ArticleCategoryDetailDto
                {
                    ArticleCategoryId = int.Parse(cat.Value),
                    Name = cat.Label,
                    Selected = artigo.ArticleArticleCategories.Any(a => a.ArticleCategoryId == int.Parse(cat.Value))
                }
            }).ToList();

            artigo.ArticleSymptomCategories = tags.SymptomCategories.Select(sym => new ArticleSymptomCategoryLinkDto
            {
                ArticleId = artigo.ArticleId,
                SymptomCategoryId = int.Parse(sym.Value),
                SymptomCategory = new SymptomCategoryDetailDto
                {
                    SymptomCategoryId = int.Parse(sym.Value),
                    Description = sym.Label,
                    Selected = artigo.ArticleSymptomCategories.Any(a => a.SymptomCategoryId == int.Parse(sym.Value))
                }
            }).ToList();

            artigo.ArticleSolutions = tags.Solutions.Select(sol => new ArticleSolutionLinkDto
            {
                ArticleId = artigo.ArticleId,
                SolutionId = int.Parse(sol.Value),
                Solution = new SolutionDetailDto
                {
                    SolutionId = int.Parse(sol.Value),
                    Name = sol.Label,
                    Selected = artigo.ArticleSolutions.Any(a => a.SolutionId == int.Parse(sol.Value))
                }
            }).ToList();

            return PartialView("Edit", artigo);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-deletar")]
        public async Task<IActionResult> Delete(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var conteudoedit = await _cmsService.DeleteAsync(token, id);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetArticleStatus()
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            var result = await _cmsService.GetArticleStatusOptionsAsync(token);
            return Ok(result);
        }

    }
}