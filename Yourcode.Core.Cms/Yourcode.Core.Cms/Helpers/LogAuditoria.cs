using Serilog;

namespace Yourcode.Core.Cms.Helpers
{
    public static class LogAuditoria
    {
        public static void Login(string usuario, int usuarioId, string acao)
        {
            Log.Information("{Usuario}({UsuarioId}) {Acao}",
                usuario, usuarioId, acao.ToUpperInvariant());
        }

        public static void Action(string usuario, int usuarioId, string acao, string entidade, int idEntidade, string descricao)
        {
            Log.Information("{Usuario}({UsuarioId}) {Acao} O {Entidade} ({IdEntidade} {Descricao})",
                usuario, usuarioId, acao.ToUpperInvariant(), entidade.ToUpperInvariant(), idEntidade, descricao);
        }
    }
}
