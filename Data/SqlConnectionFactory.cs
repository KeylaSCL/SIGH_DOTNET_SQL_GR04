using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Data;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> AbrirAsync(CancellationToken ct = default);
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _cadena;

    public SqlConnectionFactory(IConfiguration configuracion)
    {
        _cadena = configuracion.GetConnectionString("SighoDb")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'SighoDb' en appsettings.json.");
    }

    public async Task<SqlConnection> AbrirAsync(CancellationToken ct = default)
    {
        var conexion = new SqlConnection(_cadena);
        await conexion.OpenAsync(ct);
        return conexion;
    }
}
