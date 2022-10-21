using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly IAppRepository _app;
        public AppController(IAppRepository app)
        {
            _app = app;
        }
        [HttpGet("[action]")]
        public async Task<AppInformation> GetApp(string app_id)
        {
            return await _app.GetApp(app_id);
        }
    }
}
