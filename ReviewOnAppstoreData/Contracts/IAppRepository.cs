using ReviewOnAppstoreData.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Contracts
{
    public interface IAppRepository
    {
        public Task<List<AppInformation>> GetListApp();
        public Task<AppInformation> GetApp(string app_id);
    }
}
