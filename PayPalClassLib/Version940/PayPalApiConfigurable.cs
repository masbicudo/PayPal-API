using System;
using System.Linq.Expressions;

namespace PayPal.Version940
{
    public class PayPalApiConfigurable : PayPalApi
    {
        private readonly IPayPalApiSettings configurations;

        public PayPalApiConfigurable(IPayPalApiSettings configurations)
        {
            this.configurations = configurations;

            // Getting settings ApiType.
            switch (configurations.ApiType.ToLowerInvariant())
            {
                case "sandbox":
                    base.ApiEnvironmentType = ApiEnvironmentType.Sandbox;
                    break;
                case "production":
                    base.ApiEnvironmentType = ApiEnvironmentType.Production;
                    break;
                default:
                    var msgSetConfig = configurations.MessageToChangeProperty("ApiType");
                    throw new Exception(string.Format("Invalid 'ApiType' value. {0}", msgSetConfig));
            }
        }

        internal static Exception ExceptionForCannotChangeProperty(Expression<Func<object>> exprProp, Type typeToCreate, string msg)
        {
            var propName = ExpressionHelper.PropertyInfoOf(exprProp).Name;
            var typeName = typeToCreate.Name;
            if (!string.IsNullOrEmpty(msg))
                return new Exception(string.Format("Cannot set the value of '{0}'. "
                    + "Create a new '{1}' object; "
                    + " == or == {2}.", propName, typeName, msg));

            return new Exception(string.Format("Cannot set the value of '{0}'. "
                + "Create a new '{1}' object.", propName, typeName));
        }

        public override ApiEnvironmentType ApiEnvironmentType
        {
            get { return base.ApiEnvironmentType; }
            set { throw PayPalApiConfigurable.ExceptionForCannotChangeProperty(() => this.ApiEnvironmentType, typeof(PayPalApi), this.configurations.MessageToChangeProperty("ApiType")); }
        }

        public override PayPalSetExpressCheckoutResult SetExpressCheckout(PayPalSetExpressCheckoutOperation operation)
        {
            var operationToUse = operation;

            SetupCredential(operation, ref operationToUse);

            SetupDefaultCurrencyCode(operation, ref operationToUse);

            // If LocaleCode is undefined then load it from the configuration file.
            if (operationToUse.LocaleCode != LocaleCode.Undefined)
            {
                if (operationToUse != operation)
                    operationToUse = operation.Clone();

                var locale = this.configurations.LocalCode;
                LocaleCode outValue;
                if (Enum.TryParse(locale, out outValue))
                    operationToUse.LocaleCode = outValue;
            }

            // Calling the API.
            return base.SetExpressCheckout(operationToUse);
        }

        public override PayPalDoExpressCheckoutPaymentResult DoExpressCheckoutPayment(PayPalDoExpressCheckoutPaymentOperation operation)
        {
            var operationToUse = operation;

            SetupCredential(operation, ref operationToUse);

            SetupDefaultCurrencyCode(operation, ref operationToUse);

            return base.DoExpressCheckoutPayment(operationToUse);
        }

        private void SetupCredential<T>(IPayPalCloneable<T> operation, ref T operationToUse)
            where T : PaypalOperation
        {
            // If API credential is not filled, then create a credential from the configuration file.
            if (operationToUse.Credential == null)
            {
                if (operationToUse != operation)
                    operationToUse = operation.Clone();

                SetupCredential(operationToUse);
            }
        }

        public void SetupCredential(PaypalOperation operation)
        {
            var user = this.configurations.PayPalUser;
            var password = this.configurations.PayPalPassword;
            var signature = this.configurations.PayPalSignature;

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(signature))
                operation.Credential = new PayPalSignatureCredential
                {
                    ApiUserName = user,
                    ApiPassword = password,
                    ApiSignature = signature,
                };

            if (operation.Credential == null)
                throw new Exception("Unsupported credential configuration.");
        }

        private void SetupDefaultCurrencyCode<T>(IPayPalCloneable<T> operation, ref T operationToUse)
            where T : PayPalExpressCheckoutOperation
        {
            // If API DefaultCurrency is not filled, then get the DefaultCurrency from the configuration file.
            if (operationToUse.DefaultCurrencyCode == CurrencyCode.Undefined)
            {
                if (operationToUse != operation)
                    operationToUse = operation.Clone();

                SetupDefaultCurrencyCode<T>(operationToUse);
            }
        }

        public void SetupDefaultCurrencyCode<T>(T operation)
            where T : PayPalExpressCheckoutOperation
        {
            var currencyCodeStr = this.configurations.CurrencyCode;

            CurrencyCode currencyCode;
            if (!string.IsNullOrWhiteSpace(currencyCodeStr))
                if (!Enum.TryParse<CurrencyCode>(currencyCodeStr, out currencyCode))
                    throw new Exception(string.Format("Unsupported DefaultCurrency value in config file: '{0}'.", currencyCodeStr));
        }
    }
}
