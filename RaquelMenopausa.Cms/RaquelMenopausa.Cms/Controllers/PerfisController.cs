using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using X.PagedList.Extensions;
using Yourcode.Core.Utilities;

namespace RaquelMenopausa.Cms.Controllers
{
    [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis")]
    public class PerfisController : CustomController
    {
        private readonly ILogger<PerfisController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;

        public PerfisController(ILogger<PerfisController> logger, Context db, IWebHostEnvironment env)
            : base(logger, db)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-listar")]
        public IActionResult Index(int? page, string search)
        {
            if (TempData["SUCESSO"] != null)
                ViewData["SUCESSO"] = TempData["SUCESSO"];
            else if (TempData["ERRO"] != null)
                ViewData["ERRO"] = TempData["ERRO"];

            int usuarioLogadoId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var usuarioLogado = _db.Usuarios
                .Include(u => u.Permissao)
                .FirstOrDefault(u => u.Id == usuarioLogadoId);

            bool usuarioLogadoEhMaster = usuarioLogado.Email.EndsWith("@yourcode.com.br");

            IQueryable<Usuario> query = _db.Usuarios
                .Where(o => o.Situacao)
                .Include(o => o.Permissao)
                .Include(o => o.UsuarioModuloPermissoes) 
                .ThenInclude(ump => ump.Modulo);

            if (!usuarioLogadoEhMaster)
            {
                query = query.Where(o => !o.Email.EndsWith("@yourcode.com.br"));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Nome.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Cargo.ToLower().Contains(search));
            }
            ViewBag.Search = search; 


            var totalUsuarios = query.Count(); 
            ViewBag.TotalUsuarios = totalUsuarios;

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
        public IActionResult CreateUsuario()
        {

            var queryModulo = _db.Modulos.Where(o => o.Situacao && o.Ativo).ToList();
            ViewData["listaModulo"] = queryModulo;

            return PartialView("CreateUsuario");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUsuario(IFormCollection form)
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
                    usuario.PermissaoId = 3;
                    usuario.Senha = CryptoHelper.HashMd5(form["txtSenha"].ToString());
                    usuario.Ativo = true;
                    usuario.DataInc = DateTime.Now;
                    usuario.Situacao = true;
                    _db.Usuarios.Add(usuario);
                    await _db.SaveChangesAsync();

                    var modulosSelecionados = form["ModulosSelecionados"];

                    if (modulosSelecionados.Count > 0)
                    {
                        foreach (var moduloId in modulosSelecionados)
                        {
                            var permissao = new UsuarioModuloPermissao
                            {
                                UsuarioId = usuario.Id,
                                ModuloId = int.Parse(moduloId),
                                Permitir = true
                            };
                            _db.UsuarioModuloPermissoes.Add(permissao);
                        }
                        await _db.SaveChangesAsync();
                    }


                    var nome_site = _db.Configs.Where(o => o.Chave == "nome-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();
                    var admin_site = _db.Configs.Where(o => o.Chave == "admin-site" && o.Situacao).Select(o => o.Valor).FirstOrDefault();

                    var mensagemHTML = "";
                    mensagemHTML = "Seu usuário foi cadastrado no administrador do site " + nome_site + ".<br />Segue abaixo os dados para acesso:" +
                       "<br><br>Link: <a target='_blank' href='" + admin_site + "'>" + admin_site + "</a>" +
                       "<br>Login: " + usuario.Email +
                       "<br>Senha: " + senha_sem_criptografia +
                       "<br><br>Atenciosamente,<br><b>Suporte Técnico - " + nome_site + "</b>";


                    bool enviado = await CmsNotificationHelper.SendEmail(
                        clienteId: 1149,
                        projetoId: 354,
                        nome: nome_site,
                        remetente: "noreply@raquelmenopausa.com.br",
                        destinatario: usuario.Email,
                        assunto: "Cadastro de Usuário",
                        mensagem: mensagemHTML
                    );


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

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-criar")]
        public IActionResult Create()
        {
            var queryModulo = _db.Modulos.ToList();
            ViewData["listaModulo"] = queryModulo;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var permissao = new Permissao
                    {
                        Tipo = form["txtNome"].ToString()
                    };
                    _db.Permissoes.Add(permissao);
                    await _db.SaveChangesAsync();

                    var queryModulos = await _db.Modulos.OrderBy(m => m.Nome).ToListAsync();
                    var listaModulos = new List<ModuloPermissao>();

                    foreach (var item in queryModulos)
                    {
                        listaModulos.Add(new ModuloPermissao
                        {
                            ModuloId = item.Id,
                            PermissaoId = permissao.Id,
                            Permitir = false
                        });
                    }

                    _db.ModuloPermissoes.AddRange(listaModulos);
                    await _db.SaveChangesAsync();

                    // Marca os checkboxes selecionados como true
                    if (form.TryGetValue("txtModulo", out var selecionados) && selecionados.Count > 0)
                    {
                        var modulosPermissao = await _db.ModuloPermissoes
                            .Where(u => u.PermissaoId == permissao.Id)
                            .ToListAsync();

                        foreach (var sel in selecionados)
                        {
                            if (int.TryParse(sel, out int moduloId))
                            {
                                var usuarioModulo = modulosPermissao.FirstOrDefault(u => u.ModuloId == moduloId);
                                if (usuarioModulo != null)
                                    usuarioModulo.Permitir = true;
                            }
                        }
                        await _db.SaveChangesAsync();
                    }

                    // LOG
                    var userName = User.Identity?.Name ?? "Usuário desconhecido";
                    var userId = User.FindFirst("sub")?.Value ?? "0";
                    CmsGeneralHelper.SaveLogInfo($"{userName}({userId}) ADICIONOU A PERMISSÃO ({permissao.Id} {permissao.Tipo})");

                    TempData["SUCESSO"] = "Perfil adicionado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ERRO"] = "Dados inválidos!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ERRO"] = "Erro ao adicionar: " + ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-editar")]
        public async Task<IActionResult> EditUsuario(int id)
        {

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(o => o.Id == id);
            if (usuario == null)
                return NotFound();

            ViewData["txtNome"] = usuario.Nome;
            ViewData["txtEmail"] = usuario.Email;
            ViewData["txtCargo"] = usuario.Cargo;

            var todosModulos = await _db.Modulos
                .Where(x => x.Situacao)
                .OrderBy(x => x.Nome)
                .GroupBy(x => x.Id) 
                .Select(g => g.First())
                .ToListAsync();

            var permissoesUsuario = await _db.UsuarioModuloPermissoes
                .Where(x => x.UsuarioId == usuario.Id && x.Permitir)
                .Select(x => x.ModuloId)
                .ToListAsync();

            ViewData["listaModulo"] = todosModulos
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nome,
                    Selected = permissoesUsuario.Contains(m.Id)
                })
                .ToList();


            return PartialView("EditUsuario", usuario);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUsuario(int id, IFormCollection form)
        {
            try
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(o => o.Id == id);
                if (usuario == null)
                    return NotFound();

                usuario.Nome = form["txtNome"].ToString();
                usuario.Email = form["txtEmail"].ToString();
                usuario.Cargo = form["txtCargo"].ToString();
                usuario.DataAlt = DateTime.Now;
                usuario.UserAlt = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                if (!string.IsNullOrWhiteSpace(form["txtSenha"]))
                {
                    usuario.Senha = CryptoHelper.HashMd5(form["txtSenha"]);
                }

                if (!string.IsNullOrEmpty(form["txtAtivo"]))
                {
                    usuario.Ativo = Convert.ToBoolean(Convert.ToInt32(form["txtAtivo"]));
                }

                await _db.SaveChangesAsync();

                var modulosSelecionados = form["modulosSelecionados"]
                    .Select(int.Parse)
                    .Distinct() // remove qualquer duplicata vinda do front-end
                    .ToList();

                var permissoesAtuais = await _db.UsuarioModuloPermissoes
                    .Where(x => x.UsuarioId == usuario.Id)
                    .ToListAsync();

                foreach (var permissao in permissoesAtuais)
                {
                    if (!modulosSelecionados.Contains(permissao.ModuloId) && permissao.Permitir)
                    {
                        permissao.Permitir = false;
                        _db.Entry(permissao).State = EntityState.Modified;
                    }
                }

                foreach (var moduloId in modulosSelecionados)
                {
                    var permissaoExistente = permissoesAtuais
                        .FirstOrDefault(p => p.ModuloId == moduloId && p.UsuarioId == usuario.Id);

                    if (permissaoExistente != null)
                    {
                        if (!permissaoExistente.Permitir)
                        {
                            permissaoExistente.Permitir = true;
                            _db.Entry(permissaoExistente).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        var novaPermissao = new UsuarioModuloPermissao
                        {
                            UsuarioId = usuario.Id,
                            ModuloId = moduloId,
                            Permitir = true
                        };

                        _db.UsuarioModuloPermissoes.Add(novaPermissao);
                    }
                }

                await _db.SaveChangesAsync();


                TempData["SUCESSO"] = "Usuário atualizado com sucesso!";

                #region LOG
                var userName = User.Identity?.Name ?? "Usuário desconhecido";
                var userId = User.FindFirst("sub")?.Value ?? "0";
                LogAuditoria.Action(userName, Convert.ToInt32(userId), "editou", "usuario", usuario.Id, usuario.Nome);
                #endregion LOG

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar usuário");

                TempData["ERRO"] = $"Ocorreu um erro: {ex.Message}";

                #region LOG
                var userName = User.Identity?.Name ?? "Usuário desconhecido";
                var userId = User.FindFirst("sub")?.Value ?? "0";
                LogAuditoria.Action(userName, Convert.ToInt32(userId), "Erro ao editar", "usuario", id, form["txtNome"]);
                #endregion LOG

                return View();
            }
        }



        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-editar")]
        public async Task<IActionResult> Edit(int id)
        {
            var query = await _db.Permissoes
                .FirstOrDefaultAsync(o => o.Id == id);

            if (query != null)
            {
                ViewData["txtNome"] = query.Tipo;

                //var modulesUser = ClaimsHelper.GetPermissoesDeModulos(User);
                var moduloPermissao = _db.ModuloPermissoes.Where(x => x.PermissaoId == id).ToList();
                var modulos = await _db.Modulos.Where(x => x.Situacao).ToListAsync();
                var listModulo = modulos
                    .Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Nome,
                        Selected = moduloPermissao
                            .Where(x => x.ModuloId.Equals(item.Id))
                            .Select(x => x.Permitir)
                            .FirstOrDefault() == true
                    })
                    .OrderBy(o => o.Text)
                    .ToList();

                ViewData["listaModulo"] = listModulo;
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            try
            {
                var permissao = await _db.Permissoes
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (permissao == null)
                {
                    TempData["ERRO"] = "Permissão não encontrada!";
                    return RedirectToAction(nameof(Index));
                }

                permissao.Tipo = form["txtNome"].ToString();
                await _db.SaveChangesAsync();

                var queryModulo = await _db.ModuloPermissoes
                    .Where(o => o.PermissaoId == permissao.Id)
                    .ToListAsync();

                _db.ModuloPermissoes.RemoveRange(queryModulo);
                await _db.SaveChangesAsync();

                var queryModulos = await _db.Modulos.ToListAsync();
                var novosModulos = new List<ModuloPermissao>();

                foreach (var item in queryModulos)
                {
                    novosModulos.Add(new ModuloPermissao
                    {
                        ModuloId = item.Id,
                        PermissaoId = permissao.Id,
                        Permitir = false
                    });
                }

                _db.ModuloPermissoes.AddRange(novosModulos);
                await _db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(form["txtModulo"]))
                {
                    var modulos = await _db.ModuloPermissoes
                        .Where(o => o.PermissaoId == permissao.Id)
                        .ToListAsync();

                    string[] arraySelecionados = form["txtModulo"].ToString().Split(',');

                    foreach (var selecionado in arraySelecionados)
                    {
                        var usuario_modulo = modulos
                            .FirstOrDefault(o => o.ModuloId == int.Parse(selecionado));

                        if (usuario_modulo != null)
                            usuario_modulo.Permitir = true;
                    }

                    await _db.SaveChangesAsync();
                }

                TempData["SUCESSO"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ERRO"] = "Erro ao atualizar!";
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpGet]
        [AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-deletar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    TempData["ERRO"] = "Usuário não encontrado.";
                    return RedirectToAction("Index");
                }

                usuario.Situacao = false;
                usuario.Ativo = false;

                _db.Usuarios.Update(usuario);
                await _db.SaveChangesAsync();

                TempData["SUCESSO"] = "Usuário desativado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ERRO"] = "Erro ao desativar o usuário.";
                return RedirectToAction("Index");
            }
        }

