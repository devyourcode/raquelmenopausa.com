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
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario")]
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
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-listar")]
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

            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var result = await _cmsService.GetUsuariasAsync(skip, take, search, status, token: token, initialDate: initialDate, finalDate: finalDate);

            ViewBag.Indicators = await _cmsService.GetIndicatorsUsers(token, search, status, initialDate: initialDate, finalDate: finalDate);

            var pagedList = new StaticPagedList<UsuariaDto>(
                result.Items, pageIndex, pageSize, result.TotalCount
            );


            ViewBag.PageCount = pagedList.PageCount;

            return View(pagedList);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-criar")]
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            Usuario usuario = new Usuario();

            try
            {

                if (ModelState.IsValid)
                {
                    var senha_sem_criptografia = form["txtSenha"];

                    usuario.Nome = form["txtNome"].ToString();
                    usuario.Email = form["txtEmail"].ToString();
                    usuario.Cargo = form["txtCargo"].ToString();
                    usuario.PermissaoId = int.Parse(form["txtPermissao"].ToString());
                    usuario.Senha = CryptoHelper.HashMd5(form["txtSenha"].ToString());
                    usuario.Ativo = true;
                    usuario.DataInc = DateTime.Now;
                    usuario.Situacao = true;
                    _db.Usuarios.Add(usuario);
                    await _db.SaveChangesAsync();

                    var EnviarEmail = int.Parse(form["EnviarEmail"].ToString());

                    if (EnviarEmail == 1)
                    {
                        var nome_site = _db.Configs.Where(o => o.Chave == "nome-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
                        var admin_site = _db.Configs.Where(o => o.Chave == "admin-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

                        var mensagemHTML = "";
                        mensagemHTML = "Seu usuário foi cadastrado no administrador do site " + nome_site + ".<br />Segue abaixo os dados para acesso:" +
                           "<br><br>Link: <a target='_blank' href='" + admin_site + "'>" + admin_site + "</a>" +
                           "<br>Login: " + usuario.Email +
                           "<br>Senha: " + senha_sem_criptografia +
                           "<br><br>Atenciosamente,<br><b>Suporte Técnico - " + nome_site + "</b>";


                        bool enviado = await CmsNotificationHelper.SendEmail(
                            clienteId: 1148,
                            projetoId: 345,
                            nome: "YourCode",
                            remetente: "noreply@yourcode.com.br",
                            destinatario: usuario.Email,
                            assunto: "Teste",
                            mensagem: mensagemHTML
                        );
                    }

                    TempData["SUCESSO"] = "Usuário criado com sucesso!";

                    #region LOG
                    var userName = User.Identity?.Name ?? "Usuário desconhecido";
                    var userId = User.FindFirst("sub")?.Value ?? "0";

                    LogAuditoria.Action(userName, Convert.ToInt32(userId), "adicionou", "usuario", usuario.Id, usuario.Nome);
                    #endregion LOG


                    return RedirectToAction(nameof(Index));

                }
                var listAtivo = new List<SelectListItem>();
                listAtivo.Add(new SelectListItem() { Value = "true", Text = "Ativo" });
                listAtivo.Add(new SelectListItem() { Value = "false", Text = "Inativo" });
                ViewBag.ListaAtivo = new SelectList(listAtivo, "Value", "Text", usuario.Ativo);
                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar popup");

                TempData["ERRO"] = $"Ocorreu um erro: {ex.Message}";

                #region LOG
                var userName = User.Identity?.Name ?? "Usuário desconhecido";
                var userId = User.FindFirst("sub")?.Value ?? "0";

                LogAuditoria.Action(userName, Convert.ToInt32(userId), "Erro ao adicionar", "usuario", usuario.Id, usuario.Nome);
                #endregion LOG

                return View(usuario);

            }
        }

        private void PrepararListaAtivo(bool ativo)
        {
            var listAtivo = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Ativo" },
                new SelectListItem { Value = "false", Text = "Inativo" }
            };
            ViewBag.ListaAtivo = new SelectList(listAtivo, "Value", "Text", ativo);
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-editar")]
        public IActionResult Edit()
        {
            return PartialView("Edit");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var query = await _db.Usuarios.FirstOrDefaultAsync(o => o.Id == id);
                    if (query != null)
                    {
                        query.Nome = form["txtNome"].ToString();
                        query.Email = form["txtEmail"].ToString();
                        query.Cargo = form["txtCargo"].ToString();

                        query.DataAlt = DateTime.Now;
                        query.UserAlt = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                        query.PermissaoId = int.Parse(form["txtPermissao"]);
                        if (!string.IsNullOrWhiteSpace(form["txtSenha"]))
                        {
                            query.Senha = CryptoHelper.HashMd5(form["txtSenha"]);
                        }

                        if (!string.IsNullOrEmpty(form["txtAtivo"]))
                        {
                            query.Ativo = Convert.ToBoolean(Convert.ToInt32(form["txtAtivo"]));
                        }

                        await _db.SaveChangesAsync();
                    }

                    TempData["SUCESSO"] = "Popup atualizado com sucesso!";

                    #region LOG
                    var userName = User.Identity?.Name ?? "Usuário desconhecido";
                    var userId = User.FindFirst("sub")?.Value ?? "0";

                    LogAuditoria.Action(userName, Convert.ToInt32(userId), "editou", "usuario", query.Id, query.Nome);
                    #endregion LOG

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar popup");

                #region LOG
                var userName = User.Identity?.Name ?? "Usuário desconhecido";
                var userId = User.FindFirst("sub")?.Value ?? "0";
                var query = await _db.Usuarios.FirstOrDefaultAsync(o => o.Id == id);

                LogAuditoria.Action(userName, Convert.ToInt32(userId), "Erro ao editar", "usuario", query.Id, query.Nome);
                #endregion LOG

                TempData["ERRO"] = $"Ocorreu um erro: {ex.Message}";
            }

            return View();
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

                var token = _context.Configs
                    .Where(o => o.Chave == "token" && o.Situacao)
                    .Select(o => o.Valor)
                    .FirstOrDefault();

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
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var usuaria = await _cmsService.SuspendAccountAsync(token, id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuaria-ativar-conta")]
        public async Task<IActionResult> Activate(string id)
        {
            var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            var usuaria = await _cmsService.ActivateAccountAsync(token, id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EnvioEmail(IFormCollection form)
        {
            try
            {
                var token = _context.Configs.Where(o => o.Chave == "token" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

                var id = form["UserId"];
                var subject = form["subject"];
                var content = form["content"];

                await _cmsService.EnvioEmailAsync(id, subject, content,token);

                TempData["SuccessMessage"] = "E-mail enviado com sucesso!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar artigo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

    }
}