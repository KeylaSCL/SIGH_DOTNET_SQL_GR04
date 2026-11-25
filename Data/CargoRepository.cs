using System.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Data;

public interface ICargoRepository
{
    Task<List<Cargo>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(string codigo, CancellationToken ct = default);
    Task InsertarAsync(Cargo c, CancellationToken ct = default);
    Task<int> ActualizarAsync(Cargo c, CancellationToken ct = default);
    Task<int> EliminarAsync(string codigo, CancellationToken ct = default);
}

public class CargoRepository : ICargoRepository
{
    private readonly ISqlConnectionFactory _fabrica;

    public CargoRepository(ISqlConnectionFactory fabrica) => _fabrica = fabrica;

    // Sin "Size": el parámetro toma el largo real del valor y nunca lo trunca.
    private static SqlParameter Texto(string nombre, string valor) =>
        new(nombre, SqlDbType.VarChar) { Value = (object)valor ?? DBNull.Value };

    /// <summary>Lista los cargos junto con la descripción de su departamento.</summary>
    public async Task<List<Cargo>> ListarAsync(CancellationToken ct = default)
    {
        const string sql =
            "SELECT c.PECAR_CODIGO, c.PECAR_DESCRI, c.PEDEP_CODIGO, d.PEDEP_DESCRI " +
            "FROM PECAR_CARGO c " +
            "INNER JOIN PEDEP_DEPAR d ON d.PEDEP_CODIGO = c.PEDEP_CODIGO " +
            "ORDER BY c.PECAR_CODIGO";

        var lista = new List<Cargo>();
        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        await using var rs = await cmd.ExecuteReaderAsync(ct);
        while (await rs.ReadAsync(ct))
        {
            lista.Add(new Cargo(rs.GetString(0).TrimEnd(),
                                rs.GetString(1),
                                rs.GetString(2).TrimEnd(),
                                rs.GetString(3)));
        }
        return lista;
    }

    public async Task<bool> ExisteAsync(string codigo, CancellationToken ct = default)
    {
        const string sql = "SELECT 1 FROM PECAR_CARGO WHERE PECAR_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", codigo));
        return await cmd.ExecuteScalarAsync(ct) != null;
    }

    public async Task InsertarAsync(Cargo c, CancellationToken ct = default)
    {
        const string sql =
            "INSERT INTO PECAR_CARGO (PECAR_CODIGO, PEDEP_CODIGO, PECAR_DESCRI) " +
            "VALUES (@codigo, @departamento, @descripcion)";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", c.Codigo));
        cmd.Parameters.Add(Texto("@departamento", c.DepartamentoCodigo));
        cmd.Parameters.Add(Texto("@descripcion", c.Descripcion));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Modifica descripción y departamento. El código no cambia.</summary>
    public async Task<int> ActualizarAsync(Cargo c, CancellationToken ct = default)
    {
        const string sql =
            "UPDATE PECAR_CARGO SET PEDEP_CODIGO = @departamento, PECAR_DESCRI = @descripcion " +
            "WHERE PECAR_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@departamento", c.DepartamentoCodigo));
        cmd.Parameters.Add(Texto("@descripcion", c.Descripcion));
        cmd.Parameters.Add(Texto("@codigo", c.Codigo));
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> EliminarAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;

        const string sql = "DELETE FROM PECAR_CARGO WHERE PECAR_CODIGO = @codigo";

        await using var cn = await _fabrica.AbrirAsync(ct);
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(Texto("@codigo", codigo.Trim()));
        return await cmd.ExecuteNonQueryAsync(ct);
    }
}