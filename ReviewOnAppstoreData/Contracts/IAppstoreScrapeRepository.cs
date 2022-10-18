using IronPython.Runtime;
using ReviewOnAppstoreData.Entity;
using ReviewOnAppstoreData.Entity.App;
using ReviewOnAppstoreData.Entity.AuthenModel;
using ReviewOnAppstoreData.Requests;
using ReviewOnAppstoreData.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Contracts
{
    public interface IAppstoreScrapeRepository
    {
        public Task<List<AuthenticationModel>> GetKeyToAccessToken();
        public Task<List<ReviewInfomation>> GetAllReview();
        public Task<bool> InsertReviews(ReviewInfomation input);
        public Task<bool> InsertResponse(ResponseInformation input);
        public string GenerateToken();
        public Task<ReviewInfomationResponse> GetListReviewsFromDB(CustomerReviewRequest request);
        public Task<List<ResponseInformation>> GetListResponseFromDB();
        public Task<string> GetCustomerReviewRespone(Guid reviewID);
        public Task<string> CreateAndUpdateResponeToCustomerReview(CreateReviewResquest request);
        public Task<ResponseInformation> ReadCustomerReviewResponse(Guid responseID);
        public Task<string> DeleteResponseToCustomerReview(Guid responseID);
        public Task<bool> UpdateStateResponse(UpdateStatusRequest input);
        public Task<AppInformation> GetApp();
        public Task<bool> CheckExistReviewID(Guid reviewID);
        public Task<bool> UpdateAppstoreResponse(ResponseInformation input);
        public Task<bool> DeleteAppstoreResponse(Guid responseID);
    }
}
