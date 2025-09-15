using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;

namespace RaquelMenopausa.Cms.Controllers
{
    public class CustomController : Controller
    {
        public readonly ILogger _logger;
        public readonly Context _context;

        public CustomController(ILogger logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            ViewBag.TitlePage = _context?.Configs.Where(o => o.Chave == "nome-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            ViewBag.ContatoEmail = _context?.Configs.Where(o => o.Chave == "contato-email" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            ViewBag.CorAdmin = _context?.Configs.Where(o => o.Chave == "cor-administrador" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
            ViewBag.DomainSite = _context?.Configs.Where(o => o.Chave == "dominio-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

            //var qmod = ClaimsHelper.GetModulosDoUsuario(User);
            //var modulosPermitidos = ClaimsHelper.GetPermissoesDeModulos(User);
            //ViewBag.ModulosPermitidos = modulosPermitidos;

            if (User.Identity.IsAuthenticated)
            {
                var modulosPermitidos = ClaimsHelper.GetPermissoesDeModulos(User);
                ViewBag.ModulosPermitidos = modulosPermitidos;
            }
            else
            {
                ViewBag.ModulosPermitidos = new List<RaquelMenopausa.Cms.Models.Dto.PermissaoModuloDto>();
            }
        }
    }
}