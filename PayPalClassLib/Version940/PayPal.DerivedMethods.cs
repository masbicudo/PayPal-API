using System;
using System.Configuration;
using System.Linq.Expressions;

namespace PayPal.Version940
{
    public class PayPalApiConfigurable : PayPalApi
    {
        const string cannotChangePropertyMsg = "Cannot set the value of '{0}'. "
                    + "Create a new '{1}' object, "
                    + "or change the appSettings section of the config file: '{2}'.";

        internal static Exception ExceptionForCannotChangeProperty(Expression<Func<object>> exprProp, Type typeToCreate, string appSettingName)
        {
            var propName = ExpressionHelper.PropertyInfoOf(exprProp).Name;
            var typeName = typeToCreate.Name;
            return new Exception(string.Format(cannotChangePropertyMsg, propName, typeName, appSettingName));
        }

        public PayPalApiConfigurable(string settingsToUse = null)
        {
            this.SettingsToUse = settingsToUse ?? ConfigurationManager.AppSettings["PayPal:SettingsToUse"];

            // Getting settings ApiType.
            var configKey = string.Format("PayPal:{0}.ApiType", this.SettingsToUse);
            var apiType = ConfigurationManager.AppSettings[configKey];
            switch (apiType.ToLowerInvariant())
            {
                case "sandbox":
                    base.ApiEnvironmentType = ApiEnvironmentType.Sandbox;
                    break;
                case "production":
                    base.ApiEnvironmentType = ApiEnvironmentType.Production;
                    break;
                default: throw new Exception(string.Format("appSettings value not found: {0}", configKey));
            }
        }

        public string SettingsToUse { get; private set; }

        public override ApiEnvironmentType ApiEnvironmentType
        {
            get { return base.ApiEnvironmentType; }
            set { throw PayPalApiConfigurable.ExceptionForCannotChangeProperty(() => this.ApiEnvironmentType, typeof(PayPalApi), this.SettingsToUse); }
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

                var locale = ConfigurationManager.AppSettings["PayPal:LocalCode"];
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

                SetupCredential<T>(operationToUse);
            }
        }

        public void SetupCredential<T>(T operation)
            where T : PaypalOperation
        {
            var user = ConfigurationManager.AppSettings[string.Format("PayPal:{0}.PayPalUser", this.SettingsToUse)];
            var password = ConfigurationManager.AppSettings[string.Format("PayPal:{0}.PayPalPassword", this.SettingsToUse)];
            var signature = ConfigurationManager.AppSettings[string.Format("PayPal:{0}.PayPalSignature", this.SettingsToUse)];

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
            var globalCurrencyCode = ConfigurationManager.AppSettings[string.Format("PayPal:CurrencyCode", this.SettingsToUse)];
            var userCurrencyCode = ConfigurationManager.AppSettings[string.Format("PayPal:{0}.CurrencyCode", this.SettingsToUse)];

            var currencyCodeStr = userCurrencyCode ?? globalCurrencyCode;

            CurrencyCode currencyCode;
            if (!string.IsNullOrWhiteSpace(currencyCodeStr))
                if (!Enum.TryParse<CurrencyCode>(currencyCodeStr, out currencyCode))
                    throw new Exception(string.Format("Unsupported DefaultCurrency value in config file: '{0}'.", currencyCodeStr));
        }
    }
}
