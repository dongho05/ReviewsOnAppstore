using Dapper;
using IronPython.Runtime;
using Jose;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ReviewOnAppstoreData.Context;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity;
using ReviewOnAppstoreData.Entity.App;
using ReviewOnAppstoreData.Entity.AuthenModel;
using ReviewOnAppstoreData.Requests;
using ReviewOnAppstoreData.Responses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Runtime.Profiler;

namespace ReviewOnAppstoreData.Data
{
    public class AppstoreScrapeRepository : IAppstoreScrapeRepository
    {
        private readonly IConfiguration _configuration;
        private readonly DapperContext _context;
        public AppstoreScrapeRepository(IConfiguration configuration, DapperContext dapper)
        {
            _configuration = configuration;
            _context = dapper;
        }

        public string GenerateToken()
        {
            var list_key_authen = GetKeyToAccessToken().Result;
            var key_authen = list_key_authen.FirstOrDefault();
            var header = new Dictionary<string, object>()
            {
                { "alg", key_authen.Algorithm },
                { "kid", key_authen.Key_ID },
                { "typ", key_authen.Type_Algorithm }
                //{ "alg", _configuration["Jwt:Algorithm"] },
                //{ "kid", _configuration["Jwt:Key"] },
                //{ "typ", "JWT" }
            };

            //var scope = new string[1] { "GET /v1/apps?filter[platform]=IOS" };
            var payload = new Dictionary<string, object>
            {
                //{ "iss",_configuration["Jwt:Issuer"]},
                { "iss",key_authen.Issuer_ID},
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds() },
                //{ "aud", _configuration["Jwt:Audience"] }
                { "aud", key_authen.Audience }
            };
            var privateKey = key_authen.Private_Key;
            privateKey = privateKey.Replace("\\r\\n", string.Empty);
            privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", "");
            privateKey = privateKey.Replace("-----END PRIVATE KEY-----", "");
            CngKey key = CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.Pkcs8PrivateBlob);

            string token = Jose.JWT.Encode(payload, key, JwsAlgorithm.ES256, header);

