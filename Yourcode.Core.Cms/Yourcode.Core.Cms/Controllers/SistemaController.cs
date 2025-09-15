using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList.Extensions;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using Yourcode.Core.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaquelMenopausa.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-sistema")]
    public class SistemaController : CustomController
    {
        private readonly ILogger<SistemaController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;

        public SistemaController(ILogger<SistemaController> logger, Context db, IWebHostEnvironment env)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }


        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-sistema-editar-perfil")]
        public ActionResult EditarPerfil()
        {
            try
            {
                int id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                var query = _db.Usuarios.FirstOrDefault(o => o.Id == id);

                if (TempData["SUCESSO"] != null)
                    ViewData["SUCESSO"] = TempData["SUCESSO"];
                else if (TempData["ERRO"] != null)
                    ViewData["ERRO"] = TempData["ERRO"];

                if (query != null)
                {
                    ViewData["txtId"] = query.Id;
                    ViewData["txtNome"] = query.Nome;
                    ViewData["txtEmail"] = query.Email;
                    ViewData["txtLogin"] = query.Login;
                }

                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditarUsuario(IFormCollection form)
        {
            try
            {
                int id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                if (id != 0)
                {
                    var query = _db.Usuarios.FirstOrDefault(o => o.Id == id);
                    if (query != null)
                    {
                        query.Nome = form["txtNome"].ToString();
                        query.Login = form["txtLogin"].ToString();
                        query.Email = form["txtEmail"].ToString();
                        query.DataInc = DateTime.Now;

                        _db.SaveChanges();

                    }

                        #region LOG
                        var userName = User.Identity?.Name ?? "Usuário desconhecido";
                    var userId = User.FindFirst("sub")?.Value ?? "0";

                    LogAuditoria.Action(userName, Convert.ToInt32(userId), "alterou", "usuario", query.Id, query.Nome);
                    #endregion LOG
                }

                TempData["SUCESSO"] = "Usuário alterado com sucesso! Faça login novamente.";
                return RedirectToAction("EditarPerfil", "Sistema");
            }
            catch (Exception)
            {
                TempData["ERRO"] = "Erro ao alterar.";
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditarSenha(IFormCollection form)
        {
            try
            {
                int id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                if (id != 0)
                {
                    var query = _db.Usuarios.FirstOrDefault(o => o.Id == id);
                    if (query != null)
                    {
                        var senhaAtual = CryptoHelper.HashMd5(form["txtSenhaAtual"]);

                        if (senhaAtual == query.Senha)
                        {
                            if (!string.IsNullOrEmpty(form["txtSenhaNova"]))
                            {
                                query.Senha = CryptoHelper.HashMd5(form["txtSenhaNova"]);
                            }
                            else
                            {
                                query.Senha = CryptoHelper.HashMd5(form["txtSenhaAtual"]);
                            }

                            query.DataInc = DateTime.Now;
                            _db.SaveChanges();

                            #region LOG
                            var userName = User.Identity?.Name ?? "Usuário desconhecido";
                            var userId = User.FindFirst("sub")?.Value ?? "0";

                            LogAuditoria.Action(userName, Convert.ToInt32(userId), "alterou a senha", "usuario", query.Id, query.Nome);
                            #endregion LOG

                            TempData["SUCESSO"] = "Senha alterada com sucesso!";
                        }
                        else
                        {
                            TempData["ERRO"] = "Digite corretamente a senha atual.";
                        }
                    }
                }

                return RedirectToAction("EditarPerfil", "Sistema");
            }
            catch (Exception ex)
            {

                TempData["ERRO"] = "Erro ao alterar a senha.";
                return View();
            }
        }
    }
}