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
            var header = new Dictionary<string, object>()
            {
                { "alg", _configuration["Jwt:Algorithm"] },
                { "kid", _configuration["Jwt:Key"] },
                { "typ", "JWT" }
            };

            var scope = new string[1] { "GET /v1/apps?filter[platform]=IOS" };
            var payload = new Dictionary<string, object>
            {
                { "iss",_configuration["Jwt:Issuer"]},
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds() },
                { "aud", _configuration["Jwt:Audience"] }
            };
            var privateKey = _configuration["Jwt:PrivateKey"];
            privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", "");
            privateKey = privateKey.Replace("-----END PRIVATE KEY-----", "");
            CngKey key = CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.Pkcs8PrivateBlob);

            string token = Jose.JWT.Encode(payload, key, JwsAlgorithm.ES256, header);

            return token;
        }

        public async Task<List<ReviewInfomation>> GetAllReview()
        {
            List<ReviewInfomation> listReview = new List<ReviewInfomation>();
            var token = GenerateToken();
            try
            {
                RestClient client = new RestClient(_configuration.GetSection("AppClient").Value);
                var requesturl = new RestRequest($"v1/apps/1625930819/customerReviews?limit={_configuration["Jwt:Limit"]}", RestSharp.Method.Get);
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
                            Territory = item.attributes.territory
                        };
                        listReview.Add(review_infor);
                    }
                }
                //var data = JObject.Parse(respone.Content);
                //var list_Data = data.SelectToken("data");
                //foreach (var item in list_Data)
                //{
                //    //var type = item.SelectToken("type").ToString();
                //    //var cusID = Guid.Parse(item.SelectToken("id").ToString());
                //    //var listAttribute = item.SelectToken("attributes");
                //    //var rate = listAttribute.SelectToken("rating");
                //    //var title = listAttribute.SelectToken("title").ToString();
                //    //var body = listAttribute.SelectToken("body").ToString();
                //    //var reviewerNickname = listAttribute.SelectToken("reviewerNickname").ToString();
                //    //var createdDate = DateTime.Parse(listAttribute.SelectToken("createdDate").ToString());
                //    //var territory = listAttribute.SelectToken("territory").ToString();

                //    var review_infor = new ReviewInfomation
                //    {
                //        IdCustomer = cusID,
                //        TypeReview = type,
                //        Rating = Convert.ToInt32(rate),
                //        Title = title,
                //        BodyDescription = body,
                //        NameReviewer = reviewerNickname,
                //        CreatedTime = createdDate,
                //        Territory = territory

                //    };
                //    listReview.Add(review_infor);

                //}
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

        public async Task<string> CreateAndUpdateResponeToCustomerReview(CreateReviewResquest request)
        {
            var token = GenerateToken();
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
                        State_response = info.attributes.state
                    };
                    InsertResponse(response_infor);
                    //}

                }

                return response.Content.ToString();

            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<List<ReviewInfomation>> GetListReviewsFromDB()
        {
            using (var connection = _context.CreateConnection())
            {
                var list = await connection.QueryAsync<ReviewInfomation>(@"select * from Reviews order by CreatedTime desc ");
                return list.ToList();
            }
        }

        public async Task<bool> InsertReviews(ReviewInfomation input)
        {
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(@"insert into Reviews 
                    values(@IdCustomer,
                    @TypeReview,
                    @Rating,
                    @Title,
                    @BodyDescription,
                    @NameReviewer,
                    @CreatedTime,
                    @Territory)", new
                {
                    IdCustomer = input.IdCustomer,
                    TypeReview = input.TypeReview,
                    Rating = input.Rating,
                    Title = input.Title,
                    BodyDescription = input.BodyDescription,
                    NameReviewer = input.NameReviewer,
                    CreatedTime = input.CreatedTime,
                    Territory = input.Territory
                });
                return true;
            }
        }

        public async Task<bool> InsertResponse(ResponseInformation input)
        {
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(@"insert into Responses 
                    values(@Type,
                    @ResponseID,
                    @ResponseBody,
                    @LastModifiedDate,
                    @State_response)", new
                {
                    Type = input.Type_Response,
                    ResponseID = input.ResponseID,
                    ResponseBody = input.ResponseBody,
                    LastModifiedDate = input.LastModifiedDate,
                    State_response = input.State_response
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
                return respone.Content.ToString();
            }
            catch (Exception ex)
            {

                throw new Exception("Has Errors");
            }
        }

        public async Task<List<ResponseInformation>> GetListResponseFromDB()
        {
            using (var connection = _context.CreateConnection())
            {
                var list = await connection.QueryAsync<ResponseInformation>(@"select * from Responses order by CreatedTime desc ");
                return list.ToList();
            }
        }
    }
}
