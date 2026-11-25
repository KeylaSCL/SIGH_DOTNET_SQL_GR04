using System.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Data;

public interface IUsuarioRepository
{
    Task<Usuario> BuscarPorCodigoAsync(string codigo, CancellationToken ct = default);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ISqlConnectionFactory _fabrica;

    public UsuarioRepository(ISqlConnectionFactory fabrica) => _fabrica = fabrica;

    public async Task<Usuario> BuscarPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        const string sql =
            "SELECT XEUSU_CODIGO, PEPER_CODIGO, XEEST_CODIGO, XEUSU_PASWD, XEUSU_INTFAL " +
            "FROM XEUSU_USUAR WHERE XEUSU_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@codigo", SqlDbType.VarChar) { Value = codigo });

        await using var rs = await cmd.ExecuteReaderAsync(ct);
        if (!await rs.ReadAsync(ct)) return null;

        return new Usuario
        {
            Codigo = rs.GetString(0).TrimEnd(),
            CodigoPersona = rs.IsDBNull(1) ? null : rs.GetString(1).TrimEnd(),
            CodigoEstado = rs.IsDBNull(2) ? null : rs.GetString(2).TrimEnd(),
            ContrasenaAlmacenada = rs.IsDBNull(3) ? null : rs.GetString(3),
            IntentosFallidos = rs.IsDBNull(4) ? 0 : rs.GetInt32(4)
        };
    }
}
