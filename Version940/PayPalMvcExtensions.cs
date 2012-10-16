using System;
using System.Reflection;
using System.Web.Mvc;

namespace PayPal.Version940
{
    public static class PayPalMvcExtensions
    {
        delegate RedirectResult RedirectFunc(Controller controller, string url);

        static RedirectFunc redirectFunc = (RedirectFunc)Delegate.CreateDelegate(
            typeof(RedirectFunc),
            typeof(Controller).GetMethod("Redirect", BindingFlags.NonPublic | BindingFlags.Instance));

        /// <summary>
        /// Changes the express checkout operation
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="request"></param>
        public static void SetupExpressCheckoutUrls(this Controller controller, PayPalSetExpressCheckoutOperation request, string confirmAction, string cancelAction)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            if (request == null)
                throw new ArgumentNullException("request");

            // Setting return and cancel URLs.
            var context = controller.Request.RequestContext;
            UrlHelper url = new UrlHelper(context);
            string urlBase = string.Format("{0}://{1}", context.HttpContext.Request.Url.Scheme, context.HttpContext.Request.Url.Authority);

            if (request.ReturnURL == null)
                request.ReturnURL = urlBase + url.Action(confirmAction);

            if (request.CancelURL == null)
                request.CancelURL = urlBase + url.Action(cancelAction);
        }

        /// <summary>
        /// Set express checkout 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static PayPalSetExpressCheckoutResult SetExpressCheckout(this Controller controller, PayPalSetExpressCheckoutOperation operation, string confirmAction = "PayPalConfirm", string cancelAction = "PayPalCancel")
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            if (operation == null)
                throw new ArgumentNullException("request");

            var operationToUse = operation;

            if (string.IsNullOrWhiteSpace(operationToUse.CancelURL) || string.IsNullOrWhiteSpace(operationToUse.ReturnURL))
            {
                // Need to clone to avoid side effect on passed object.
                if ((object)operationToUse == operation)
                    operationToUse = operation.Clone();

                SetupExpressCheckoutUrls(controller, operationToUse, confirmAction, cancelAction);
            }

            // Calling the PayPal API, and returning the result.
            var api = new PayPalApiConfigurable();
            var response = api.SetExpressCheckout(operationToUse);
            return response;
        }

        public static PayPalDoExpressCheckoutPaymentResult DoExpressCheckoutPayment(this Controller controller, PayPalDoExpressCheckoutPaymentOperation operation)
        {
            // Calling the PayPal API, and returning the result.
            var api = new PayPalApiConfigurable();
            var response = api.DoExpressCheckoutPayment(operation);
            return response;
        }

        /// <summary>
        /// Redirects the user to the PayPal Express Checkout page, to review the operation details,
        /// fill some other informations, select payment option, and approve the payment.
        /// </summary>
        /// <param name="controller">Controller that is going to do the Redirect.</param>
        /// <param name="checkoutResponse">Response of the SetExpressCheckout method, that must be called before this one.</param>
        /// <returns>Returns a RedirectResult, that must be returned from the action method, to do the redirect.</returns>
        public static RedirectResult RedirectToCheckout(this Controller controller, PayPalSetExpressCheckoutResult checkoutResponse)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            if (checkoutResponse == null)
                throw new ArgumentNullException("checkoutResponse");

            if (!(checkoutResponse is PayPalApi.PayPalSetExpressCheckoutApiResult))
                throw new ArgumentException("checkoutResponse was not created by a PayPalApi object.", "checkoutResponse");

            var api = ((PayPalApi.PayPalSetExpressCheckoutApiResult)checkoutResponse).Api;
            var url = api.EndPoints.GetExpressCheckoutRedirectUrl(checkoutResponse.Token);
            return redirectFunc(controller, url);
        }
    }
}
