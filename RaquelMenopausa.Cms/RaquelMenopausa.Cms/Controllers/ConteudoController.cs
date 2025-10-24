using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
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
using static RaquelMenopausa.Cms.Helpers.CmsGeneralHelper;

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
                    Label = TagTranslator.Translate(x.Label),
                    x.Value
                }));
            }

            if (tagsResponse?.SymptomCategories != null)
            {
                allTags.AddRange(tagsResponse.SymptomCategories.Select(x => new
                {
                    Label = TagTranslator.Translate(x.Label),
                    x.Value
                }));
            }

            if (tagsResponse?.Solutions != null)
            {
                allTags.AddRange(tagsResponse.Solutions.Select(x => new
                {
                    Label = TagTranslator.Translate(x.Label),
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

            if (tags.ArticleCategories != null)
            {
                tags.ArticleCategories.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

            if (tags.SymptomCategories != null)
            {
                tags.SymptomCategories.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

            if (tags.Solutions != null)
            {
                tags.Solutions.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

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
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-editar")]
        public async Task<IActionResult> Edit(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var artigo = await _cmsService.GetArticleAsync(token, id);
            var tags = await _cmsService.GetTagsAsync(token);

            if (tags.ArticleCategories != null)
            {
                tags.ArticleCategories.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

            if (tags.SymptomCategories != null)
            {
                tags.SymptomCategories.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

            if (tags.Solutions != null)
            {
                tags.Solutions.ForEach(x => x.Label = TagTranslator.Translate(x.Label));
            }

            foreach (var cat in tags.ArticleCategories)
            {
                cat.Selected = artigo.ArticleArticleCategories
                    .Any(a => a.ArticleCategoryId == int.Parse(cat.Value));
            }

            foreach (var sym in tags.SymptomCategories)
            {
                sym.Selected = artigo.ArticleSymptomCategories
                    .Any(a => a.SymptomCategoryId == int.Parse(sym.Value));
            }

            foreach (var sol in tags.Solutions)
            {
                sol.Selected = artigo.ArticleSolutions
                    .Any(a => a.SolutionId == int.Parse(sol.Value));
            }

            ViewBag.Tags = tags;
            return PartialView("Edit", artigo);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IFormCollection form, IFormFile arquivoImagem, IFormFile arquivoMidia)
        {
            try
            {
                var token = _context.Configs
                    .Where(o => o.Chave == "token" && o.Situacao)
                    .Select(o => o.Valor)
                    .FirstOrDefault();

                var id = form["article_id"];
                var hash = form["hash"];

                var titulo = form["Titulo"];               
                var assunto = form["Subject"];            
                var introducao = form["Introducao"];      
                var texto = form["Text"];                 
                var cor = form["Cor"];                  
                var referencias = form["References"];    

                var acao = form["acao"];
                var status = acao == "rascunho" ? "DRAFT" : "PUBLISHED";

                var categorias = form["Categorias"].Select(int.Parse).ToList();
                var sintomas = form["Sintomas"].Select(int.Parse).ToList();
                var solucoes = form["Solucoes"].Select(int.Parse).ToList();

                bool changedImage = form["ChangedImage"] == "true";
                bool changedAudioVideo = form["ChangedAudioVideo"] == "true";

                await _cmsService.UpdateArticleAsync(
                    id, hash, titulo, introducao, texto, referencias, cor, status, assunto,
                    categorias, sintomas, solucoes,
                    arquivoImagem, arquivoMidia,
                    changedImage, changedAudioVideo, token
                );

                TempData["SuccessMessage"] = status == "DRAFT"
                    ? "Artigo salvo como rascunho!"
                    : "Artigo atualizado com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar artigo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-deletar")]
        public async Task<IActionResult> Delete(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var conteudoedit = await _cmsService.DeleteAsync(token, id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-conteudo-publicar")]
        public async Task<IActionResult> Publish(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var conteudoedit = await _cmsService.PublishAsync(token, id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCsv(string search = null, string status = null, string tag = null)
        {
            try
            {
                var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

                var csvBytes = await _cmsService.GetArticlesCsvAsync(token, search, status, tag);

                return File(csvBytes, "text/csv", "conteudos.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao baixar CSV: {ex.Message}");
            }
        }


        public async Task<IActionResult> GetArticleStatus()
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            var result = await _cmsService.GetArticleStatusOptionsAsync(token);
            return Ok(result);
        }

    }
}