using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using X.PagedList.Extensions;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using RaquelMenopausa.Cms.Models.Dto;

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
        public IActionResult Index(int? page)
        {
            if (TempData["SUCESSO"] != null)
                ViewData["SUCESSO"] = TempData["SUCESSO"];
            else if (TempData["ERRO"] != null)
                ViewData["ERRO"] = TempData["ERRO"];

            var query = _db.Permissoes.OrderBy(o => o.Tipo).OrderBy(o => o.Tipo).ToList();
            List<PerfisDto> listPerfis = new List<PerfisDto>();
            foreach (var item in query)
            {
                var perfis = new PerfisDto
                {
                    Id = item.Id,
                    Tipo = item.Tipo,
                    Delete = !_db.Usuarios.Any(u => u.PermissaoId == item.Id)
                };

                listPerfis.Add(perfis);
            }

            ViewData["ListaPerfis"] = listPerfis;

            ViewData["Mensagem"] = TempData["Mensagem"];

            var model = _db.Usuarios.Include(o => o.Permissao).Where(o => o.Situacao).OrderByDescending(o => o.Id).ToList();

            int pageSize = 10;
            int pageIndex = page ?? 1;

            var emp = model.ToPagedList(pageIndex, pageSize);
            ViewBag.PageCount = emp.PageCount;

            return View(emp);
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

                    // Cria todos os módulos como "Permitir = false"
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



        //[HttpGet]
        //[ValidateAntiForgeryToken]
        //[AuthorizeUser(LoginPage = "~/home", Module = "modulo-perfis-deletar")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        var query = await _db.Permissoes
        //            .Include(p => p.Usuarios)
        //            .FirstOrDefaultAsync(o => o.Id == id);

        //        if (query != null)
        //        {
        //            var delete = query.Usuarios.FirstOrDefault(o => o.PermissaoId == id);

        //            if (delete == null)
        //            {
        //                var queryModulo = await _db.UsuarioModulos
        //                    .Where(o => o.PermissaoId == query.Id)
        //                    .ToListAsync();

        //                _db.UsuarioModulos.RemoveRange(queryModulo);

        //                _db.Permissoes.Remove(query);

        //                await _db.SaveChangesAsync();
        //            }
        //        }

        //        TempData["SUCESSO"] = "Permissão deletada com sucesso!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        TempData["ERRO"] = "Erro ao deletar.";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}


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