            return token;
        }

        public async Task<List<ReviewInfomation>> GetAllReview(int app_id)
        {
            List<ReviewInfomation> listReview = new List<ReviewInfomation>();
            var token = GenerateToken();
            var app = GetAppByID(app_id).Result;
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/apps/{app.App_ID}/customerReviews?limit={_configuration["Jwt:Limit"]}", RestSharp.Method.Get);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);

                if (respone.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var info = (JsonConvert.DeserializeObject<ReviewsRespone>(respone.Content)).Data.ToList();
                    foreach (var item in info)
                    {
                        var review_infor = new ReviewInfomation
                        {
                            IdCustomer = item.id,
                            TypeReview = item.type,
                            Rating = item.attributes.rating,
                            Title = item.attributes.title,
                            BodyDescription = item.attributes.body,
                            NameReviewer = item.attributes.reviewerNickname,
                            CreatedTime = item.attributes.createdDate,
                            Territory = item.attributes.territory,
                            App_ID = app_id
                        };
                        listReview.Add(review_infor);
                    }
                }
                return listReview;

            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<string> GetCustomerReviewRespone(Guid reviewID)
        {
            var token = GenerateToken();
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/customerReviews/{reviewID}/response", RestSharp.Method.Get);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);
                return respone.Content.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception("Has Errors");
            }
        }
        public async Task<int> GetIDAppByReviewID(Guid reviewid)
        {
            using (var connection = _context.CreateConnection2())
            {
                var result = await connection.QueryAsync<int>(@"select al.ID from AppList al join AppstoreReviews ar on al.ID = ar.App_ID 
                where ar.IdCustomerRV = @id", new { id = reviewid });
                return result.FirstOrDefault();
            }
        }
        public async Task<string> CreateAndUpdateResponeToCustomerReview(CreateReviewResquest request)
        {
            var token = GenerateToken();
            var check = CheckExistReviewID(request.data.relationships.review.data.id);
            var app_id = GetIDAppByReviewID(request.data.relationships.review.data.id).Result;
            try
            {

                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest("v1/customerReviewResponses", RestSharp.Method.Post);
                requesturl.AddHeader("content-type", "application/json");
                requesturl.AddHeader("Authorization", "Bearer " + token);
                var body = new CreateReviewResquest { data = request.data };
                requesturl.AddParameter("application/json-patch+json", body, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(requesturl);

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var info = (JsonConvert.DeserializeObject<ResponseReviewsResponse>(response.Content)).Data;
                    //foreach (var item in info)
                    //{
                    var response_infor = new ResponseInformation
                    {
                        Type_Response = info.type,
                        ResponseID = info.id,
                        ResponseBody = info.attributes.responseBody,
                        LastModifiedDate = info.attributes.lastModifiedDate,
                        State_response = info.attributes.state,
                        ReviewID = request.data.relationships.review.data.id,
                        CreatedDate = info.attributes.lastModifiedDate,
                        App_ID = app_id
                    };
                    if(check.Result == false)
                    {
                            InsertResponse(response_infor);
                    }
                    else
                    {
                        UpdateAppstoreResponse(response_infor);
                    }
                    
                    //}

                }

                return response.Content.ToString();

            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<ReviewInfomationResponse> GetListReviewsFromDB(CustomerReviewRequest request)
        {
            using (var connection = _context.CreateConnection2())
            {
                var list = await connection.QueryAsync<ReviewInfomation>(@"select * from AppstoreReviews
                                where Title Like '%'+ @query +'%' or @query = ''
                                order by CreatedTime desc 
                                offset @offset rows fetch next @limit rows only ", new
                {
                                                                             query = request.Query,
                                                                             offset = request.Offset,
                                                                             limit= request.Limit
                });
                var total = (await connection.QueryAsync<int>(@"select count(*) from AppstoreReviews")).FirstOrDefault();
                var result = new ReviewInfomationResponse { Total = total, Data = list.ToList() };
                return result;
            }
        }

        public async Task<bool> InsertReviews(ReviewInfomation input)
        {
            using (var connection = _context.CreateConnection2())
            {
                var result = await connection.ExecuteAsync(@"insert into AppstoreReviews 
                    values(@IdCustomer,
                    @TypeReview,
                    @Rating,
                    @Title,
                    @BodyDescription,
                    @NameReviewer,
                    @CreatedTime,
                    @Territory,
                    @App_ID)", new
                {
                    IdCustomer = input.IdCustomer,
                    TypeReview = input.TypeReview,
                    Rating = input.Rating,
                    Title = input.Title,
                    BodyDescription = input.BodyDescription,
                    NameReviewer = input.NameReviewer,
                    CreatedTime = input.CreatedTime,
                    Territory = input.Territory,
                    App_ID = input.App_ID
                });
                return true;
            }
        }

        public async Task<bool> InsertResponse(ResponseInformation input)
        {
            using (var connection = _context.CreateConnection2())
            {
                var result = await connection.ExecuteAsync(@"insert into AppstoreResponse 
                    values(@Type,
                    @ResponseID,
                    @ResponseBody,
                    @LastModifiedDate,
                    @State_response,
                    @ReviewID,
                    @CreatedDate,
                    @App_ID)", new
                {
                    Type = input.Type_Response,
                    ResponseID = input.ResponseID,
                    ResponseBody = input.ResponseBody,
                    LastModifiedDate = input.LastModifiedDate,
                    State_response = input.State_response,
                    ReviewID = input.ReviewID,
                    CreatedDate = input.CreatedDate,
                    App_ID=input.App_ID
                });
                return true;
            }
        }

        public async Task<ResponseInformation> ReadCustomerReviewResponse(Guid responseID)
        {
            var token = GenerateToken();
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/customerReviewResponses/{responseID}", RestSharp.Method.Get);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);
                if(respone.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var info = (JsonConvert.DeserializeObject<ResponseReviewsResponse>(respone.Content)).Data;

                    var response_infor = new ResponseInformation
                    {
                        ResponseID = info.id,
                        LastModifiedDate = info.attributes.lastModifiedDate,
                        ResponseBody = info.attributes.responseBody,
                        State_response = info.attributes.state,
                        Type_Response = info.type
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

        public async Task<string> DeleteResponseToCustomerReview(Guid responseID)
        {
            var token = GenerateToken();
            try
            {
                //using (var connection = _context.CreateConnection())
                //{
                //    var response_infor = await ReadCustomerReviewResponse(responseID);
                //    var result = await connection.ExecuteAsync(@"insert into Logging 
                //    values(@Type,
                //    @ResponseID,
                //    @ResponseBody,
                //    @LastModifiedDate,
                //    @State_response,
                //    @Process)", new
                //    {
                //        Type = response_infor.Type_Response,
                //        ResponseID = response_infor.ResponseID,
                //        ResponseBody = response_infor.ResponseBody,
                //        LastModifiedDate = response_infor.LastModifiedDate,
                //        State_response = response_infor.State_response,
                //        Process = "Delete"
                //    });
                //    var result_delete = await connection.ExecuteAsync(@"delete from Responses where ResponseID = " + responseID + "");
                    
                //}
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/customerReviewResponses/{responseID}", RestSharp.Method.Delete);
                requesturl.RequestFormat = DataFormat.Json;
                requesturl.AddHeader("content-type", "application/json-patch+json");
                requesturl.AddHeader("Authorization", token);

                var respone = await client.ExecuteAsync(requesturl);
                DeleteAppstoreResponse(responseID);
                return respone.Content.ToString();
            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<List<ResponseInformation>> GetListResponseFromDB()
        {
            using (var connection = _context.CreateConnection2())
            {
                var list = await connection.QueryAsync<ResponseInformation>(@"select * from AppstoreResponse");
                return list.ToList();
            }
        }

        public async Task<bool> UpdateStateResponse(UpdateStatusRequest input)
        {
            using(var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"update AppstoreResponse set State_response = @state where ResponseID = @id ",
                    new {state = input.State_response, id = input.ResponseID});
                return true;
            }
        }

        public async Task<List<AuthenticationModel>> GetKeyToAccessToken()
        {
            using(var connection = _context.CreateConnection2())
            {
                var result = (await connection.QueryAsync<AuthenticationModel>(@"select * from AppstoreAuthen")).ToList();
                return result;
            }
        }

        //public async Task<AppInformation> GetApp()
        //{
        //    var token = GenerateToken();
        //    try
        //    {
        //        RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
        //        var requesturl = new RestRequest("v1/apps", RestSharp.Method.Get);
        //        requesturl.RequestFormat = DataFormat.Json;
        //        requesturl.AddHeader("content-type", "application/json-patch+json");
        //        requesturl.AddHeader("Authorization", token);

        //        var respone = await client.ExecuteAsync(requesturl);
        //        if (respone.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var info = (JsonConvert.DeserializeObject<AppModel>(respone.Content)).data;

        //            var response_infor = new AppInformation
        //            {
        //                App_ID = info.id,
        //                App_name = info.attributes.name,
        //                Description = info.attributes.name,
        //                Type_app = "Appstore"
        //            };
        //            return response_infor;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("Has Errors");
        //    }
        //}

        public async Task<bool> CheckExistReviewID(Guid reviewID)
        {
            using(var connection = _context.CreateConnection2())
            {
                var query = (await connection.QueryAsync<ResponseInformation>(@"select * from AppstoreResponse where ReviewID = @id", new { id = reviewID })).FirstOrDefault();
                if(query != null) { return true; }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> UpdateAppstoreResponse(ResponseInformation input)
        {
            using(var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"update AppstoreResponse set LastModifiedDate = @date,ResponseBody = @responsebody",
                    new {date= input.LastModifiedDate,responsebody = input.ResponseBody});
                return true;
            }
        }

        public async Task<bool> DeleteAppstoreResponse(Guid responseID)
        {
            using (var connection = _context.CreateConnection2())
            {
                var query = await connection.ExecuteAsync(@"delete from AppstoreResponse where ResponseID = @id", new {id = responseID});
                return true;
            }
        }
        public async Task<AppInformation> GetAppByID(int id)
        {
            using (var connection = _context.CreateConnection2())
            {
                var result = await connection.QueryAsync<AppInformation>(@"select * from AppList where ID = @id", new { id = id });
                return result.FirstOrDefault();
            }
        }
    }
}
