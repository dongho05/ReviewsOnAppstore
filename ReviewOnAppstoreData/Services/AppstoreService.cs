﻿using Microsoft.Extensions.Hosting;
using ReviewOnAppstoreData.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ReviewOnAppstoreData.Services
{
    public class AppstoreService : BackgroundService
    {
        private readonly IAppstoreScrapeRepository appstoreScrapeRepository;
        public AppstoreService(IAppstoreScrapeRepository appstoreScrapeRepository)
        {
            this.appstoreScrapeRepository = appstoreScrapeRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var list_review = await appstoreScrapeRepository.GetAllReview();
                    var list_review_db = await appstoreScrapeRepository.GetListReviewsFromDB();
                    DateTime latestDate = (from day in list_review_db
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