using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using Yourcode.Core.Cms.Controllers;
using Yourcode.Core.Cms.Helpers;
using Yourcode.Core.Cms.Models;
using Yourcode.Core.Cms.Models.Dto;
using Yourcode.Core.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Yourcode.CMS.Controllers
{
    public class EsqueciSenhaController : CustomController
    {
        private readonly ILogger<EsqueciSenhaController> _logger;
        private readonly Context _context;

        public EsqueciSenhaController(ILogger<EsqueciSenhaController> logger, Context context)
            : base(logger, context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var usuario = _context.Usuarios.FirstOrDefault();

            ViewBag.Usuario = usuario;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginDto model)
        {
            try
            {
                //var util = new Criptografia();
                string user = model.Email;

                var query = _context.Usuarios.Where(o => (o.Email.Equals(user) || o.Email.Equals(user)) && o.Ativo == true).FirstOrDefault();

                if (query != null)
                {
                    Guid guid = Guid.NewGuid();

                    query.RecuperarSenha = guid.ToString();
                    _context.SaveChanges();

                    var mensagem = "<b>Você esqueceu sua senha no Administrador do site da " + ViewBag.TitlePage + "<br />Clique no LINK abaixo e coloque sua nova senha.</b><br /><br />Caso você não fez esta solicitação, desconsidere este e-mail<br /><br /><a href='" + ViewBag.DomainSite + "/admin/recuperarsenha/novasenha/[{CODIGO}]' target=_blank'/><b>Clique aqui para recuperar sua senha</b></a>";

                    mensagem = mensagem.Replace("[{CODIGO}]", query.RecuperarSenha);

                    //var not = new NotificacaoService();
                    //bool retorno = not.EnviaEmail(ViewBag.ContatoEmail, query.EMAIL, "Alterar de Senha" + ViewBag.TitlePage, mensagem);
                    bool retorno = false;

                    #region ViewBags
                    //config
                    var busca_cliente_id = _context.Configs.Where(o => o.Chave == "cliente-id" && o.Situacao).FirstOrDefault();
                    int cliente_id = int.Parse(busca_cliente_id.Valor);
                    var busca_proj_id = _context.Configs.Where(o => o.Chave == "projeto-id" && o.Situacao).FirstOrDefault();
                    int projeto_id = int.Parse(busca_proj_id.Valor);
                    var email_remetente = _context.Configs.Where(o => o.Chave == "email-remetente" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
                    var email_destinatario = _context.Configs.Where(o => o.Chave == "email-destinatario" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
                    #endregion

                    bool enviado = await CmsNotificationHelper.SendEmail(
                                clienteId: 1148,
                                projetoId: 345,
                                nome: "YourCode",
                                remetente: "noreply@clinicapromaxi.com",
                                destinatario: model.Email,
                                assunto: "Teste",
                                mensagem: mensagem
                            );

                    if (retorno)
                    {
                        ViewData["RETORNO"] = true;
                    }
                    else
                    {
                        ViewData["RETORNO"] = false;
                    }

                    #region LOG
                    CmsGeneralHelper.SaveLogInfo($"{User.FindFirst(ClaimTypes.Name)?.Value}({User.FindFirst(ClaimTypes.NameIdentifier)?.Value}) SOLICITOU LINK DE RECUPERACAO DE SENHA ({query.Id} {query.Nome})");
                    #endregion LOG

                    TempData["mensagem"] = "Foi enviado um e-mail para fazer a recuperação da senha";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["mensagem"] = "Usuário ou E-mail não encontrado";
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception ex)
            {
                var st = new StackTrace();
                CmsGeneralHelper.SaveLogInfo($"{User.FindFirst(ClaimTypes.Name)?.Value}({User.FindFirst(ClaimTypes.NameIdentifier)?.Value}) - {st.GetFrame(0).GetMethod().ReflectedType.FullName} - {st.GetFrame(0).GetMethod().Name}");
                return RedirectToAction("Index", "Erros");
            }
        }
    }
}
