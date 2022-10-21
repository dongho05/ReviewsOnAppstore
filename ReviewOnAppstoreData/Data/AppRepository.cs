using Newtonsoft.Json;
using RestSharp;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity.App;
using ReviewOnAppstoreData.Entity;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using ReviewOnAppstoreData.Context;
using Dapper;

namespace ReviewOnAppstoreData.Data
{
    public class AppRepository : IAppRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IAppstoreScrapeRepository _scrapeRepository;
        private readonly DapperContext _context;
        public AppRepository(IConfiguration configuration, IAppstoreScrapeRepository scrapeRepository,DapperContext context)
        {
            _configuration = configuration;
            _scrapeRepository = scrapeRepository;
            _context = context;
        }

        public async Task<AppInformation> GetApp(string app_id)
        {
            var token = _scrapeRepository.GenerateToken();
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/apps/{app_id}", RestSharp.Method.Get);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);
                if (respone.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var info = (JsonConvert.DeserializeObject<AppModel>(respone.Content)).data;

                    var response_infor = new AppInformation
                    {
                        App_ID = app_id,
                        App_name = info.attributes.name,
                        Description = info.attributes.name,
                        Type_app = "Appstore"
                    };
                    return response_infor;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<List<AppInformation>> GetListApp()
        {
            var token = _scrapeRepository.GenerateToken();
            List<AppInformation> list = new List<AppInformation>();
            
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest("v1/apps", RestSharp.Method.Get);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);
                if (respone.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var info = (JsonConvert.DeserializeObject<ListAppModel>(respone.Content)).data.ToList();
                    foreach (var item in info)
                    {
                        var response_infor = new AppInformation
                        {
                            App_ID = item.id,
                            App_name = item.attributes.name,
                            Description = item.attributes.name,
                            Type_app = "Appstore"
                        };
                        list.Add(response_infor);
                    }

                    return list;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }
        public async Task<AppInformation> GetAppByAppID(string app_id)
        {
            using (var connection = _context.CreateConnection2())
            {
                var result = await connection.QueryAsync<AppInformation>(@"select * from AppList where App_ID = @id", new { id = app_id });
                return result.FirstOrDefault();
            }
        }

        public async Task<List<AppInformation>> GetListAppFromDB()
        {
            using(var connection = _context.CreateConnection2())
            {
                var result = await connection.QueryAsync<AppInformation>(@"select * from AppList");
                return result.ToList(); 
            }
        }
    }
}
