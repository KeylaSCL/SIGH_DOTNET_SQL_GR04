using System.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Data;

public interface IDepartamentoRepository
{
    Task<List<Departamento>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(string codigo, CancellationToken ct = default);
    Task InsertarAsync(Departamento d, CancellationToken ct = default);
    Task<int> ActualizarAsync(Departamento d, CancellationToken ct = default);
    Task<int> EliminarAsync(string codigo, CancellationToken ct = default);
}


public class DepartamentoRepository : IDepartamentoRepository
{
    private readonly ISqlConnectionFactory _fabrica;

    public DepartamentoRepository(ISqlConnectionFactory fabrica) => _fabrica = fabrica;

    // Sin "Size": el parámetro toma el largo real del valor y nunca lo trunca.
    private static SqlParameter Texto(string nombre, string valor) =>
        new(nombre, SqlDbType.VarChar) { Value = (object)valor ?? DBNull.Value };

    public async Task<List<Departamento>> ListarAsync(CancellationToken ct = default)
    {
        const string sql =
            "SELECT PEDEP_CODIGO, PEDEP_DESCRI FROM PEDEP_DEPAR ORDER BY PEDEP_CODIGO";

        var lista = new List<Departamento>();
        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        await using var rs = await cmd.ExecuteReaderAsync(ct);
        while (await rs.ReadAsync(ct))
        {
            lista.Add(new Departamento(rs.GetString(0).TrimEnd(), rs.GetString(1)));
        }
        return lista;
    }

    public async Task<bool> ExisteAsync(string codigo, CancellationToken ct = default)
    {
        const string sql = "SELECT 1 FROM PEDEP_DEPAR WHERE PEDEP_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", codigo));
        return await cmd.ExecuteScalarAsync(ct) != null;
    }

    public async Task InsertarAsync(Departamento d, CancellationToken ct = default)
    {
        const string sql =
            "INSERT INTO PEDEP_DEPAR (PEDEP_CODIGO, PEDEP_DESCRI) VALUES (@codigo, @descripcion)";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", d.Codigo));
        cmd.Parameters.Add(Texto("@descripcion", d.Descripcion));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Modifica solamente la descripción. Devuelve las filas afectadas.</summary>
    public async Task<int> ActualizarAsync(Departamento d, CancellationToken ct = default)
    {
        const string sql =
            "UPDATE PEDEP_DEPAR SET PEDEP_DESCRI = @descripcion WHERE PEDEP_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@descripcion", d.Descripcion));
        cmd.Parameters.Add(Texto("@codigo", d.Codigo));
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Elimina SOLO el departamento solicitado. No borra cargos relacionados:
    /// si existen, SQL Server devuelve el error 547 (clave foránea) y el
    /// controlador lo muestra al usuario.
    /// </summary>
    public async Task<int> EliminarAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;

        const string sql = "DELETE FROM PEDEP_DEPAR WHERE PEDEP_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", codigo.Trim()));
        return await cmd.ExecuteNonQueryAsync(ct);
    }
}
