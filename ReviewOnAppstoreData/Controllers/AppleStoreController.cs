using IronPython.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Entity;
using ReviewOnAppstoreData.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppleStoreController : ControllerBase
    {
        private readonly IAppstoreScrapeRepository _scrapeRepository;
        public AppleStoreController(IAppstoreScrapeRepository appstoreScrape)
        {
            _scrapeRepository = appstoreScrape;
        }
        [HttpGet("[action]")]
        public string GenerateToken()
        {
            return _scrapeRepository.GenerateToken();
        }
        [HttpGet("[action]")]
        public async Task<List<ReviewInfomation>> GetAll()
        {
            return await _scrapeRepository.GetAllReview();
        }
        [HttpGet("[action]")]
        //Get the response to a specific customer review.
        public async Task<string> GetCustomerReviewRespone(Guid reviewID)
        {
            return await _scrapeRepository.GetCustomerReviewRespone(reviewID);
        }
        [HttpPost("[action]")]
        //Create a response or replace an existing response you wrote to a customer review.
        public async Task<string> CreateAndUpdateResponeToCustomerReview([FromBody] CreateReviewResquest request)
        {
            return await _scrapeRepository.CreateAndUpdateResponeToCustomerReview(request);
        }
        [HttpGet("[action]")]
        //Get information about a specific response you wrote to a customer review, including the response content and its state.
        public async Task<ResponseInformation> ReadCustomerReviewResponse(Guid responseID)
        {
            return await _scrapeRepository.ReadCustomerReviewResponse(responseID);
        }
        [HttpDelete("[action]")]
        public async Task<string> DeleteResponseToCustomerReview(Guid responseID)
        {
            return await _scrapeRepository.DeleteResponseToCustomerReview(responseID);
        }
        [HttpGet("[action]")]
        public async Task<List<ResponseInformation>> GetListResponseFromDB()
        {
            return await _scrapeRepository.GetListResponseFromDB();
        }
        [HttpPost("[action]")]
        public async Task<bool> UpdateStateResponse(UpdateStatusRequest input)
        {
            return await _scrapeRepository.UpdateStateResponse(input);
        }
    }
}
