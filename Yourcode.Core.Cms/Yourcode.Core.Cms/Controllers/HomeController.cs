using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Yourcode.Core.Cms.Helpers;
using Yourcode.Core.Cms.Models;
using Yourcode.Core.Cms.Models.Dto;
using Yourcode.Core.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Yourcode.Core.Cms.Controllers
{
    //[AuditoriaAtribute]
    public class HomeController : CustomController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _context;

        public HomeController(ILogger<HomeController> logger, Context context)
            : base(logger, context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Verifica se o usuário está autenticado via Claims
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                //int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value)
                //LogAuditoria.Acao(, , "LOGOU", "popup", 1, "teste");
                LogAuditoria.Login(User.FindFirst(ClaimTypes.Name).Value, Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value), "está logado");
                return RedirectToAction("Index", "Dashboard");
            }

            // Carrega chave do reCAPTCHA
            ViewBag.ReCaptchaPublicKeyInvisible = _context.Configs
                .Where(o => o.Chave == "recaptcha-public-key-invisible" && o.Situacao)
                .Select(o => o.Valor)
                .FirstOrDefault();

            // Leitura dos cookies
            //Request.Cookies.TryGetValue("Login", out string cookieUserName);
            //Request.Cookies.TryGetValue("Senha", out string cookiePassword);

            //if (!string.IsNullOrEmpty(cookieUserName))
            //{
            //    ViewData["txtLogin"] = cookieUserName;
            //}

            //if (!string.IsNullOrEmpty(cookiePassword))
            //{
            //    ViewData["txtSenha"] = ""; // Recomendado não preencher senha, segurança
            //    ViewData["cbLembrarMe"] = true;
            //}
            //else
            //{
            //    ViewData["cbLembrarMe"] = false;
            //}

            // Mensagem do TempData (substituto do ViewData transitório)
            if (TempData.TryGetValue("mensagem", out var mensagem) && mensagem is string msg && !string.IsNullOrEmpty(msg))
            {
                ViewData["mensagem"] = msg;
            }

            // Url de retorno (Referrer)
            var referer = Request.Headers["Referer"].ToString();
            ViewBag.returnUrl = referer;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginDto model, string? returnUrl)
        {
            try
            {
                bool relembrar = false;
                if (ModelState.IsValid)
                {
                    var recaptchaSecret = _context.Configs.Where(o => o.Chave == "recaptcha-private-key-invisible" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
                    var recaptchaResponse = Request.Form["g-recaptcha-response"];
                    var isRecaptchaValid = Helpers.CmsGeneralHelper.ValidateRecaptcha(recaptchaSecret, recaptchaResponse);
                    if (!isRecaptchaValid)
                    {
                        ViewData["mensagem"] = "O Recapctha detectou Spam, por favor preencha os dados novamente!";
                        return View();
                    }


                    string user = model.Email;
                    string password = CryptoHelper.HashMd5(model.Senha);

                    var query = _context.Usuarios
                        .Where(o => o.Email != null && o.Senha != null && o.Email.Equals(user) && o.Senha.Equals(password) && o.Ativo == true && o.Situacao)
                        .FirstOrDefault();

                    if (query != null)
                    {
                        //string ip = Utilities.GeneralHelper.GetPublicServerIp();
                        string ip = "";
                        #region insere no LOG
                        _context.UsuarioLogs.Add(new UsuarioLog
                        {
                            Log = "ENTROU NO SISTEMA - IP: " + ip,
                            UsuarioId = query.Id,
                            DataInc = DateTime.Now
                        });
                        _context.SaveChanges();
                        LogAuditoria.Login(query.Nome, query.Id, "realizou login");
                        #endregion

                        //Session["IP"] = ip;
                        //Session["USUARIO"] = query;
                        //Session["userid"] = query.ID;

                        // Monta as Claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, query.Id.ToString()),
                            new Claim(ClaimTypes.Name, query.Nome ?? ""),
                            new Claim(ClaimTypes.Email, query.Email),
                            new Claim("UserIp", ip)
                        };

                        //atualiza a session de módulos
                        var queryMod = (from m in _context.Modulos
                                        join um in _context.ModuloPermissoes on m.Id equals um.ModuloId
                                        where m.Situacao && um.PermissaoId == query.PermissaoId
                                        select new UsuarioModuloDto
                                        {
                                            Id = m.Id,
                                            Modulo = m.Nome,
                                            ModuloId = m.Valor,
                                            Permitir = (bool)um.Permitir
                                        }).ToList();

                        foreach (var modulo in queryMod)
                        {
                            string valor = $"{modulo.ModuloId}|{modulo.Permitir.ToString().ToLower()}";
                            claims.Add(new Claim("PermissaoModulo", valor));
                        }

                        //Session["USUARIO_MODULO"] = queryMod;

                        //if (!string.IsNullOrEmpty(returnUrl))
                        //    return Redirect(returnUrl);

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        //FormsAuthentication.SetAuthCookie(model.Login, model.Lembrar);
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        ViewData["mensagem"] = "Usuário ou senha inválido!";
                        ViewData["txtLogin"] = model.Email;
                        ViewData["txtSenha"] = model.Senha;
                        ViewData["cbLembrarMe"] = relembrar;
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    string chavesInvalidas = string.Empty;
                    foreach (var key in ModelState.Keys)
                    {
                        chavesInvalidas += "<br />" + key;
                    }
                    ViewData["mensagem"] = "Chaves inválidas: <b>" + chavesInvalidas + "</b>";
                    ViewData["txtLogin"] = model.Email;
                    ViewData["txtSenha"] = model.Senha;
                    ViewData["cbLembrarMe"] = relembrar;
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception ex)
            {
                #region tratamento de erros
                //var st = new StackTrace();
                //var dtoLog = new LOG();
                //dtoLog. = st.GetFrame(0).GetMethod().ReflectedType.FullName + " - " + st.GetFrame(0).GetMethod().Name;
                //dtoLog.Descricao = ex.ToString();
                //dtoLog.DataCadastro = DateTime.Now;
                //var log = new LogService();
                //log.Inserir(dtoLog);
                return RedirectToAction("Error", "Home");
                #endregion
            }
        }

        //public void SetSessionTimeOut(int hour, int min, int sec)
        //{
        //    try
        //    {
        //        Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
        //        SystemWebSectionGroup syswebSection = (SystemWebSectionGroup)config.GetSectionGroup("system.web");

        //        //you can change the value as you want
        //        syswebSection.SessionState.Timeout = new TimeSpan(hour, min, sec);
        //        config.Save();
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Write(ex.Message);
        //    }
        //}

        public async Task<IActionResult> Logout()
        {
            LogAuditoria.Login(User.FindFirst(ClaimTypes.Name).Value, Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value), "fez logout");
            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal());
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpGet]
        //public ActionResult Logout()
        //{
        //    if (Session["USUARIO"] != null)
        //    {
        //        #region insere no LOG
        //        var usuarioLog = new USUARIO_LOG();
        //        usuarioLog.LOG = "SAIU DO SISTEMA";
        //        usuarioLog.USUARIO_ID = ((USUARIO)Session["USUARIO"]).ID;
        //        usuarioLog.DATAINC = DateTime.Now;
        //        db.USUARIO_LOG.Add(usuarioLog);
        //        db.SaveChanges();
        //        #endregion
        //    }
        //    FormsAuthentication.SignOut();
        //    Session.Abandon();
        //    Session.Clear();
        //    return RedirectToAction("Index", "Home");
        //}
    }
}
