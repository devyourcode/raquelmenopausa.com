using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;

namespace RaquelMenopausa.Cms.Controllers
{
    //[AuditoriaAtribute]
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-dashboard")]
    public class DashboardController : CustomController
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly Context _context;

        public DashboardController(ILogger<DashboardController> logger, Context context) 
            : base(logger, context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Admin/Dashboard/
        public async Task<IActionResult> Index()
        {
            //MÉTODO PARA ENVIAR EMAIL

            //bool enviado = await CmsNotificationHelper.SendEmail(
            //            clienteId: 1148,
            //            projetoId: 345,
            //            nome: "YourCode",
            //            remetente: "noreply@clinicapromaxi.com",
            //            destinatario: "contato@yourcode.com.br",
            //            assunto: "Teste",
            //            mensagem: "<b>Este é um e-mail de teste.</b>"
            //        );

            return View();
        }

    }
}
