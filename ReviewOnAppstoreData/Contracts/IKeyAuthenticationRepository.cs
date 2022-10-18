using IronPython.Runtime;
using ReviewOnAppstoreData.Entity.AuthenModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Contracts
{
    public interface IKeyAuthenticationRepository
    {
        public Task<bool> InsertKeyAuthen(AuthenticationModel authen);
        public Task<bool> UpdateKeyAuthen(AuthenticationModel authen);
        public Task<bool> DeleteKeyAuthen(int id);
        public Task<List<AuthenticationModel>> GetListAuthen();
    }
}
