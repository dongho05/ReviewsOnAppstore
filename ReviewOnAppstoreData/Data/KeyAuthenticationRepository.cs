using Dapper;
using Microsoft.Extensions.Configuration;
using ReviewOnAppstoreData.Context;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity.AuthenModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Data
{
    public class KeyAuthenticationRepository : IKeyAuthenticationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly DapperContext _context;
        public KeyAuthenticationRepository(IConfiguration configuration, DapperContext dapper)
        {
            _configuration = configuration;
            _context = dapper;
        }
        public async Task<bool> DeleteKeyAuthen(int id)
        {
            using (var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"delete from AppstoreAuthen where Id = @id", new { id = id });
                return true;
            }
        }

        public async Task<List<AuthenticationModel>> GetListAuthen()
        {
            using(var connection = _context.CreateConnection2())
            {
                var list = (await connection.QueryAsync<AuthenticationModel>(@"select * from AppstoreAuthen")).ToList();
                return list;
            }
        }

        public async Task<bool> InsertKeyAuthen(AuthenticationModel authen)
        {
            using(var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"insert into AppstoreAuthen values (@keyid,@issueid,@audience,@privatekey,@algorithm,@type)", new
                {
                    keyid = authen.Key_ID,
                    issueid = authen.Issuer_ID,
                    audience = authen.Audience,
                    privatekey = authen.Private_Key,
                    algorithm = authen.Algorithm,
                    type = authen.Type_Algorithm
                });
                return true;
            }
        }

        public async Task<bool> UpdateKeyAuthen(AuthenticationModel authen)
        {
            using (var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"update Authentication set 
                    Key_ID = @keyid,
                    Issuer_ID = @issueid, 
                    Audience = @audience, 
                    Private_Key = @privatekey,
                    Algorithm = @algorithm,
                    Type_Algorithm = @type", new
                {
                    keyid = authen.Key_ID,
                    issueid = authen.Issuer_ID,
                    audience = authen.Audience,
                    privatekey = authen.Private_Key,
                    algorithm = authen.Algorithm,
                    type = authen.Type_Algorithm
                });
                return true;
            }
        }
    }
}
