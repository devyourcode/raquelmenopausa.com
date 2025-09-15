using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Yourcode.Core.Cms.Models;

public class AuditoriaAttribute : ActionFilterAttribute
{
    private readonly Context _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaAttribute(Context context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext.Request;

        var usuario = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "Anonymous";

        var ip = httpContext.Connection.RemoteIpAddress?.ToString();

        var areaAcesso = $"{request.Path}{request.QueryString}";

        var audit = new Auditoria
        {
            AuditoriaId = Guid.NewGuid().ToString(),
            UsuarioAcesso = usuario,
            Ip = ip,
            AreaAcesso = areaAcesso,
            Data = DateTime.Now
        };

        _context.Auditorias.Add(audit);
        _context.SaveChanges();

        base.OnActionExecuting(context);
    }
}
