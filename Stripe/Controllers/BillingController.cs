using Stripe.Views.CreateSubscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace Stripe.Controllers
{
    public class BillingController : Controller
    {
        private readonly IOptions<StripeOptions> options;

        public BillingController(IOptions<StripeOptions> options)
        {
            this.options = options;
            StripeConfiguration.ApiKey = options.Value.SecretKey;
        }

        [HttpPost("create-subscription")]
        public ActionResult<Subscription> CreateSubscription([FromBody] CreateSubscriptionRequest req)
        {
            // Attach payment method
            var options = new PaymentMethodAttachOptions
            {
                Customer = req.Customer,
            };
            var service = new PaymentMethodService();
            var paymentMethod = service.Attach(req.PaymentMethod, options);

            // Update customer's default invoice payment method
            var customerOptions = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethod.Id,
                },
            };
            var customerService = new CustomerService();
            customerService.Update(req.Customer, customerOptions);

            // Create subscription
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = req.Customer,
                Items = new List<SubscriptionItemOptions>
        {
            new SubscriptionItemOptions
            {
                Price = Environment.GetEnvironmentVariable(req.Price),
            },
        },
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = subscriptionService.Create(subscriptionOptions);
                return subscription;
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Failed to create subscription.{e}");
                return BadRequest();
            }
        



        [HttpPost("cancel-subscription")]
        public ActionResult<Subscription> CancelSubscription([FromBody] CancelSubscriptionRequest req)
        {
            var service = new SubscriptionService();
            var subscription = service.Cancel(req.Subscription, null);
            return subscription;
        }

        [HttpPost("update-subscription")]
        public ActionResult<Subscription> UpdateSubscription([FromBody] UpdateSubscriptionRequest req)
        {
            var service = new SubscriptionService();
            var subscription = service.Get(req.Subscription);

            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = Environment.GetEnvironmentVariable(req.NewPrice),
                    }
                }
            };
            var updatedSubscription = service.Update(req.Subscription, options);
            return updatedSubscription;
        }

        [HttpPost("retrieve-customer-payment-method")]
        public ActionResult<PaymentMethod> RetrieveCustomerPaymentMethod([FromBody] RetrieveCustomerPaymentMethodRequest req)
        {
            var service = new PaymentMethodService();
            var paymentMethod = service.Get(req.PaymentMethod);
            return paymentMethod;
        }


    }
}
