using log4net;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Stripe.Controllers
{
    public class HomeController : Controller
    {
        private ILog log = LogManager.GetLogger("HomeController");
        public ActionResult Index()
        {
            // Set your secret key. Remember to switch to your live secret key in production!
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.ApiKey = "sk_test_51Gy9HFFpAeHvoOJGeTIYoZ6tQsbY30DbGfY6OOu38CuNTuf6tykHW5eChnLi50Aa1HJhusgG8QNnqenqOTLD7PCM00FAoO8sEP";

            var options = new PaymentIntentCreateOptions
            {
                Amount = 1099,
                Currency = "sgd",
                // Verify your integration in this guide by including this parameter
                Metadata = new Dictionary<string, string>
            {
              { "integration_check", "accept_a_payment" },
            },
            };

            var service = new PaymentIntentService();
            var intent = service.Create(options);
            ViewData["ClientSecret"] = intent.ClientSecret;
            return View();

        }
    }
}