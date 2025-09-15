using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;

public class AuthorizeUserAttribute : Attribute, IAuthorizationFilter
{
    public string LoginPage { get; set; } = "/Home";
    public string Module { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Usuário não está logado
        if (!(user != null && user.Identity.IsAuthenticated))
        {
            context.Result = new RedirectResult(LoginPage);
            return;
        }

        // Verifica permissões com base nas claims
        if (!string.IsNullOrEmpty(Module))
        {
            var modulesClaim = ClaimsHelper.GetPermissoesDeModulos(user);

            if (modulesClaim == null)
            {
                context.Result = new RedirectResult("/nopermition");
                return;
            }

            var allowedModules = string.Join(",", modulesClaim.OrderBy(x => x.ModuloId).Select(x => x.ModuloId));

            if (!allowedModules.Contains(Module))
            {
                context.Result = new RedirectResult("/nopermition");
                return;
            }
        }
    }
}


public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}

