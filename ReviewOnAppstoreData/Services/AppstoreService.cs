using Microsoft.Extensions.Hosting;
using ReviewOnAppstoreData.Contracts;
using ReviewOnAppstoreData.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Services
{
    public class AppstoreService : BackgroundService
    {
        private readonly IAppstoreScrapeRepository appstoreScrapeRepository;
        private readonly IAppRepository appRepository;
        public AppstoreService(IAppstoreScrapeRepository appstoreScrapeRepository, IAppRepository appRepository)
        {
            this.appstoreScrapeRepository = appstoreScrapeRepository;
            this.appRepository = appRepository;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var list_app = await appRepository.GetListAppFromDB();
                    foreach (var app_item in list_app)
                    {
                        var list_review = await appstoreScrapeRepository.GetAllReview(app_item.ID);
                        var list_review_db = await appstoreScrapeRepository.GetListReviewsFromDB(new CustomerReviewRequest { Limit = 100, Offset = 0, Query = "" });
                        DateTime latestDate = (from day in list_review_db.Data
                                               select day.CreatedTime).FirstOrDefault();
                        //DateTime latestDate = new DateTime(2022, 07, 19, 16, 15, 00);
                        foreach (var item in list_review)
                        {
                            int result_compare = DateTime.Compare(item.CreatedTime, latestDate);
                            if (result_compare > 0)
                            {
                                appstoreScrapeRepository.InsertReviews(item);
                            }

                        }
                    }

                    var list_response_db = await appstoreScrapeRepository.GetListResponseFromDB();
                    foreach (var item in list_response_db)
                    {
                        var response_app = await appstoreScrapeRepository.ReadCustomerReviewResponse(item.ResponseID);
                        var request_update = new UpdateStatusRequest
                        {
                            ResponseID = response_app.ResponseID,
                            State_response = response_app.State_response
                        };
                        appstoreScrapeRepository.UpdateStateResponse(request_update);
                    }
                    Console.WriteLine("true");
                    await Task.Delay(1000 * 60 * 60 * 6, stoppingToken);

                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }



    }
}
