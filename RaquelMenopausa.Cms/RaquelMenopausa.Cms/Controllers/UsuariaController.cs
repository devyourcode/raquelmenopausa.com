using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using RaquelMenopausa.Cms.Models.Dto;
using X.PagedList;
using X.PagedList.Extensions;
using Yourcode.Core.Utilities;

namespace RaquelMenopausa.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria")]
    public class UsuariaController : CustomController
    {
        private readonly ILogger<UsuariaController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;
        private readonly CmsService _cmsService;

        public UsuariaController(ILogger<UsuariaController> logger, Context db, IWebHostEnvironment env, CmsService cmsService)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _cmsService = cmsService;
        }



        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria-listar")]
        public async Task<IActionResult> Index(int? page, string search, string status, string periodo)
        {
            int pageSize = 30;
            int pageIndex = page ?? 1;

            int skip = (pageIndex - 1) * pageSize;
            int take = pageSize;

            DateTime initialDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime finalDate = initialDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrEmpty(periodo) && periodo.Contains("|"))
            {
                var parts = periodo.Split('|');
                if (DateTime.TryParse(parts[0], out var ini)) initialDate = ini;
                if (DateTime.TryParse(parts[1], out var fim)) finalDate = fim;
            }

            var token = await _cmsService.GetValidTokenAsync();

            var result = await _cmsService.GetUsuariasAsync(skip, take, search, status, token: token, initialDate: initialDate, finalDate: finalDate);

            ViewBag.Indicators = await _cmsService.GetIndicatorsUsers(token, search, status, initialDate: initialDate, finalDate: finalDate);

            var pagedList = new StaticPagedList<UsuariaDto>(
                result.Items, pageIndex, pageSize, result.TotalCount
            );


            ViewBag.PageCount = pagedList.PageCount;

            return View(pagedList);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria-editar")]
        public async Task<IActionResult> Edit(string id)
        {
            var token = await _cmsService.GetValidTokenAsync();

            var usuaria = await _cmsService.GetUsuariaAsync(token, id);

            return PartialView("Edit", usuaria);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IFormCollection form, IFormFile profileImageUpload)
        {
            try
            {
                var token = await _cmsService.GetValidTokenAsync();

                var userId = form["user_id"];
                var hash = form["hash"];
                var user = form["Nome"];
                var aliasName = form["Chamada"];
                var email = form["Email"];
                var emailVerified = form["EmailVerified"].Count > 0; 
                var status = form["status"];
                var admin = form["admin"].ToString().ToLower() == "true";
                var changedImage = form["changedProfileImage"] == "true" ||
                           (profileImageUpload != null && profileImageUpload.Length > 0);


                bool isSuspended = status == "Deleted";

                await _cmsService.UpdateUsuariaAsync(
                    userId,
                    hash,
                    user,
                    aliasName,
                    email,
                    emailVerified,
                    isSuspended,
                    admin,
                    profileImageUpload,
                    changedImage,
                    token
                );

                TempData["SUCESSO"] = "Usuária atualizada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERRO"] = $"Erro ao atualizar usuária: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public async Task<IActionResult> DownloadCsv(string search = null, string status = null, string periodo = null)
        {
            try
            {
                DateTime initialDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime finalDate = initialDate.AddMonths(1).AddDays(-1);

                if (!string.IsNullOrEmpty(periodo) && periodo.Contains("|"))
                {
                    var parts = periodo.Split('|');
                    if (DateTime.TryParse(parts[0], out var ini)) initialDate = ini;
                    if (DateTime.TryParse(parts[1], out var fim)) finalDate = fim;
                }

                var token = await _cmsService.GetValidTokenAsync();

                var csvBytes = await _cmsService.GetUsersCsvAsync(token, search, status,
                    initialDate: initialDate, finalDate: finalDate);

                return File(csvBytes, "text/csv", "Usuarias.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao baixar CSV: {ex.Message}");
            }
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria-suspender-conta")]
        public async Task<IActionResult> Suspend(string id)
        {
            var token = await _cmsService.GetValidTokenAsync();

            var usuaria = await _cmsService.SuspendAccountAsync(token, id);

            TempData["SUCESSO"] = "Conta suspensa com sucesso!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria-ativar-conta")]
        public async Task<IActionResult> Activate(string id)
        {
            var token = await _cmsService.GetValidTokenAsync();

            var usuaria = await _cmsService.ActivateAccountAsync(token, id);

            TempData["SUCESSO"] = "Conta ativada com sucesso!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EnvioEmail(IFormCollection form)
        {
            try
            {
                var token = await _cmsService.GetValidTokenAsync();

                var id = form["UserId"];
                var subject = form["subject"];
                var content = $"<p>{form["content"]}</p>";


                await _cmsService.EnvioEmailAsync(id, subject, content,token);

                TempData["SUCESSO"] = "E-mail enviado com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERRO"] = $"Erro ao enviar e-mail: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetSenha(IFormCollection form)
        {
            try
            {
                var token = await _cmsService.GetValidTokenAsync();

                var id = form["UserId"];
                var newPassword = form["newPassword"];


                await _cmsService.ResetSenhaAsync(id, newPassword, token);

                TempData["SUCESSO"] = "Senha atualizada com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERRO"] = $"Erro ao atualizar a senha: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddObservation(IFormCollection form)
        {
            try
            {
                var token = await _cmsService.GetValidTokenAsync();

                var id = form["UserId"];
                var observations = form["observations"];


                await _cmsService.AddObservationAsync(id, observations, token);

                TempData["SUCESSO"] = "Observação adicionada com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERRO"] = $"Erro ao adicionar observação: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

    }
}