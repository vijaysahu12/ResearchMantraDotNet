using RM.Model.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.CommonServices.Services
{
    public class LandingPageService
    {
        public List<LandingPage> GetLandingPageList()
        {
            var landingPages = new List<LandingPage>
            {
                new LandingPage { Code = "none", Name = "None" },
                new LandingPage { Code = "getAllBlogs", Name = "Blogs Screen" },
                new LandingPage { Code = "productscreen", Name = "Products Screen" },
                new LandingPage { Code = "researchScreen", Name = "Research Screen" },
                new LandingPage { Code = "subscriptonPlanScreen", Name = "Subscription Plan By Product Name" },
                new LandingPage { Code = "productDetailsScreenWidget", Name = "Product Details Screen" },
                new LandingPage { Code = "myBucketList", Name = "My Bucket Screen" },
                new LandingPage { Code = "ticketsScreen", Name = "Ticket Screen" },
                new LandingPage { Code = "performancescreen", Name = "Performance Screen" },
                new LandingPage { Code = "scannersScreen", Name = "My Scanner Screen" },
                new LandingPage { Code = "screenerScreen", Name = "Screener Screen" },
                new LandingPage { Code = "partnerAccountScreen", Name = "Partner Account Screen" },
                new LandingPage { Code = "marketBasicsScreen", Name = "Market Basics Screen" },
                new LandingPage { Code = "inflationscreen", Name = "Inflation Screen" },
                new LandingPage { Code = "marketAnalysisScreen", Name = "Market Analysis Screen" },
                new LandingPage { Code = "store", Name = "Apple/Android Store" }
            };

            return landingPages;
        }

    }
}
