using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace PayPal.Version940
{
    public class PayPalApi
    {
        // reference:
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/howto_api_reference
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_PreviousAPIVersionsNVP
        // http://www.phpkode.com/source/s/paypal-payments-pro/samples/DoExpressCheckoutPayment.php
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_SetExpressCheckout

        // PDFs:
        // https://www.x.com/developers/paypal/development-and-integration-guides

        // Online API testing tools:
        // http://www.shopsandbox.de/api/ec/

        // Button generator:
        // pt-BR: https://www.paypal-brasil.com.br/logocenter/conversao_vendas.html

        public PayPalApi()
        {
            this.EndPoints = new PayPalApiEndPoints();
        }

        /// <summary>
        /// Sets up the end-point URL's used to make requests.
        /// </summary>
        public PayPalApiEndPoints EndPoints
        {
            get { return this.endPoints; }
            set
            {
                var old = this.endPoints;
                this.endPoints = null;
                if (old != null) old.Api = null;
                this.endPoints = value;
                if (value != null) value.Api = this;
            }
        }
        PayPalApiEndPoints endPoints;

        public virtual ApiEnvironmentType ApiEnvironmentType { get; set; }

        public virtual PayPalSetExpressCheckoutResult SetExpressCheckout(PayPalSetExpressCheckoutOperation operation)
        {
            // reference:
            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_ECGettingStarted
            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_testing_SBWPTesting
            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_testing_SBTestTools

            // sample code:
            // http://stackoverflow.com/questions/8452563/paypal-api-request-with-mvc3
            // https://www.paypal-brasil.com.br/x/tutoriais/integracao-avancada-com-express-checkout-em-c/
            // https://github.com/paypalxbrasil/ExpressCheckoutAdvancedFeatures/blob/master/ExpressCheckoutAdvancedFeatures/Controllers/HomeController.cs

            return ApiCall<PayPalSetExpressCheckoutApiResult>(operation);
        }

        public virtual PayPalDoExpressCheckoutPaymentResult DoExpressCheckoutPayment(PayPalDoExpressCheckoutPaymentOperation operation)
        {
            // reference:
            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_DoExpressCheckoutPayment

            // sample code:
            // None!

            return ApiCall<PayPalDoExpressCheckoutPaymentApiResult>(operation);
        }

        protected T ApiCall<T>(PaypalOperation operation)
            where T : IPayPalApiModel, new()
        {
            // validating arguments
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.Credential == null)
                throw new Exception("Must fill in the Credential property of operation.");

            // informations to build request
            var url = this.EndPoints.GetApiEndPoint(operation.Credential.CredentialType, ApiProtocol.NVP);
            var nvp = operation.ToNameValueCollection();
            byte[] byteArray = GetPostData(nvp);

            // building request with the above informations
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;

            // sending data
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(byteArray, 0, byteArray.Length);
            }

            // receiving data
            using (WebResponse webResponse = webRequest.GetResponse())
            using (Stream responseStream = webResponse.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseFromServer = reader.ReadToEnd();

                // creating result object from response
                var nvc = HttpUtility.ParseQueryString(responseFromServer);
                var result = new T();
                result.Api = this;
                nvc.LoadToPayPalModelObject(result);
                return result;
            }
        }

        private static byte[] GetPostData(NameValueCollection nvp)
        {
            StringBuilder sb = new StringBuilder(nvp.Count * 100);

            for (int it = 0, t = nvp.Count; it < t; ++it)
            {
                string key = nvp.GetKey(it);

                sb.Append(key);
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(nvp.Get(key)) + "&");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        protected interface IPayPalApiModel : IPayPalModel
        {
            PayPalApi Api { get; set; }
        }

        protected internal class PayPalSetExpressCheckoutApiResult : PayPalSetExpressCheckoutResult, IPayPalApiModel
        {
            public PayPalApi Api { get; set; }
        }

        protected internal class PayPalDoExpressCheckoutPaymentApiResult : PayPalDoExpressCheckoutPaymentResult, IPayPalApiModel
        {
            public PayPalApi Api { get; set; }
        }
    }

    public class PayPalApiEndPoints
    {
        public PayPalApi Api { get; internal set; }

        public const string SignatureApiUrl = "https://{1}.{0}/{2}";

        public const string RedirectToUrl = "https://www.{0}/webscr?{1}";

        public const string Signature = "api-3t";
        public const string Certificate = "api";

        public const string Sandbox = "sandbox.paypal.com";
        public const string Production = "paypal.com";

        public const string NVP = "nvp";
        public const string SOAP = "2.0";

        public string GetApiEndPoint(ApiCredentialType apiCredentialType, ApiProtocol apiProtocol)
        {
            return this.GetApiEndPoint(this.Api.ApiEnvironmentType, apiCredentialType, apiProtocol);
        }

        protected virtual string GetApiEndPoint(ApiEnvironmentType apiEnvironment, ApiCredentialType apiCredentialType, ApiProtocol apiProtocol)
        {
            if (apiEnvironment != ApiEnvironmentType.Sandbox && apiEnvironment != ApiEnvironmentType.Production)
                throw new Exception(string.Format("Invalid value of apiEnvironment: {0}", apiEnvironment));

            if (apiCredentialType != ApiCredentialType.Signature && apiCredentialType != ApiCredentialType.Certificate)
                throw new Exception(string.Format("Invalid value of apiCredentialType: {0}", apiCredentialType));

            if (apiProtocol != ApiProtocol.NVP && apiProtocol != ApiProtocol.SOAP)
                throw new Exception(string.Format("Invalid value of apiProtocol: {0}", apiProtocol));

            return string.Format(SignatureApiUrl,
                apiEnvironment == ApiEnvironmentType.Sandbox ? Sandbox : Production,
                apiCredentialType == ApiCredentialType.Signature ? Signature : Certificate,
                apiProtocol == ApiProtocol.NVP ? NVP : SOAP);
        }

        public string GetExpressCheckoutRedirectUrl(string token)
        {
            return this.GetExpressCheckoutRedirectUrl(this.Api.ApiEnvironmentType, token);
        }

        protected virtual string GetExpressCheckoutRedirectUrl(ApiEnvironmentType apiEnvironment, string token)
        {
            return string.Format(RedirectToUrl,
                apiEnvironment == ApiEnvironmentType.Sandbox ? Sandbox : Production,
                string.Format("cmd=_express-checkout&token={0}", token));
        }
    }

    public enum ApiProtocol
    {
        /// <summary>
        /// Name value pair HTTP/HTTPS POST based protocol.
        /// </summary>
        NVP,

        /// <summary>
        /// Simple Object Access Protocol.
        /// </summary>
        SOAP,
    }

    public enum ApiCredentialType
    {
        /// <summary>
        /// Credential that is verified by a signature sent over HTTPS protocol.
        /// </summary>
        Signature,

        /// <summary>
        /// Credential that is verified by using a certificate and a private key to encript data.
        /// </summary>
        Certificate,
    }

    public enum ApiEnvironmentType
    {
        /// <summary>
        /// Production is the environment where the application work with real money and users.
        /// </summary>
        Production,

        /// <summary>
        /// Sandbox is a testing environment, where you can test your application using only
        /// play-money, and fake users.
        /// </summary>
        Sandbox,
    }
}
