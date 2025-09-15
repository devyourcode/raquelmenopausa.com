using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using Yourcode.Core.Cms.Helpers;
using Yourcode.Core.Cms.Models;
using Yourcode.Core.Utilities;
using Yourcode.Core.Cms.Models.Dto;

namespace Yourcode.Core.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-contato")]
    public class RecuperarSenhaController : CustomController
    {
        private readonly ILogger<RecuperarSenhaController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;

        public RecuperarSenhaController(ILogger<RecuperarSenhaController> logger, Context db, IWebHostEnvironment env)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }
        public ActionResult NovaSenha(string id)
        {
            var usuario = _db.Usuarios.Where(o => o.RecuperarSenha == id).FirstOrDefault();

            if (usuario != null)
            {
                return View();
            }
            else
            {
                TempData["mensagem"] = "Requisite uma nova troca de senha!";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Admin/RecuperarSenha
        [HttpPost]
        public ActionResult NovaSenha(string id, LoginDto model)
        {
            var usuario = _db.Usuarios.Where(o => o.RecuperarSenha == id).FirstOrDefault();

            if (usuario != null)
            {
                usuario.RecuperarSenha = null;
                usuario.Senha = CryptoHelper.HashMd5(model.Senha);

                _db.SaveChanges();

                TempData["mensagem"] = "Senha atualizada!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["mensagem"] = "Falha ao tentar trocar a senha";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}