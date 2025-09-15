using System.Security.Claims;
using RaquelMenopausa.Cms.Models.Dto;

namespace RaquelMenopausa.Cms.Helpers
{
    public static class ClaimsHelper
    {
        /// <summary>
        /// Buscar todos os módulos salvos no Claim "PermissaoModulo"
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetModulosDoUsuario(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return new List<string>();

            return user.Claims
                       .Where(c => c.Type == "PermissaoModulo")
                       .Select(c => c.Value)
                       .ToList();
        }

        /// <summary>
        /// Busca os módulos e se estão liberados (true) ou não (false) nas Claims.
        /// </summary>
        public static List<PermissaoModuloDto> GetPermissoesDeModulos(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return new List<PermissaoModuloDto>();

            var lista = user.Claims
                .Where(c => c.Type == "PermissaoModulo")
                .Select(c =>
                {
                    var partes = c.Value.Split('|');
                    return new PermissaoModuloDto
                    {
                        ModuloId = partes[0],
                        Liberado = partes.Length > 1 && partes[1].ToLower() == "true" ? true : false
                    };
                })
                .OrderBy(x => x.ModuloId)
                .ToList();

            return lista;
        }
    }
}
