using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity.AuthenModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyAuthenticationController : ControllerBase
    {
        private readonly IKeyAuthenticationRepository _keyauthen;
        public KeyAuthenticationController(IKeyAuthenticationRepository keyauthen)
        {
            _keyauthen = keyauthen;
        }
        [HttpPost("[action]")]
        public async Task<List<AuthenticationModel>> GetListAuthen()
        {
            return await _keyauthen.GetListAuthen();
        }
        [HttpDelete("[action]")]
        public async Task<bool> DeleteKeyAuthen(int id)
        {
            return await _keyauthen.DeleteKeyAuthen(id);
        }
        [HttpPost("[action]")]
        public async Task<bool> InsertKeyAuthen(AuthenticationModel authen)
        {
            return await _keyauthen.InsertKeyAuthen(authen);
        }
        [HttpPut("[action]")]
        public async Task<bool> UpdateKeyAuthen(AuthenticationModel authen)
        {
            return await _keyauthen.UpdateKeyAuthen(authen);
        }

        //[HttpPost]

    }
}
