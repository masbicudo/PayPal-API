using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using PayPal.Version940;

namespace Mvc3Sample.Controllers
{
    public class PayPalController : Controller
    {
        //
        // GET: /PayPal/

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult PayPalCheckout()
        {
            var operation = new PayPalSetExpressCheckoutOperation();
            FillOperationDetails(operation, true);

            // Validating the request object.
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(
                operation,
                new ValidationContext(operation, null, null),
                validationResults,
                validateAllProperties: true);

            var opResult = this.SetExpressCheckout(operation, "PayPalConfirm", "PayPalCancel");

            return this.RedirectToCheckout(opResult);
        }

        private void FillOperationDetails<T1>(T1 operation, bool hasDiscount)
          where T1 : PayPalExpressCheckoutOperation, new()
        {
            operation.DefaultCurrencyCode = CurrencyCode.UnitedStates_Dollar;
            operation.PaymentRequests = new PayPalList<PayPalPaymentRequest>
            {
                new PayPalPaymentRequest
                {
                    Description = "My SaaS",
                    Items = new PayPalList<PayPalPaymentRequestItem>
                    {
                        new PayPalPaymentRequestItem
                        {
                            Amount = 100.00m,
                            Name = "SaaS",
                            Description = "Software as a service",
                            Category = ItemCategory.Digital,
                        },
                    },
                },
            };

            if (hasDiscount)
            {
                operation.PaymentRequests[0].Items.Add(new PayPalPaymentRequestItem
                {
                    Amount = -20.00m,
                    Name = "Special discount! (20%)",
                    Description = "Special discount for special customers.",
                    Category = ItemCategory.Digital,
                });
            }
        }

        public ActionResult PayPalConfirm(PayPalExpressCheckoutConfirmation data)
        {
            var operation = new PayPalDoExpressCheckoutPaymentOperation
            {
                PayerId = data.PayerId,
                Token = data.Token,
            };
            this.FillOperationDetails(operation, true);

            this.DoExpressCheckoutPayment(operation);

            return this.RedirectToAction("PaymentConfirmed");
        }

        public ActionResult PaymentConfirmed(PayPalExpressCheckoutConfirmation data)
        {
            return this.View();
        }

        public ActionResult PayPalCancel()
        {
            return this.RedirectToAction("PaymentCanceled");
        }

        public ActionResult PaymentCanceled()
        {
            return this.View();
        }
    }
}
