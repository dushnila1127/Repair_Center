using Microsoft.Data.SqlClient;

namespace RepairCenter.Api.Data;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;
    public SqlConnectionFactory(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("RepairCenterDb") ?? throw new InvalidOperationException("Connection string RepairCenterDb is not configured.");
    public SqlConnection Create() => new SqlConnection(_connectionString);
}