public IActionResult ExportarUsuarios()
    {
        var usuarios = _db.Usuarios
            .Where(u => u.Situacao)
            .OrderBy(u => u.Nome)
            .Select(u => new
            {
                u.Nome,
                u.Email,
                u.Cargo,
                Status = u.Ativo.GetValueOrDefault() ? "Ativo" : "Inativo",
                DataCadastro = u.DataInc.ToString("dd/MM/yyyy")
            })
            .ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Usuários");

            worksheet.Cell(1, 1).Value = "Nome";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Cargo";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "Data de Cadastro";

            int row = 2;
            foreach (var usuario in usuarios)
            {
                worksheet.Cell(row, 1).Value = usuario.Nome;
                worksheet.Cell(row, 2).Value = usuario.Email;
                worksheet.Cell(row, 3).Value = usuario.Cargo;
                worksheet.Cell(row, 4).Value = usuario.Status;
                worksheet.Cell(row, 5).Value = usuario.DataCadastro;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Usuarios_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }


    private bool VerificaModuloSelecionado(string[] arraySelecionados, int p)
        {
            bool flag = false;
            foreach (var item in arraySelecionados)
            {
                if (item.ToString().Equals(p.ToString()))
                {
                    flag = true;
                }
            }
            return flag;
        }

    }
}
