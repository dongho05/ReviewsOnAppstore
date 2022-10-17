using IronPython.Runtime;
using ReviewOnAppstoreData.Entity;
using ReviewOnAppstoreData.Entity.AuthenModel;
using ReviewOnAppstoreData.Requests;
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
        public Task<List<ReviewInfomation>> GetListReviewsFromDB();
        public Task<List<ResponseInformation>> GetListResponseFromDB();
        public Task<string> GetCustomerReviewRespone(Guid reviewID);
        public Task<string> CreateAndUpdateResponeToCustomerReview(CreateReviewResquest request);
        public Task<ResponseInformation> ReadCustomerReviewResponse(Guid responseID);
        public Task<string> DeleteResponseToCustomerReview(Guid responseID);
        public Task<bool> UpdateStateResponse(UpdateStatusRequest input);
    }
}
