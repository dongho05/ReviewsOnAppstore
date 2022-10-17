using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ReviewOnAppstoreData.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _connectionString2;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
            _connectionString2 = _configuration.GetConnectionString("newconnect");
        }
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
        public IDbConnection CreateConnection2() => new SqlConnection(_connectionString2);
    }
}
