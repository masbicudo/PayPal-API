using System.Collections.Generic;
using System.Configuration;

namespace PayPal.Version940
{
    public class PayPalApiSettingsFromConfigurationManager : IPayPalApiSettings
    {
        private string settingsToUse;

        public static class PropertyNames
        {
            public static readonly string SettingsToUse = "SettingsToUse";
            public static readonly string PayPalUser = "PayPalUser";
            public static readonly string PayPalPassword = "PayPalPassword";
            public static readonly string PayPalSignature = "PayPalSignature";
            public static readonly string ApiType = "ApiType";
            public static readonly string LocalCode = "LocalCode";
            public static readonly string CurrencyCode = "CurrencyCode";
        }

        public PayPalApiSettingsFromConfigurationManager(string settingsToUse = null)
        {
            this.settingsToUse = settingsToUse ?? this.ReadConfig(this.GetConfigKeyName(PropertyNames.SettingsToUse, null));
        }

        protected virtual string ReadConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        protected virtual string GetConfigKeyName(string key, string group)
        {
            if (string.IsNullOrEmpty(group))
                return string.Format("PayPal:{0}", key);
            return string.Format("PayPal:{0}.{1}", group, key);
        }

        public virtual string SettingsToUse
        {
            get { return settingsToUse; }
        }

        public virtual string MessageToChangeProperty(string propertyName)
        {
            var dicProps = new Dictionary<string, string>
                {
                    { PropertyNames.CurrencyCode, this.GetConfigKeyName(PropertyNames.CurrencyCode, null) + " or " + this.GetConfigKeyName(PropertyNames.CurrencyCode, this.SettingsToUse) },
                    { PropertyNames.PayPalUser, this.GetConfigKeyName(PropertyNames.PayPalUser, this.SettingsToUse) },
                    { PropertyNames.PayPalPassword, this.GetConfigKeyName(PropertyNames.PayPalPassword, this.SettingsToUse) },
                    { PropertyNames.PayPalSignature, this.GetConfigKeyName(PropertyNames.PayPalSignature, this.SettingsToUse) },
                    { PropertyNames.ApiType, this.GetConfigKeyName(PropertyNames.ApiType, this.SettingsToUse) },
                    { PropertyNames.SettingsToUse, this.GetConfigKeyName(PropertyNames.SettingsToUse, null) },
                    { PropertyNames.LocalCode, this.GetConfigKeyName(PropertyNames.LocalCode, null) },
                };

            string configKey;
            if (dicProps.TryGetValue(propertyName, out configKey))
                return string.Format("Change the property {0} by setting the appSettings key {1} in your config file.", propertyName, configKey);

            return null;
        }

        public virtual string CurrencyCode
        {
            get
            {
                var globalCurrencyCode = this.ReadConfig(this.GetConfigKeyName(PropertyNames.CurrencyCode, null));
                var userCurrencyCode = this.ReadConfig(this.GetConfigKeyName(PropertyNames.CurrencyCode, this.SettingsToUse));
                return userCurrencyCode ?? globalCurrencyCode;
            }
        }

        public virtual string PayPalUser
        {
            get
            {
                var user = this.ReadConfig(this.GetConfigKeyName(PropertyNames.PayPalUser, this.SettingsToUse));
                return user;
            }
        }

        public virtual string PayPalPassword
        {
            get
            {
                var password = this.ReadConfig(this.GetConfigKeyName(PropertyNames.PayPalPassword, this.SettingsToUse));
                return password;
            }
        }

        public virtual string PayPalSignature
        {
            get
            {
                var signature = this.ReadConfig(this.GetConfigKeyName(PropertyNames.PayPalSignature, this.SettingsToUse));
                return signature;
            }
        }

        public virtual string ApiType
        {
            get
            {
                var configKey = this.GetConfigKeyName(PropertyNames.ApiType, this.SettingsToUse);
                var apiType = this.ReadConfig(configKey);
                return apiType;
            }
        }

        public virtual string LocalCode
        {
            get
            {
                var locale = this.ReadConfig(this.GetConfigKeyName(PropertyNames.LocalCode, null));
                return locale;
            }
        }
    }
}