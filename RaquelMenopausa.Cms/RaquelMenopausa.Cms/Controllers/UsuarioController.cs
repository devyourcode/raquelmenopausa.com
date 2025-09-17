using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using X.PagedList.Extensions;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using Yourcode.Core.Utilities;

namespace RaquelMenopausa.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario")]
    public class UsuarioController : CustomController
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;

        public UsuarioController(ILogger<UsuarioController> logger, Context db, IWebHostEnvironment env)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }



        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-listar")]
        public IActionResult Index(int? page)
        {
            //var listConfig = new List<Usuario>();
            //if (((USUARIO)Session["USUARIO"]).PERMISSAO.TIPO.Contains("Administrador"))
            //{
            //    listConfig = db.USUARIO.Where(o => o.SITUACAO).OrderBy(o => o.NOME).ToList();
            //}
            //else
            //{
            //    listConfig = db.USUARIO.Where(o => o.SITUACAO && o.PERMISSAO.TIPO.Contains("Usuário")).OrderBy(o => o.NOME).ToList();
            //}

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
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-deletar")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Situacao = false;
            usuario.Ativo = false;
            usuario.UserAlt = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            usuario.DataAlt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["SUCESSO"] = "Usuário removido!";

            #region LOG
            var userName = User.Identity?.Name ?? "Usuário desconhecido";
            var userId = User.FindFirst("sub")?.Value ?? "0";

            LogAuditoria.Action(userName, Convert.ToInt32(userId), "deletou", "usuario", usuario.Id, usuario.Nome);
            #endregion LOG

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-usuario-editar")]
        public async Task<IActionResult> AtivarInativar(int id)
        {
            try
            {
                var registro = await _db.Usuarios.FirstOrDefaultAsync(o => o.Id == id);
                if (registro != null)
                {
                    registro.UserAlt = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                    registro.DataAlt = DateTime.Now;

                    if (registro.Ativo == true)
                    {
                        _logger.LogInformation($"Usuário {User.Identity.Name} ({registro.UserAlt}) DESATIVOU o usuário ({registro.Id})");

                        registro.Ativo = false;
                    }
                    else
                    {
                        _logger.LogInformation($"Usuário {User.Identity.Name} ({registro.UserAlt}) ATIVOU o usuário ({registro.Id})");

                        registro.Ativo = true;
                    }

                    await _db.SaveChangesAsync();
                    TempData["SUCESSO"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ERRO"] = "Usuário não encontrado!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar/inativar usuário");

                TempData["ERRO"] = "Ocorreu um erro ao processar sua solicitação.";
                return RedirectToAction("Index");
            }
        }


    }
}