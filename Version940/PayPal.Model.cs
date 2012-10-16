using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

// TODO: Replace MemberwiseClone by a better clonner.
// TODO: Create recurring payments object.
// TODO: Permitir usar API com Certificado digital.
// TODO: Implement variable Default value for MAXAMT field: see method DefaultMaximumAmount.
// TODO: Implement validation on all model objects.

// DONE: Comment everything involved with recurring payments, in the ExpressCheckout objects.
// DONE: Comment everything involved with digital goods, in the ExpressCheckout objects.

namespace PayPal.Version940
{
    public enum PaymentActionCode
    {
        [StringValue("")]
        Undefined,

        None, // came from SDK
        Authorization,
        Sale,
        Order,
    }

    public enum AcknowledgementStatus
    {
        [StringValue("")]
        Undefined,

        /// <summary>
        /// Successful operation.
        /// </summary>
        Success,

        /// <summary>
        /// Successful operation; however, there are messages returned in the response that you should examine.
        /// </summary>
        SuccessWithWarning,

        PartialSuccess, // came from SDK

        Warning, // came from SDK

        /// <summary>
        /// Operation failed; the response also contains one or more error messages explaining the failure.
        /// </summary>
        Failure,

        /// <summary>
        /// Operation failed and that there are messages returned in the response that you should examine.
        /// </summary>
        FailureWithWarning,
    }

    public enum CurrencyCode
    {
        [StringValue("")]
        Undefined,

        [StringValue("AUD")]
        Australian_Dollar,

        [StringValue("BRL")]
        Brazilian_Real,

        [StringValue("CAD")]
        Canadian_Dollar,

        [StringValue("CZK")]
        Czech_Koruna,

        [StringValue("DKK")]
        Danish_Krone,

        [StringValue("EUR")]
        Euro,

        [StringValue("HKD")]
        HongKong_Dollar,

        [StringValue("HUF")]
        Hungarian_Forint,

        [StringValue("ILS")]
        Israeli_NewSheqel,

        [StringValue("JPY")]
        Japan_Yen,

        [StringValue("MYR")]
        Malaysian_Ringgit,

        [StringValue("MXN")]
        Mexican_Peso,

        [StringValue("NOK")]
        Norwegian_Krone,

        [StringValue("NZD")]
        New_Zealand_Dollar,

        [StringValue("PHP")]
        Philippine_Peso,

        [StringValue("PLN")]
        Polish_Zloty,

        [StringValue("GBP")]
        Pound_Sterling,

        [StringValue("SGD")]
        Singapore_Dollar,

        [StringValue("SEK")]
        Swedish_Krona,

        [StringValue("CHF")]
        Swiss_Franc,

        [StringValue("TWD")]
        Taiwan_NewDollar,

        [StringValue("THB")]
        Tai_Baht,

        [StringValue("TRY")]
        Turkish_Lira,

        [StringValue("USD")]
        UnitedStates_Dollar,

        Default = CurrencyCode.UnitedStates_Dollar,
    }

    public enum LocaleCode
    {
        [StringValue("")]
        Undefined,

        [StringValue("AU")]
        Australia,

        [StringValue("AT")]
        Austria,

        [StringValue("BE")]
        Belgium,

        [StringValue("BR")]
        Brazil,

        [StringValue("CA")]
        Canada,

        [StringValue("CH")]
        Switzerland,

        [StringValue("CN")]
        China,

        [StringValue("DE")]
        Germany,

        [StringValue("ES")]
        Spain,

        [StringValue("GB")]
        UnitedKingdom,

        [StringValue("FR")]
        France,

        [StringValue("IT")]
        Italy,

        [StringValue("NL")]
        Netherlands,

        [StringValue("PL")]
        Poland,

        [StringValue("PT")]
        Portugal,

        [StringValue("RU")]
        Russia,

        [StringValue("US")]
        UnitedStates,

        [StringValue("da_DK")]
        Danish,

        [StringValue("he_IL")]
        Hebrew,

        [StringValue("id_ID")]
        Indonesian,

        [StringValue("jp_JP")]
        Japanese,

        [StringValue("no_NO")]
        Norwegian,

        [StringValue("pt_BR")]
        Brazilian_Portuguese,

        [StringValue("ru_RU")]
        Russian,

        [StringValue("sv_SE")]
        Sweedish,

        [StringValue("th_TH")]
        Thai,

        [StringValue("tr_TR")]
        Turkish,

        [StringValue("zh_CN")]
        SimplifiedChinese,

        [StringValue("zh_HK")]
        HongKong_TraditionalChinese,

        [StringValue("zh_TW")]
        Taiwan_TraditionalChinese,

        Default = LocaleCode.UnitedStates,
    }

    public enum ItemCategory
    {
        [StringValue("")]
        Undefined,
        Digital,
        Physical,
    }

    public enum ChannelType
    {
        [StringValue("")]
        Undefined,
        Merchant,
        eBayItem,
    }

    public enum LandingPage
    {
        [StringValue("")]
        Undefined,
        Billing,
        Login,
    }

    public enum SolutionType
    {
        [StringValue("")]
        Undefined,
        Sole,
        Mark,
    }

    public enum BillingCode
    {
        [StringValue("")]
        Undefined,

        MerchantInitiatedBilling,
        RecurringPayments,
        MerchantInitiatedBillingSingleAgreement,
        ChannelInitiatedBilling
    }

    public enum MerchantPullPaymentCode
    {
        [StringValue("")]
        Undefined,

        Any,
        InstantOnly,
        EcheckOnly
    }

    public enum AutoBill
    {
        [StringValue("")]
        Undefined,

        NoAutoBill,
        AddToNextBilling
    }

    public enum CreditCardType
    {
        [StringValue("")]
        Undefined,

        Visa,
        MasterCard,
        Discover,
        Amex,
        Switch,
        Solo,
        Maestro
    }

    public enum PayPalUserStatusCode
    {
        [StringValue("")]
        Undefined,

        [StringValue("verified")]
        Verified,

        [StringValue("unverified")]
        Unverified
    }

    public enum BillingPeriod
    {
        [StringValue("")]
        Undefined,

        NoBillingPeriodType,
        Day,
        Week,
        SemiMonth,
        Month,
        Year
    }

    public enum FailedPaymentAction
    {
        [StringValue("")]
        Undefined,

        CancelOnFailure,
        ContinueOnFailure
    }

    public enum RecurringPaymentsProfileStatus
    {
        [StringValue("")]
        Undefined,

        ActiveProfile,
        PendingProfile,
        CancelledProfile,
        ExpiredProfile,
        SuspendedProfile
    }

    public enum SeverityCode
    {
        Error,
        Warning,
        PartialSuccess,
    }

    public interface IPayPalCloneable<T>
    {
        T Clone();
    }

    internal interface IPayPalParentable
    {
        object Parent { get; set; }
    }

    internal static class PayPalParentableHelper
    {
        public static T SetProperty<T>(object parent, ref T field, T value)
            where T : class, IPayPalParentable
        {
            if (value != null && value.Parent != null)
                throw new ArgumentException(PayPalResources.ObjectAlreadyAttachedToAnotherOwner(), "value");

            // Keep old value, we'll need it.
            var old = field;

            // Set value.
            field = value;

            // Detaching old object.
            if (old != null)
                old.Parent = null;

            // Attaching new object.
            if (value != null)
                value.Parent = parent;

            return old;
        }

        public static TAncestor FindAncestor<TAncestor>(this IPayPalParentable parentable)
        {
            var parent = parentable.Parent;
            while (parent != null)
            {
                if (parent is TAncestor)
                    return (TAncestor)parent;
                var node = parent as IPayPalParentable;
                parent = node == null ? null : node.Parent;
            }
            return default(TAncestor);
        }
    }

    public interface IPayPalModel
    {
    }

    internal interface IPayPalValues
    {
        NameValueCollection Values { get; set; }
    }

    public static class PayPalObjectExtensions
    {
        public static NameValueCollection ToNameValueCollection(this IPayPalModel obj)
        {
            var result = NameValueConversionCore.Save(obj);
            var objWithValues = obj as IPayPalValues;
            if (objWithValues != null)
                objWithValues.Values = result;
            return result;
        }
    }

    public static class PayPalNameValueCollectionExtensions
    {
        public static T LoadToPayPalModelType<T>(this NameValueCollection nvc)
            where T : IPayPalModel, new()
        {
            var result = NameValueConversionCore.Load<T>(nvc);
            var objWithValues = result as IPayPalValues;
            if (objWithValues != null)
                objWithValues.Values = nvc;
            return result;
        }

        public static void LoadToPayPalModelObject(this NameValueCollection nvc, IPayPalModel target)
        {
            NameValueConversionCore.LoadTo(nvc, target);
            var objWithValues = target as IPayPalValues;
            if (objWithValues != null)
                objWithValues.Values = nvc;
        }
    }

    public class PayPalList<T> : Collection<T>, IPayPalModel, IPayPalParentable
      where T : class
    {
        /// <summary>
        /// The object to which this list is attatched to.
        /// </summary>
        public object Parent { get { return (this as IPayPalParentable).Parent; } }
        object IPayPalParentable.Parent { get; set; }

        internal delegate bool ListChangeFunc(PayPalList<T> sender, int index, T item);
        internal event ListChangeFunc InsertItemEvent;
        internal event ListChangeFunc SetItemEvent;
        internal event ListChangeFunc RemoveItemEvent;
        internal event ListChangeFunc ClearItemsEvent;

        protected override void InsertItem(int index, T item)
        {
            if (this.InsertItemEvent == null || this.InsertItemEvent(this, index, item))
            {
                // Insert item.
                base.InsertItem(index, item);

                // Attach new object.
                var newItem = item as IPayPalParentable;
                if (newItem != null)
                    newItem.Parent = this;
            }
        }

        protected override void ClearItems()
        {
            if (this.ClearItemsEvent == null || this.ClearItemsEvent(this, 0, default(T)))
            {
                // Keep old values, we'll need them.
                var oldParentables = this.OfType<IPayPalParentable>().ToArray();

                // Clear collection.
                base.ClearItems();

                // Detaching old objects.
                foreach (var eachParentable in oldParentables)
                    eachParentable.Parent = null;
            }
        }

        protected override void RemoveItem(int index)
        {
            if (this.RemoveItemEvent == null || this.RemoveItemEvent(this, index, default(T)))
            {
                // Keep old value, we'll need it.
                var parentable = this[index] as IPayPalParentable;

                // Remove item.
                base.RemoveItem(index);

                // Detaching old object.
                if (parentable != null)
                    parentable.Parent = null;
            }
        }

        protected override void SetItem(int index, T item)
        {
            if (this.SetItemEvent == null || this.SetItemEvent(this, index, item))
            {
                // Keep old value, we'll need it.
                var oldItem = this[index] as IPayPalParentable;

                // Set value.
                base.SetItem(index, item);

                // Detaching old object.
                if (oldItem != null)
                    oldItem.Parent = null;

                // Attaching new object.
                var newItem = item as IPayPalParentable;
                if (newItem != null)
                    newItem.Parent = this;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class PayPal000Credential : IPayPalModel
    {
        /// <summary>
        /// Required PayPal API user name (this is not a regular PayPal user name).
        /// </summary>
        /// <remarks>
        /// </remarks>
        [NameValue(Name = "USER", WriteDefault = true)]
        public virtual string ApiUserName { get; set; }

        /// <summary>
        /// Required API password.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [NameValue(Name = "PWD", WriteDefault = true)]
        public virtual string ApiPassword { get; set; }

        public abstract ApiCredentialType CredentialType { get; }
    }

    public abstract class PayPalBasicCredential : PayPal000Credential
    {
    }

    public class PayPalCertificateCredential : PayPalBasicCredential
    {
        public override ApiCredentialType CredentialType
        {
            get { return ApiCredentialType.Certificate; }
        }
    }

    public class PayPalSignatureCredential : PayPalBasicCredential
    {
        /// <summary>
        /// Required security signature.
        /// </summary>
        [NameValue(Name = "SIGNATURE", WriteDefault = true)]
        public virtual string ApiSignature { get; set; }

        public override ApiCredentialType CredentialType
        {
            get { return ApiCredentialType.Signature; }
        }
    }

    public abstract class PayPalAppIdCredential : PayPal000Credential
    {
        [NameValue(Name = "APPID", WriteDefault = true)]
        public virtual string ApiApplicationId { get; set; }
    }

    public class PayPalCertificateAndAppIdCredential : PayPalAppIdCredential
    {
        public override ApiCredentialType CredentialType
        {
            get { return ApiCredentialType.Certificate; }
        }
    }

    public class PayPalSignatureAndAppIdCredential : PayPalAppIdCredential
    {
        /// <summary>
        /// Required security signature.
        /// </summary>
        [NameValue(Name = "SIGNATURE", WriteDefault = true)]
        public virtual string ApiSignature { get; set; }

        public override ApiCredentialType CredentialType
        {
            get { return ApiCredentialType.Signature; }
        }
    }

    public abstract class PaypalOperation : IPayPalModel, IPayPalValues
    {
        [NameValue(Name = "METHOD", SaveOrder = -2, WriteDefault = true)]
        public abstract string Method { get; }

        [NameValue(SaveOrder = -1)]
        [TypeDecisionByName(1, typeof(PayPalSignatureCredential), "SIGNATURE")]
        [TypeDecisionDefault(2, typeof(PayPalCertificateCredential))]
        public virtual PayPalBasicCredential Credential { get; set; }

        /// <summary>
        /// The version of the Api that this object is to be used with.
        /// </summary>
        [NameValue(Name = "VERSION")]
        public virtual string ApiVersion
        {
            // latest version, look for "ns:version"): https://www.paypalobjects.com/wsdl/PayPalSvc.wsdl
            get { return "94.0"; }
        }

        /// <summary>
        /// Stores the values saved/loaded during the last save or load operation.
        /// </summary>
        public NameValueCollection Values
        {
            get { return this.lastValues; }
        }

        NameValueCollection IPayPalValues.Values
        {
            get { return this.lastValues; }
            set { this.lastValues = value; }
        }
        NameValueCollection lastValues = new NameValueCollection();
    }

    /// <summary>
    /// Common Response Fields from a PayPal API call.
    /// </summary>
    /// <remarks>
    /// <h2>Common Response Fields</h2>
    /// <p>
    ///     The PayPal API always returns common fields in addition to fields that are specific to
    ///     the requested PayPal API operation.</p>
    /// <h2>Error Responses</h2>
    /// <p>
    ///     If the ACK value is not Success, API response fields may not be returned. An error
    ///     response has the following general format.</p>
    /// <p>
    ///     Additional pass-through information may appear in the L_ERRORPARAMIDn and
    ///     L_ERRORPARAMVALUEn fields.</p>
    /// <h2>Logging API Operations</h2>
    /// <p>
    ///     You should log basic information from the request and response messages of each PayPal
    ///     API operation you execute. You must log the Correlation ID from the response message,
    ///     which identifies the API operation to PayPal and which must be provided to Merchant
    ///     Technical Support if you need their assistance with a specific transaction.</p>
    /// <p>
    ///     All responses to PayPal API operations contain information that may be useful for
    ///     debugging purposes. In addition to logging the Correlation ID from the response
    ///     message, you can log other information, such as the transaction ID and timestamp, to
    ///     enable you to review a transaction on the PayPal website or through the API. You could
    ///     implement a scheme that logs the entire request and response in a “verbose” mode;
    ///     however, you should never log the password from a request.</p>
    /// </remarks>
    public class PayPalResult : IPayPalModel, IPayPalValues
    {
        // reference: https://cms.paypal.com/en/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_NVPAPIOverview
        // Describes all these fields, including the ones inside PayPalError class.

        /// <summary>
        /// Acknowledgement status.
        /// </summary>
        /// <remarks>
        /// Acknowledgement status, which is one of the following values:
        /// - Success indicates a successful operation.
        /// - SuccessWithWarning indicates a successful operation; however, there are
        /// messages returned in the response that you should examine.
        /// - Failure indicates the operation failed; the response also contains one or more error
        /// messages explaining the failure.
        /// - FailureWithWarning indicates that the operation failed and that there are
        /// messages returned in the response that you should examine.
        /// </remarks>
        [NameValue(Name = "ACK")]
        public AcknowledgementStatus Status { get; set; }

        /// <summary>
        /// Correlation ID, which uniquely identifies the transaction to PayPal.
        /// </summary>
        [NameValue(Name = "CORRELATIONID")]
        public string CorrelationID { get; set; }

        /// <summary>
        /// The date and time that the requested API operation was performed.
        /// </summary>
        [NameValue(Name = "TIMESTAMP")]
        public string TimeStamp { get; set; }

        /// <summary>
        /// The version of the API.
        /// </summary>
        [NameValue(Name = "VERSION")]
        public string Version { get; set; }

        /// <summary>
        /// The sub-version of the API.
        /// </summary>
        [NameValue(Name = "BUILD")]
        public string Build { get; set; }

        /// <summary>
        /// Errors or warnings.
        /// </summary>
        [NameValue(KeyOrIndexName = "ErrIndex", NameRegex = @"^L_(ERRORCODE|(SHORT|LONG)MESSAGE)(?<ErrIndex>\d+)$")]
        public List<PayPalError> Errors { get; set; }

        /// <summary>
        /// Stores the values saved/loaded during the last save or load operation.
        /// </summary>
        public NameValueCollection Values
        {
            get { return this.lastValues; }
        }

        NameValueCollection IPayPalValues.Values
        {
            get { return this.lastValues; }
            set { this.lastValues = value; }
        }
        NameValueCollection lastValues = new NameValueCollection();
    }

    /// <summary>
    /// Shipping information to send to PayPal, for each payment.
    /// </summary>
    public class PayPalPaymentShippingInfo : IPayPalModel
    {
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOCITY")]
        public string CityName { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOCOUNTRYCODE")]
        public string CountryCode { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOPHONENUM")]
        public string Phone { get; set; }

        /// <summary>
        /// Person’s name associated with this shipping address.
        /// </summary>
        /// <remarks>
        ///  <p>Person’s name associated with this shipping address. It is required if using a shipping address.
        ///     You can specify up to 10 payments, where n is a digit between 0 and 9, inclusive.</p>
        ///  <p>Character length and limitations: 32 single-byte characters</p>
        ///  <p>SHIPTONAME is deprecated since version 63.0. Use PAYMENTREQUEST_0_SHIPTONAME instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTONAME")]
        public string ShipToName { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOSTATE")]
        public string StateOrProvince { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOSTREET")]
        public string Street1 { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOSTREET2")]
        public string Street2 { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPTOZIP")]
        public string PostalCode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PayPalPaymentRequestBase : IPayPalParentable, IPayPalModel
    {
        public PayPalPaymentRequestBase()
        {
            this.Items = new PayPalList<PayPalPaymentRequestItem>();
        }

        /// <summary>
        /// The object to which this list is attatched to.
        /// </summary>
        public object Parent { get { return (this as IPayPalParentable).Parent; } }
        object IPayPalParentable.Parent { get; set; }

        [NameValue(KeyOrIndexName = "Index", NameRegex = @"^L_PAYMENTREQUEST_{PaymentIndex}_\w+?(?<Index>\d+)$", SaveOrder = 1)]
        public PayPalList<PayPalPaymentRequestItem> Items
        {
            get { return this.item; }
            set
            {
                // Validating the list.
                if (value != null)
                {
                    var ancestor = this.FindAncestor<PayPalList<PayPalPaymentRequest>>();
                    if (ancestor != null)
                        if (ancestor.Count > 1 && value.Any(x => x.Category == ItemCategory.Digital))
                            throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                }

                var old = PayPalParentableHelper.SetProperty(this, ref this.item, value);
                var evt = new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                if (old != null)
                {
                    old.InsertItemEvent -= new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                    old.SetItemEvent -= new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                }
                if (value != null)
                {
                    value.InsertItemEvent += new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                    value.SetItemEvent += new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                }
            }
        }
        PayPalList<PayPalPaymentRequestItem> item;

        bool InsertOrSetItemEvent(PayPalList<PayPalPaymentRequestItem> sender, int index, PayPalPaymentRequestItem item)
        {
            var ancestor = this.FindAncestor<PayPalList<PayPalPaymentRequest>>();
            if (ancestor != null)
                if (ancestor.Count > 1 && item.Category == ItemCategory.Digital)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());

            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PayPalBasicPaymentRequest : PayPalPaymentRequestBase
    {
        public PayPalBasicPaymentRequest()
        {
            this.Items = new PayPalList<PayPalPaymentRequestItem>();
        }

        [NameValue(KeyOrIndexName = "Index", NameRegex = @"^L_PAYMENTREQUEST_{PaymentIndex}_\w+?(?<Index>\d+)$", SaveOrder = 1)]
        public PayPalList<PayPalPaymentRequestItem> Items
        {
            get { return this.item; }
            set
            {
                // Validating the list.
                if (value != null)
                {
                    var ancestor = this.FindAncestor<PayPalList<PayPalPaymentRequest>>();
                    if (ancestor != null)
                        if (ancestor.Count > 1 && value.Any(x => x.Category == ItemCategory.Digital))
                            throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                }

                var old = PayPalParentableHelper.SetProperty(this, ref this.item, value);
                var evt = new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                if (old != null)
                {
                    old.InsertItemEvent -= new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                    old.SetItemEvent -= new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                }
                if (value != null)
                {
                    value.InsertItemEvent += new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                    value.SetItemEvent += new PayPalList<PayPalPaymentRequestItem>.ListChangeFunc(InsertOrSetItemEvent);
                }
            }
        }
        PayPalList<PayPalPaymentRequestItem> item;

        bool InsertOrSetItemEvent(PayPalList<PayPalPaymentRequestItem> sender, int index, PayPalPaymentRequestItem item)
        {
            var ancestor = this.FindAncestor<PayPalList<PayPalPaymentRequest>>();
            if (ancestor != null)
                if (ancestor.Count > 1 && item.Category == ItemCategory.Digital)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());

            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PayPalPaymentRequest : PayPalPaymentRequestBase
    {
        public PayPalPaymentRequest()
        {
            this.Action = PaymentActionCode.Sale;
        }

        /// <summary>
        /// Shipping information to send to PayPal.
        /// </summary>
        [NameValue(NameRegex = @"^PAYMENTREQUEST_(?<PaymentIndex>\d+)_SHIP.+$")]
        public PayPalPaymentShippingInfo Shipping { get; set; }

        /// <summary>
        /// How you want to obtain payment.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_PAYMENTACTION</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">PAYMENTACTION</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      How you want to obtain payment. When implementing parallel payments, this field
        ///      is required and must be set to <samp class="codeph">Order</samp>.
        ///      When implementing digital goods, this field is required and must be set to
        ///      <samp class="codeph">Sale</samp>. You can specify up to 10 payments, where n is
        ///      a digit between 0 and 9, inclusive; except for digital goods, which supports single
        ///      payments only. If the transaction does not include a one-time purchase, this field
        ///      is ignored. It is one of the following values:</p>
        ///  <br />
        ///  <ul>
        ///      <li><p><samp class="codeph">Sale</samp>
        ///          – This is a final sale for which you are requesting payment (default).</p></li>
        ///      <li><p><samp class="codeph">Authorization</samp>
        ///          – This payment is a basic authorization subject to settlement with PayPal Authorization
        ///          and Capture.</p></li>
        ///      <li><p><samp class="codeph">Order</samp>
        ///          – This payment is an order authorization subject to settlement with PayPal Authorization
        ///          and Capture.</p></li>
        ///  </ul>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          You cannot set this field to <samp class="codeph">Sale</samp> in
        ///          <samp class="codeph">SetExpressCheckout</samp> request and then change the value to
        ///          <samp class="codeph">Authorization</samp> or <samp class="codeph">Order</samp> in the
        ///          <samp class="codeph">DoExpressCheckoutPayment</samp> request. If you set the field to
        ///          <samp class="codeph">Authorization</samp> or
        ///          <samp class="codeph">Order</samp> in
        ///          <samp class="codeph">SetExpressCheckout</samp>, you may set the field to
        ///          <samp class="codeph">Sale</samp>.</p>
        ///  </div>
        ///  <p>Character length and limitations: Up to 13 single-byte alphabetic characters</p>
        ///  <p>
        ///      <samp class="codeph">PAYMENTACTION</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_PAYMENTACTION</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_PAYMENTACTION")]
        public PaymentActionCode Action { get; set; }

        /// <summary>
        /// Total cost of the transaction to the buyer.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_AMT</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">AMT</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Required)</em> Total cost of the transaction to the buyer. If shipping cost
        ///      and tax charges are known, include them in this value. If not, this value should
        ///      be the current sub-total of the order. If the transaction includes one or more one-time
        ///      purchases, this field must be equal to the sum of the purchases. Set this field
        ///      to 0 if the transaction does not include a one-time purchase such as when you set
        ///      up a billing agreement for a recurring payment that is not immediately charged.
        ///      When the field is set to 0, purchase-specific fields are ignored. You can specify
        ///      up to 10 payments, where n is a digit between 0 and 9, inclusive; except for digital
        ///      goods, which supports single payments only.</p>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <p>
        ///      <samp class="codeph">AMT</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_AMT</samp> instead.
        ///  </p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_AMT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal Amount
        {
            get
            {
                return this.HandlingAmount ?? 0m
                    + this.InsuranceAmount ?? 0m
                    + this.ItemAmount ?? 0m
                    + this.ShippingAmount ?? 0m
                    + this.ShippingDiscountAmount ?? 0m
                    + this.TaxAmount ?? 0m;
            }
        }

        /// <summary>
        /// A 3-character currency code (default is USD).
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_CURRENCYCODE</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">CURRENCYCODE</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> A 3-character currency code (default is USD). You can specify
        ///      up to 10 payments, where n is a digit between 0 and 9, inclusive; except for digital
        ///      goods, which supports single payments only.</p>
        ///  <p>
        ///      <samp class="codeph">CURRENCYCODE</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_CURRENCYCODE</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_CURRENCYCODE", Default = CurrencyCode.Default, WriteDefault = true)]
        public CurrencyCode CurrencyCode
        {
            get
            {
                // If the property value is undefined, return the ancestor DefaultCurrencyCode.
                // Note: this causes a side-effect, when inserting a payment with undefined value
                //  in an ExpressCheckout object, the value is going to change automatically.
                if (this.currencyCode == CurrencyCode.Undefined)
                {
                    var ancestor = this.FindAncestor<PayPalExpressCheckoutOperation>();
                    if (ancestor != null)
                        return ancestor.DefaultCurrencyCode;
                }

                return this.currencyCode;
            }
            set
            {
                // Cannot allow a no-effect set... that is when the setter is called but the value don't change.
                // Happens when a default value exists, and you try to change the value of this property to undefined,
                // without any effect, because undefined maps to the default value.
                if (value == CurrencyCode.Undefined)
                {
                    var ancestor = this.FindAncestor<PayPalExpressCheckoutOperation>();
                    if (ancestor != null && ancestor.DefaultCurrencyCode != CurrencyCode.Undefined)
                        throw new Exception(PayPalResources.CannotChangePropertyToUndefined("CurrencyCode", ancestor.DefaultCurrencyCode));
                }

                this.currencyCode = value;
            }
        }
        CurrencyCode currencyCode;

        /// <summary>
        /// A free-form field for your own use.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_CUSTOM</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">CUSTOM</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> A free-form field for your own use. You can specify up to 10
        ///      payments, where n is a digit between 0 and 9, inclusive.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          The value you specify is available only if the transaction includes a purchase.
        ///          This field is ignored if you set up a billing agreement for a recurring payment
        ///          that is not immediately charged.
        ///      </p>
        ///  </div>
        ///  <p>Character length and limitations: 256 single-byte alphanumeric characters</p>
        ///  <p>
        ///      <samp class="codeph">CUSTOM</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_CUSTOM</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_CUSTOM")]
        [StringLength(255)]
        public string Custom { get; set; }

        /// <summary>
        /// Description of items the buyer is purchasing.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_DESC</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">DESC</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Description of items the buyer is purchasing. You can specify
        ///      up to 10 payments, where n is a digit between 0 and 9, inclusive; except for digital
        ///      goods, which supports single payments only.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          The value you specify is available only if the transaction includes a purchase.
        ///          This field is ignored if you set up a billing agreement for a recurring payment
        ///          that is not immediately charged.
        ///      </p>
        ///  </div>
        ///  <p>Character length and limitations: 127 single-byte alphanumeric characters</p>
        ///  <p>
        ///      <samp class="codeph">DESC</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_DESC</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_DESC")]
        public string Description { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_EMAIL")]
        //[EmailAddress]
        public string Email { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_HANDLINGAMT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? HandlingAmount { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_INSURANCEAMT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? InsuranceAmount { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_INSURANCEOPTIONOFFERED")]
        public bool? InsuranceOptionOffered { get; set; }

        /// <summary>
        /// Your own invoice or tracking number.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_INVNUM</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">INVNUM</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Your own invoice or tracking number. You can specify up to 10
        ///      payments, where n is a digit between 0 and 9, inclusive; except for digital goods,
        ///      which supports single payments only.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          The value you specify is available only if the transaction includes a purchase.
        ///          This field is ignored if you set up a billing agreement for a recurring payment
        ///          that is not immediately charged.
        ///      </p>
        ///  </div>
        ///  <p>Character length and limitations: 256 single-byte alphanumeric characters</p>
        ///  <p>
        ///      <samp class="codeph">INVNUM</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_INVNUM</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_INVNUM")]
        public string InvoiceNum { get; set; }

        /// <summary>
        /// Sum of cost of all items in this order.
        /// A value can be set only when Items collection is empty.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_ITEMAMT</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">ITEMAMT</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      Sum of cost of all items in this order. For digital goods, this field is required.
        ///      You can specify up to 10 payments, where n is a digit between 0 and 9, inclusive;
        ///      except for digital goods, which supports single payments only.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          <samp class="codeph">PAYMENTREQUEST_n_ITEMAMT</samp> is required if you specify
        ///          <samp class="codeph">L_PAYMENTREQUEST_n_AMTm</samp>.</p>
        ///  </div>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <p>
        ///      <samp class="codeph">ITEMAMT</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph"> PAYMENTREQUEST_0_ITEMAMT</samp> instead.</p>
        /// <masb>
        ///     I have seen this being used without specifying any items.
        ///     That is why this value can be set when there is no element in Items list.
        /// </masb>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_ITEMAMT", ValueFormat = "0.00")]
        [Range(-10000, 10000)]
        public decimal? ItemAmount
        {
            get
            {
                if (this.Items.Any())
                {
                    var itemsWithValue = this.Items.Where(x => x.Amount.HasValue);

                    if (!itemsWithValue.Any())
                        return null;

                    var sumAllItems = itemsWithValue.Sum(x => x.Amount.Value * x.Quantity);

                    return sumAllItems;
                }
                else
                {
                    return this.itemAmount;
                }
            }
            set
            {
                this.itemAmount = value;
            }
        }
        decimal? itemAmount;

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_NOTETEXT")]
        [StringLength(255)]
        public string NoteText { get; set; }

        /// <summary>
        /// Your URL for receiving Instant Payment Notification (IPN) about this transaction.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_NOTIFYURL</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">NOTIFYURL</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Your URL for receiving Instant Payment Notification (IPN) about
        ///      this transaction. If you do not specify this value in the request, the notification
        ///      URL from your Merchant Profile is used, if one exists.You can specify up to 10 payments,
        ///      where n is a digit between 0 and 9, inclusive; except for digital goods, which supports
        ///      single payments only.</p>
        ///  <div class="important">
        ///      <span class="importanttitle">Important:</span><p>
        ///          The notify URL applies only to
        ///          <samp class="codeph">DoExpressCheckoutPayment</samp>. This value is ignored when set in
        ///          <samp class="codeph">SetExpressCheckout</samp> or
        ///          <samp class="codeph">GetExpressCheckoutDetails</samp>.</p>
        ///  </div>
        ///  <p>Character length and limitations: 2,048 single-byte alphanumeric characters</p>
        ///  <p>
        ///      <samp class="codeph">NOTIFYURL</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_NOTIFYURL</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_NOTIFYURL")]
        public string NotifyUrl { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_PAYMENTREQUESTID")]
        public string PaymentRequestId { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPPINGAMT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? ShippingAmount { get; set; }

        /// <summary>
        /// Shipping discount for this order, specified as a negative number.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_SHIPDISCAMT</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">SHIPPINGDISCAMT</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Shipping discount for this order, specified as a negative number.
        ///      You can specify up to 10 payments, where n is a digit between 0 and 9, inclusive.</p>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).
        ///  </p>
        ///  <p>
        ///      <samp class="codeph">SHIPPINGDISCAMT</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_SHIPPINGDISCAMT</samp> instead.</p>
        /// <masb>
        ///     <p>
        ///         I have seen this being sent to PayPal with a '-' sign before the number, in a number of places:
        ///         <a href="https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&amp;content_ID=developer/e_howto_api_ECCustomizing">link</a>
        ///         <a href="https://cms.paypal.com/fr/cgi-bin/?cmd=_render-content&amp;content_ID=developer/e_howto_api_ECCustomizing">link</a>
        ///         The statement "Value is a positive number which ..." appears to be false.
        ///     </p>
        ///     <p>
        ///         Also, this seems to affect the total amount, negatively.
        ///     </p>
        /// </masb>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_SHIPDISCAMT", ValueFormat = "0.00")]
        [Range(-10000, 0)]
        public decimal? ShippingDiscountAmount { get; set; }

        /// <summary>
        /// Sum of tax for all items in this order.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: PAYMENTREQUEST_0_TAXAMT</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">TAXAMT</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Sum of tax for all items in this order. You can specify up to
        ///      10 payments, where n is a digit between 0 and 9, inclusive; except for digital goods,
        ///      which supports single payments only.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          <samp class="codeph">PAYMENTREQUEST_n_TAXAMT</samp> is required if you specify
        ///          <samp class="codeph">L_PAYMENTREQUEST_n_TAXAMTm</samp></p>
        ///  </div>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <p>
        ///      <samp class="codeph">TAXAMT</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">PAYMENTREQUEST_0_TAXAMT</samp> instead.</p>
        /// </remarks>
        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_TAXAMT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? TaxAmount { get; set; }

        [NameValue(Name = "PAYMENTREQUEST_{PaymentIndex}_TRANSACTIONID")]
        public string TransactionId { get; set; }

        #region Billing Agreement Details Type Fields
        /// <summary>
        /// Type of billing agreement.
        /// </summary>
        /// <remarks>
        ///   <p>
        ///     <em>(Required)</em> Type of billing agreement. For recurring payments, this field
        ///     must be set to <samp class="codeph">RecurringPayments</samp>. In this case, you can
        ///     specify up to ten billing agreements. Other defined values are not valid.</p>
        ///   <p>Type of billing agreement for reference transactions. You must have permission
        ///     from PayPal to use this field. This field must be set to one of the following values:</p>
        ///   <br />
        ///   <ul>
        ///     <li><p>
        ///         <samp class="codeph">MerchantInitiatedBilling</samp> - PayPal creates a billing
        ///         agreement for each transaction associated with buyer. You must specify version
        ///         54.0 or higher to use this option.</p></li>
        ///     <li><p>
        ///         <samp class="codeph">MerchantInitiatedBillingSingleAgreement</samp> - PayPal
        ///         creates a single billing agreement for all transactions associated with buyer.
        ///         Use this value unless you need per-transaction billing agreements. You must
        ///         specify version 58.0 or higher to use this option.</p></li>
        ///   </ul>
        /// </remarks>
        [NameValue(Name = "L_BILLINGTYPE{PaymentIndex}", Default = ChannelType.Undefined)]
        public BillingCode BillingType { get; set; }

        /// <summary>
        /// Description of goods or services associated with the billing agreement.
        /// </summary>
        /// <remarks>
        ///  <p>
        ///     Description of goods or services associated with the billing agreement. This field
        ///     is required for each recurring payment billing agreement. PayPal recommends that
        ///     the description contain a brief summary of the billing agreement terms and
        ///     conditions. For example, buyer is billed at “9.99 per month for 2 years”.</p>
        ///  <p>
        ///     Character length and limitations: 127 single-byte alphanumeric characters</p>
        /// </remarks>
        [NameValue(Name = "L_BILLINGAGREEMENTDESCRIPTION{PaymentIndex}")]
        [StringLength(127)]
        public string BillingAgreementDescription { get; set; }

        [NameValue(Name = "L_PAYMENTTYPE{PaymentIndex}")]
        [StringLength(127)]
        public MerchantPullPaymentCode PaymentType { get; set; }

        [NameValue(Name = "L_BILLINGAGREEMENTCUSTOM{PaymentIndex}")]
        [StringLength(256)]
        public string BillingAgreementCustom { get; set; }

        /// <summary>
        /// Hint property to calculate the MaximumAmount, when setting up a billing agreement
        /// for a recurring payment, or a non immediately charged payment.
        /// </summary>
        public decimal? PlannedAmount { get; set; }
        #endregion
    }

    public class PayPalPaymentRequestItem : IPayPalParentable, IValidatableObject, IPayPalModel
    {
        public PayPalPaymentRequestItem()
        {
            // Initial quantity is 1.
            this.Quantity = 1;
        }

        /// <summary>
        /// The object to which this list is attatched to.
        /// </summary>
        public object Parent { get { return (this as IPayPalParentable).Parent; } }
        object IPayPalParentable.Parent { get; set; }

        /// <summary>
        /// Item description.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_DESC0</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">L_DESCn</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional) </em>Item description. You can specify up to 10 payments, where n
        ///      is a digit between 0 and 9, inclusive, and m specifies the list item within the
        ///      payment; except for digital goods, which supports single payments only. These parameters
        ///      must be ordered sequentially beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_DESC0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_DESC1</samp>).</p>
        ///  <p>Character length and limitations: 127 single-byte characters</p>
        ///  <p>
        ///      This field is introduced in version 53.0.
        ///      <samp class="codeph">L_DESCn</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">L_PAYMENTREQUEST_0_DESCm</samp> instead.
        ///  </p>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_DESC{Index}")]
        public string Description { get; set; }

        /// <summary>
        /// Item name.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_NAME0</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">L_NAMEn</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      Item name. This field is required when
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORYm</samp>
        ///      is passed. You can specify up to 10 payments, where n is a digit between 0 and 9,
        ///      inclusive, and m specifies the list item within the payment; except for digital
        ///      goods, which supports single payments only. These parameters must be ordered sequentially
        ///      beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_NAME0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_NAME1</samp>).</p>
        ///  <p>Character length and limitations: 127 single-byte characters</p>
        ///  <p>
        ///      This field is introduced in version 53.0.
        ///      <samp class="codeph">L_NAMEn</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">L_PAYMENTREQUEST_0_NAMEm</samp> instead.
        ///  </p>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_NAME{Index}")]
        public string Name { get; set; }

        /// <summary>
        /// Item quantity.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_QTY0</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">L_QTYn</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      Item quantity. This field is required when
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORYm</samp>
        ///      is passed. For digital goods (<samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORYm=Digital</samp>),
        ///      this field is required. You can specify up to 10 payments, where n is a digit between
        ///      0 and 9, inclusive, and m specifies the list item within the payment; except for
        ///      digital goods, which only supports single payments. These parameters must be ordered
        ///      sequentially beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_QTY0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_QTY1</samp>).</p>
        ///  <p>Character length and limitations: Any positive integer</p>
        ///  <p>
        ///      This field is introduced in version 53.0.
        ///      <samp class="codeph">L_QTYn</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">L_PAYMENTREQUEST_0_QTYm</samp> instead.
        ///  </p>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_QTY{Index}")]
        public int? Quantity { get; set; }

        /// <summary>
        /// Cost of item.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_AMT0</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">L_AMTn</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      Cost of item. This field is required when
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORYm</samp> is passed.
        ///      You can specify up to 10 payments, where n is a digit between 0 and 9,
        ///      inclusive, and m specifies the list item within the payment; except for digital
        ///      goods, which supports single payments only. These parameters must be ordered sequentially
        ///      beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_AMT0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_AMT1</samp>).</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          If you specify a value for <samp class="codeph">L_PAYMENTREQUEST_n_AMTm</samp>,
        ///          you must specify a value for <samp class="codeph">PAYMENTREQUEST_n_ITEMAMT</samp>.</p>
        ///  </div>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <p>
        ///      This field is introduced in version 53.0.
        ///      <samp class="codeph">L_AMTn</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">L_PAYMENTREQUEST_0_AMTm</samp> instead.
        ///  </p>
        ///  <masb>
        ///     <h2>Discount:</h2>
        ///     <a href="https://cms.paypal.com/mx/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_ECCustomizing">
        ///         Customizing Express Checkout
        ///     </a>
        ///     <p>
        ///     Item unit price. PayPal calculates the product of the item unit price and item unit
        ///     quantity (below) in the Amount column of the cart review area. The item unit price
        ///     can be a positive or a negative value, but not 0. You may provide a negative value
        ///     to reflect a discount on an order, for example.
        ///     </p>
        ///  </masb>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_AMT{Index}", ValueFormat = "0.00")]
        [Range(-10000, 10000)]
        public decimal? Amount
        {
            get
            {
                if (this.Category != null)
                    return amount ?? 0.00m;
                return amount;
            }
            set { this.amount = value; }
        }
        decimal? amount;

        /// <summary>
        /// Item sales tax.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_TAXAMT0</h2>
        ///  <p><b></b></p>
        ///  <p><b><samp class="codeph">L_TAXAMTn</samp><em>(deprecated)</em></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Item sales tax. You can specify up to 10 payments, where n is
        ///      a digit between 0 and 9, inclusive, and m specifies the list item within the payment;
        ///      except for digital goods, which only supports single payments. These parameters
        ///      must be ordered sequentially beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_TAXAMT0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_TAXAMT1</samp>).</p>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <p>
        ///      <samp class="codeph">L_TAXAMTn</samp> is deprecated since version 63.0.
        ///      Use <samp class="codeph">L_PAYMENTREQUEST_0_TAXAMTm</samp> instead.
        ///  </p>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_TAXAMT{Index}", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Indicates whether an item is digital or physical.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: L_PAYMENTREQUEST_0_ITEMCATEGORY0</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      Indicates whether an item is digital or physical. For digital goods, this field
        ///      is required and must be set to <samp class="codeph">Digital</samp>.
        ///      You can specify up to 10 payments, where n is a digit between
        ///      0 and 9, inclusive, and m specifies the list item within the payment; except for
        ///      digital goods, which only supports single payments. These parameters must be ordered
        ///      sequentially beginning with 0 (for example
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORY0</samp>,
        ///      <samp class="codeph">L_PAYMENTREQUEST_n_ITEMCATEGORY1</samp>).
        ///      It is one of the following values:</p>
        ///  <br />
        ///  <ul>
        ///      <li><p><samp class="codeph">Digital</samp></p></li>
        ///      <li><p><samp class="codeph">Physical</samp></p></li>
        ///  </ul>
        ///  <p>This field is available since version 65.1.</p>
        /// </remarks>
        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMCATEGORY{Index}", Default = ItemCategory.Undefined)]
        public ItemCategory Category
        {
            get { return this.category; }
            set
            {
                {
                    var ancestor = this.FindAncestor<PayPalList<PayPalBasicPaymentRequest>>();

                    if (ancestor != null)
                        if (ancestor.Count > 1 && value == ItemCategory.Digital)
                            throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                }

                {
                    var ancestor = this.FindAncestor<PayPalList<PayPalPaymentRequest>>();

                    if (ancestor != null)
                        if (ancestor.Count > 1 && value == ItemCategory.Digital)
                            throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                }

                this.category = value;
            }
        }
        ItemCategory category;

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_NUMBER{Index}")]
        public string Number { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMURL{Index}")]
        public string Url { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMLENGTHVALUE{Index}")]
        public uint? Length { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMLENGTHUNIT{Index}")]
        public string LengthUnit { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMWEIGHTVALUE{Index}")]
        public uint? Weight { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMWEIGHTUNIT{Index}")]
        public string WeightUnit { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMWIDTHVALUE{Index}")]
        public uint? Width { get; set; }

        [NameValue(Name = "L_PAYMENTREQUEST_{PaymentIndex}_ITEMWIDTHUNIT{Index}")]
        public string WidthtUnit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            // Page describing all errors.
            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_errorcodes

            if (this.Category != ItemCategory.Undefined && (string.IsNullOrEmpty(this.Name) || this.Amount == null || this.Quantity == null))
                // Scenario: Merchant passes item category as either Digital or Physical but does not
                // also pass one of the following parameters: name, amount, or quantity.
                result.Add(new ValidationResult(
                    "Item name, amount and quantity are required if item category is provided. "
                    + "Error 10003: Missing argument.", new[] { "Category", "Name", "Amount", "Quantity" }));

            return result;
        }
    }

    /// <summary>
    /// Shipping options to send to PayPal.
    /// </summary>
    public class PayPalShippingOptions : IPayPalParentable, IPayPalModel
    {
        /// <summary>
        /// The object to which this list is attatched to.
        /// </summary>
        public object Parent { get { return (this as IPayPalParentable).Parent; } }
        object IPayPalParentable.Parent { get; set; }

        #region PayPal shipping options
        /// <summary>
        /// Determines whether or not PayPal displays shipping address fields on the PayPal pages.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: NOSHIPPING</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      Determines whether or not PayPal displays shipping address fields on the PayPal pages.
        ///      For digital goods, this field is required, and you must set it to 1. It is one of
        ///      the following values:</p>
        ///  <br />
        ///  <ul>
        ///      <li><p><samp class="codeph">0</samp> – PayPal displays the shipping address on the PayPal pages.</p></li>
        ///      <li><p><samp class="codeph">1</samp> – PayPal does not display shipping address fields whatsoever.</p></li>
        ///      <li><p><samp class="codeph">2</samp> – If you do not pass the shipping address, PayPal obtains it from the buyer’s account profile.</p></li>
        ///  </ul>
        ///  <p>Character length and limitations: 4 single-byte numeric characters</p>
        /// </remarks>
        [NameValue(Name = "NOSHIPPING")]
        public bool? NoShipping
        {
            get
            {
                var ancestor = this.FindAncestor<PayPalExpressCheckoutOperation>();
                if (ancestor != null)
                {
                    var items = ancestor.PaymentRequests.SelectMany(x => x.Items);
                    if (items.Any() && items.All(x => x.Category == ItemCategory.Digital))
                        return true;
                }

                return this.noShipping;
            }
            // No-effect set when parent ExpressCheckout object contains digital goods.
            // TODO: throw exception when value cannot be changed.
            set { this.noShipping = value; }
        }
        bool? noShipping;

        /// <summary>
        /// Determines whether or not the PayPal pages should display the shipping address set by you.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: ADDROVERRIDE</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      <em>(Optional)</em> Determines whether or not the PayPal pages should display the
        ///      shipping address set by you in this SetExpressCheckout request, not the shipping
        ///      address on file with PayPal for this buyer. Displaying the PayPal street address
        ///      on file does not allow the buyer to edit that address. It is one of the following
        ///      values:</p>
        ///  <br />
        ///  <ul>
        ///      <li><p><samp class="codeph">0</samp> – The PayPal pages should not display the shipping address.</p></li>
        ///      <li><p><samp class="codeph">1</samp> – The PayPal pages should display the shipping address.</p></li>
        ///  </ul>
        ///  <p>Character length and limitations: 1 single-byte numeric character</p>
        /// </remarks>
        [NameValue(Name = "ADDROVERRIDE")]
        public bool? AddressOverride { get; set; }

        /// <summary>
        /// Indicates whether or not you require the buyer’s shipping address be a confirmed address.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: REQCONFIRMSHIPPING</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      Indicates whether or not you require the buyer’s shipping address on file with PayPal
        ///      be a confirmed address. For digital goods, this field is required, and you must
        ///      set it to 0. It is one of the following values:
        ///  </p>
        ///  <br />
        ///  <ul>
        ///      <li><p><samp class="codeph">0</samp> – You do not require the buyer’s shipping address be a confirmed address.</p></li>
        ///      <li><p><samp class="codeph">1</samp> – You require the buyer’s shipping address be a confirmed address.</p></li>
        ///  </ul>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          Setting this field overrides the setting you specified in your Merchant Account
        ///          Profile.</p>
        ///  </div>
        ///  <p>Character length and limitations: 1 single-byte numeric character</p>
        ///  <table>
        ///  </table>
        /// </remarks>
        [NameValue(Name = "REQCONFIRMSHIPPING")]
        public bool? RequireConfirmShipping
        {
            get
            {
                var ancestor = this.FindAncestor<PayPalExpressCheckoutOperation>();
                if (ancestor != null)
                    if (ancestor.PaymentRequests.SelectMany(x => x.Items)
                        .All(x => x.Category == ItemCategory.Digital))
                    {
                        return false;
                    }

                return this.requireConfirmShipping;
            }
            set { this.requireConfirmShipping = value; }
        }
        bool? requireConfirmShipping;

        /// <summary>
        /// Same as AddressOverride.
        /// </summary>
        //[NameValue(Name = "ADDROVERRIDE")]
        public bool? ShippingAddressOverride
        {
            get { return this.AddressOverride; }
            set { this.AddressOverride = value; }
        }

        /// <summary>
        /// True when RequireConfirmShipping and not AddressOverride.
        /// </summary>
        //[NameValue(Name = "REQCONFIRMSHIPPING")]
        public bool RequireConfirmedAddress
        {
            get { return this.RequireConfirmShipping == true && this.AddressOverride != true; }
        }
        #endregion
    }

    /// <summary>
    /// Common base class for PayPalSetExpressCheckoutOperation and PayPalDoExpressCheckoutPaymentOperation classes.
    /// </summary>
    public abstract class PayPalExpressCheckoutOperation : PaypalOperation
    {
        public PayPalExpressCheckoutOperation()
        {
            this.PaymentRequests = new PayPalList<PayPalPaymentRequest>();
        }

        #region Payment information
        [NameValue(KeyOrIndexName = "PaymentIndex",
            NameRegex = @"^(PAYMENTREQUEST_(?<PaymentIndex>\d+)_\w+)|
                (L_PAYMENTREQUEST_(?<PaymentIndex>\d+)_\w+?\d+)|
                (L_BILLING(\w+?)|L_PAYMENTTYPE)(?<PaymentIndex>\d+)$")]
        public PayPalList<PayPalPaymentRequest> PaymentRequests
        {
            get { return this.paymentRequests; }
            set
            {
                // Validating the list.
                if (value != null)
                {
                    if (value.Count > 1 && value.SelectMany(x => x.Items).Any(x => x.Category == ItemCategory.Digital))
                        throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());

                    if (value.Count > 10)
                        throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
                }

                var old = PayPalParentableHelper.SetProperty(this, ref this.paymentRequests, value);

                var evtIns = new PayPalList<PayPalPaymentRequest>.ListChangeFunc(InsertItemEvent);
                if (old != null) old.InsertItemEvent -= evtIns;
                if (value != null) value.InsertItemEvent += evtIns;

                var evtSet = new PayPalList<PayPalPaymentRequest>.ListChangeFunc(SetItemEvent);
                if (old != null) old.SetItemEvent -= evtSet;
                if (value != null) value.SetItemEvent += evtSet;
            }
        }
        PayPalList<PayPalPaymentRequest> paymentRequests;

        bool InsertItemEvent(PayPalList<PayPalPaymentRequest> sender, int index, PayPalPaymentRequest item)
        {
            if (item == null)
                throw new ArgumentException(PayPalResources.PaymentListDoesNotSupportNulls(), "item");

            var preview = sender.ContinueWith(item).ToList();
            var allItems = preview.SelectMany(x => x.Items);
            bool hasAnyDigitalGood = allItems.Any(x => x.Category == ItemCategory.Digital);

            int limit = hasAnyDigitalGood ? 1 : 10;

            if (preview.Count > limit)
            {
                if (hasAnyDigitalGood)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                else
                    throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
            }

            return true;
        }

        bool SetItemEvent(PayPalList<PayPalPaymentRequest> sender, int index, PayPalPaymentRequest item)
        {
            if (item == null)
                throw new ArgumentException(PayPalResources.PaymentListDoesNotSupportNulls(), "item");

            var preview = sender.ToList();
            preview[index] = item;

            var allItems = preview.SelectMany(x => x.Items);
            bool hasAnyDigitalGood = allItems.Any(x => x.Category == ItemCategory.Digital);

            int limit = hasAnyDigitalGood ? 1 : 10;

            if (preview.Count > limit)
            {
                if (hasAnyDigitalGood)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                else
                    throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the default currency used for this ExpressCheckout operation.
        /// Any payment request that is inserted with an undefined CurrencyCode,
        /// will assume that the value is using this default currency.
        /// </summary>
        public CurrencyCode DefaultCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets an aggregated value of all the payment currencies.
        /// When multiple currencies are in use, then this returns CurrencyCode.Undefined.
        /// If a single currency is in use, it returns that currency.
        /// Setting this property changes all the currencies to a single one.
        /// </summary>
        public CurrencyCode AggregatedCurrencyCode
        {
            get
            {
                if (this.PaymentRequests == null)
                    return CurrencyCode.Undefined;

                var allCurrencies = this.PaymentRequests.Select(x => x.CurrencyCode);
                if (allCurrencies.Any() && allCurrencies.Skip(1).All(x => x == allCurrencies.First()))
                {
                    return allCurrencies.First();
                }

                return CurrencyCode.Undefined;
            }
            set
            {
                if (value == CurrencyCode.Undefined)
                {
                    var currentValue = this.AggregatedCurrencyCode;
                    if (currentValue != CurrencyCode.Undefined)
                        throw new Exception(PayPalResources.CannotChangePropertyToUndefined("AggregatedCurrencyCode", currentValue));
                }
                else if (this.PaymentRequests != null)
                    foreach (var eachPaymentRequest in this.PaymentRequests)
                        eachPaymentRequest.CurrencyCode = value;
            }
        }
        #endregion

        #region Gift
        [NameValue(Name = "GIFTRECEIPTENABLE")]
        public bool? GiftReceiptEnable { get; set; }

        [NameValue(Name = "GIFTWRAPAMOUNT", ValueFormat = "0.00")]
        [Range(0, 10000)]
        public decimal? GiftWrapAmount { get; set; }

        [NameValue(Name = "GIFTWRAPNAME")]
        public string GiftWrapName { get; set; }
        #endregion
    }

    public class PayPalSetExpressCheckoutOperation : PayPalExpressCheckoutOperation, IPayPalCloneable<PayPalSetExpressCheckoutOperation>
    {
        public PayPalSetExpressCheckoutOperation()
        {
            this.SurveyChoice = new List<string>();
            this.LocaleCode = LocaleCode.Default;
        }

        /// <summary>
        /// Method name: SetExpressCheckout.
        /// </summary>
        public override string Method
        {
            get { return "SetExpressCheckout"; }
        }

        #region Payment information
        /// <summary>
        /// The expected maximum total amount of the complete order, including shipping cost and tax charges.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: MAXAMT</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      <em>(Optional)</em> The expected maximum total amount of the complete order, including
        ///      shipping cost and tax charges. If the transaction includes one or more one-time
        ///      purchases, this field is ignored.</p>
        ///  <p>
        ///      For recurring payments, you should pass the expected average transaction amount
        ///      (default 25.00). PayPal uses this value to validate the buyer’s funding source.
        ///  </p>
        ///  <p>
        ///      Character length and limitations: Value is a positive number which cannot exceed
        ///      $10,000 USD in any currency. It includes no currency symbol. It must have 2 decimal
        ///      places, the decimal separator must be a period (.), and the optional thousands separator
        ///      must be a comma (,).</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          This field is required when implementing the Instant Update API callback. PayPal
        ///          recommends that the maximum total amount be slightly greater than the sum of the
        ///          line-item order details, tax, and the shipping options of greatest value.</p>
        ///  </div>
        /// </remarks>
        [NameValue(Name = "MAXAMT", ValueFormat = "0.00")]
        [Range(-10000, 10000)]
        public decimal? MaximumAmount
        {
            get
            {
                if (this.PaymentRequests != null)
                {
                    var recurring = this.PaymentRequests.Where(x => x.BillingType == BillingCode.RecurringPayments);
                    if (recurring.Any())
                        return recurring.Sum(x => Math.Max(x.Amount, x.PlannedAmount ?? 0.00m)) + 25.00m;
                    else
                        return this.PaymentRequests.Sum(x => x.Amount)
                            + (this.GiftWrapEnable == true ? this.GiftWrapAmount : 0.00m)
                            + 25.00m;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the default value for the MaximumAmount property,
        /// to be used by the NameValueConverter.
        /// </summary>
        /// <param name="obj">Instance to calculate the default MaximumAmount property for.</param>
        /// <returns>The default MaximumAmount property for the instance.</returns>
        public static decimal? DefaultMaximumAmount(PayPalExpressCheckoutOperation obj)
        {
            // The usage of this method is not yet implemented in the NameValueConverter.
            // It will never be called.

            if (obj.PaymentRequests != null)
            {
                var recurring = obj.PaymentRequests.Where(x => x.BillingType == BillingCode.RecurringPayments);
                if (recurring.Any())
                    return 25.00m;
            }

            return null;
        }
        #endregion

        #region Shipping options (influences what is shown on PayPal page)
        [NameValue(NameRegex = @"^ADDROVERRIDE|REQCONFIRMSHIPPING|NOSHIPPING$")]
        public PayPalShippingOptions ShippingOptions
        {
            get { return this.shipping; }
            set { PayPalParentableHelper.SetProperty(this, ref this.shipping, value); }
        }
        PayPalShippingOptions shipping;
        #endregion

        #region Customizations: Page fields
        /// <summary>
        /// Allows the user to leave a note.
        /// </summary>
        [NameValue(Name = "ALLOWNOTE")]
        public bool? AllowNote { get; set; }

        /// <summary>
        /// Allows user to enter e-mail for additional contact (promotions, etc.)
        /// </summary>
        [NameValue(Name = "BUYEREMAILOPTINENABLE")]
        public bool? BuyerEmailOptinEnable { get; set; }

        [NameValue(Name = "SURVEYENABLE")]
        public bool? SurveyEnable { get; set; }

        [NameValue(Name = "SURVEYQUESTION")]
        public string SurveyQuestion { get; set; }

        [NameValue(KeyOrIndexName = "Index", Name = "L_SURVEYCHOICE{Index}", NameRegex = @"^L_SURVEYCHOICE(?<Index>\d+)$")]
        public List<string> SurveyChoice { get; set; }
        #endregion

        #region Instant Update
        /// <summary>
        /// (Optional) URL to which the callback request from PayPal is sent. It must start with HTTPS for production integration. It can start with HTTPS or HTTP for sandbox testing.
        /// Character length and limitations: 1024 single-byte characters
        /// This field is available since version 53.0.
        /// </summary>
        [NameValue(Name = "CALLBACK")]
        public string Callback { get; set; }

        //#error Is this options or not?
        //#error What happens if value if left as 0.
        [NameValue(Name = "CALLBACKTIMEOUT")]
        [Range(1, 6)]
        public int CallbackTimeout { get; set; }

        /// <summary>
        /// Version of the callback API. This field is required when implementing the Instant Update Callback API. It must be set to 61.0 or a later version.
        /// This field is available since version 61.0.
        /// </summary>
        [NameValue(Name = "CALLBACKVERSION")]
        public string CallbackVersion { get; set; }
        #endregion

        #region Return URLs (for general usage)
        /// <summary>
        /// URL to which the buyer’s browser is returned after choosing to pay with PayPal.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: RETURNURL</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      <em>(Required)</em> URL to which the buyer’s browser is returned after choosing
        ///      to pay with PayPal. For digital goods, you must add JavaScript to this page to close
        ///      the in-context experience.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          PayPal recommends that the value be the final review page on which the buyer confirms
        ///          the order and payment or billing agreement.</p>
        ///  </div>
        ///  <p>Character length and limitations: 2048 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "RETURNURL")]
        public virtual string ReturnURL { get; set; }

        /// <summary>
        /// URL to which the buyer is returned if the buyer does not approve the use of PayPal to pay you.
        /// </summary>
        /// <remarks>
        ///  <h2>Description: CANCELURL</h2>
        ///  <p><b></b></p>
        ///  <p>
        ///      <em>(Required)</em> URL to which the buyer is returned if the buyer does not approve
        ///      the use of PayPal to pay you. For digital goods, you must add JavaScript to this
        ///      page to close the in-context experience.</p>
        ///  <div class="note">
        ///      <span class="notetitle">Note:</span><p>
        ///          PayPal recommends that the value be the original page on which the buyer chose to
        ///          pay with PayPal or establish a billing agreement.</p>
        ///  </div>
        ///  <p>Character length and limitations: 2048 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "CANCELURL")]
        public virtual string CancelURL { get; set; }
        #endregion

        #region Return URLs (Giropay and other Germany only services)
        [NameValue(Name = "GIROPAYCANCELURL")]
        public string GiropayCancelUrl { get; set; }

        [NameValue(Name = "GIROPAYSUCCESSURL")]
        public string GiropaySuccessUrl { get; set; }

        /// <summary>
        /// (Optional) The URL on the merchant site to transfer to after a bank transfer payment.
        /// NOTE:Use this field only if you are using giropay or bank transfer payment methods in Germany.
        /// </summary>
        [NameValue(Name = "BANKTXNPENDINGURL")]
        public string BankPendingUrl { get; set; }
        #endregion

        #region Gift
        [NameValue(Name = "GIFTMESSAGEENABLE")]
        public bool? GiftMessageEnable { get; set; }

        [NameValue(Name = "GIFTWRAPENABLE")]
        public bool? GiftWrapEnable { get; set; }
        #endregion

        #region Customizations: Page text
        /// <summary>
        /// (Optional) A label that overrides the business name in the PayPal account on the PayPal hosted checkout pages.
        /// Character length and limitations: 127 single-byte alphanumeric characters
        /// </summary>
        [NameValue(Name = "BRANDNAME")]
        public string BrandName { get; set; }
        #endregion

        #region Customizations: Page style
        [NameValue(Name = "HDRBACKCOLOR")]
        public string HeaderBackgroundColor { get; set; }

        [NameValue(Name = "HDRBORDERCOLOR")]
        public string HeaderBorderColor { get; set; }

        [NameValue(Name = "HDRIMG")]
        public string HeaderImage { get; set; }

        [NameValue(Name = "PAGESTYLE")]
        public string PageStyle { get; set; }

        [NameValue(Name = "PAYFLOWCOLOR")]
        public string PayFlowColor { get; set; }
        #endregion

        #region Billing Agreement Details Type Fields
        [NameValue(Name = "BILLINGTYPE", Default = BillingCode.Undefined)]
        public BillingCode BillingType
        {
            get { return this.billingType; }
            set
            {
                // Can only set this value for reference transactions.
                if (this.IsReferenceTransaction)
                {
                    if (value != BillingCode.MerchantInitiatedBilling && value != BillingCode.MerchantInitiatedBillingSingleAgreement)
                        throw new Exception("Invalid value.");
                }
                else if (value != BillingCode.Undefined)
                    throw new Exception("Invalid value.");

                this.billingType = value;
            }
        }
        BillingCode billingType;

        public bool IsReferenceTransaction { get; set; }
        #endregion

        #region Other
        [NameValue(Name = "LOCALECODE", Default = LocaleCode.Default, WriteDefault = true)]
        public LocaleCode LocaleCode { get; set; }

        [NameValue(Name = "CHANNELTYPE", Default = ChannelType.Undefined)]
        public ChannelType ChannelType { get; set; }

        [NameValue(Name = "CUSTOMERSERVICENUMBER")]
        [StringLength(16)]
        public string CustomerServiceNumber { get; set; }

        [NameValue(Name = "EMAIL")]
        //[EmailAddress]
        public string Email { get; set; }

        [NameValue(Name = "LANDINGPAGE", Default = LandingPage.Undefined)]
        public LandingPage LandingPage { get; set; }

        [NameValue(Name = "SOLUTIONTYPE", Default = SolutionType.Undefined)]
        public SolutionType SolutionType { get; set; }
        #endregion

        #region Cloning
        public PayPalSetExpressCheckoutOperation Clone()
        {
            return (PayPalSetExpressCheckoutOperation)this.MemberwiseClone();
        }
        #endregion
    }

    public class PayPalError : IPayPalModel
    {
        // reference: https://cms.paypal.com/en/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_NVPAPIOverview
        // Describes all these fields.

        [NameValue(Name = "L_ERRORCODE{ErrIndex}")]
        public int ErrorCode { get; set; }

        [NameValue(Name = "L_SHORTMESSAGE{ErrIndex}")]
        public string ShortMessage { get; set; }

        [NameValue(Name = "L_LONGMESSAGE{ErrIndex}")]
        public string LongMessage { get; set; }

        [NameValue(Name = "L_SEVERITYCODE{ErrIndex}")]
        public SeverityCode SeverityCode { get; set; }

        [NameValue(Name = "L_ERRORPARAMID{ErrIndex}")]
        public string ErrorParameterId { get; set; }

        [NameValue(Name = "L_ERRORPARAMVALUE{ErrIndex}")]
        public string ErrorParameterValue { get; set; }
    }

    public class PayPalSetExpressCheckoutResult : PayPalResult
    {
        /// <summary>
        /// A timestamped token by which you identify to PayPal that you are processing this
        /// payment with Express Checkout.
        /// </summary>
        /// <remarks>
        /// <p>
        ///     A timestamped token by which you identify to PayPal that you are processing this
        ///     payment with Express Checkout. The token expires after three hours. If you set the
        ///     token in the SetExpressCheckout request, the value of the token in the response is
        ///     identical to the value in the request.
        ///     Character length and limitations: 20 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "TOKEN")]
        public string Token { get; set; }
    }

    /// <summary>
    /// Incoming model from PayPal when the user confirms an ExpressCheckout operation,
    /// and is returned back to the original site.
    /// </summary>
    public class PayPalExpressCheckoutConfirmation : IPayPalModel
    {
        [NameValue(Name = "token")]
        public string Token { get; set; }

        [NameValue(Name = "PayerID")]
        public string PayerId { get; set; }
    }

    public class PayPalDoExpressCheckoutPaymentOperation : PayPalExpressCheckoutOperation, IPayPalCloneable<PayPalDoExpressCheckoutPaymentOperation>
    {
        public override string Method
        {
            get { return "DoExpressCheckoutPayment"; }
        }

        /// <summary>
        /// The timestamped token value that was returned in the SetExpressCheckout
        /// response and passed in the GetExpressCheckoutDetails request.
        /// </summary>
        /// <remarks>
        /// <p>
        ///     (Required) The timestamped token value that was returned in the SetExpressCheckout
        ///     response and passed in the GetExpressCheckoutDetails request.</p>
        /// <p>
        ///     Character length and limitations: 20 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "TOKEN")]
        public string Token { get; set; }

        /// <summary>
        /// Unique PayPal buyer account identification number as returned in the
        /// GetExpressCheckoutDetails response.
        /// </summary>
        /// <remarks>
        /// <p>
        ///     (Required) Unique PayPal buyer account identification number as returned in the
        ///     GetExpressCheckoutDetails response.</p>
        /// <p>
        ///     Character length and limitations: 13 single-byte alphanumeric characters</p>
        /// </remarks>
        [NameValue(Name = "PAYERID")]
        public string PayerId { get; set; }

        [NameValue(Name = "GIFTMESSAGE")]
        public string GiftMessage { get; set; }

        [NameValue(Name = "RETURNFMFDETAILS")]
        public bool ReturnFmfDetails { get; set; }
        #region Cloning
        public PayPalDoExpressCheckoutPaymentOperation Clone()
        {
            return (PayPalDoExpressCheckoutPaymentOperation)this.MemberwiseClone();
        }
        #endregion
    }

    public class PayPalDoExpressCheckoutPaymentResult : PayPalResult
    {
    }

    /// <summary>
    /// Shipping information to send to PayPal for the subscription.
    /// </summary>
    public class PayPalSubscriberShippingAddress : IPayPalModel
    {
        // reference:
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments

        #region Ship To Address Fields
        [NameValue(Name = "SHIPTOCITY")]
        [StringLength(40)]
        public string CityName { get; set; }

        // Note: in PayPal API there is a CountryName property that is not an enum,
        // and also Country property that is an enum.
        [NameValue(Name = "SHIPTOCOUNTRY")]
        [StringLength(2)]
        public string CountryCode { get; set; }

        [NameValue(Name = "SHIPTOPHONENUM")]
        [StringLength(20)]
        public string Phone { get; set; }

        /// <summary>
        /// Person’s name associated with this shipping address.
        /// </summary>
        /// <remarks>
        ///  <p>Person’s name associated with this shipping address. It is required if using a shipping address.
        ///     You can specify up to 10 payments, where n is a digit between 0 and 9, inclusive.</p>
        ///  <p>Character length and limitations: 32 single-byte characters</p>
        ///  <p>SHIPTONAME is deprecated since version 63.0. Use PAYMENTREQUEST_0_SHIPTONAME instead.</p>
        /// </remarks>
        [NameValue(Name = "SHIPTONAME")]
        [StringLength(32)]
        public string Name { get; set; }

        [NameValue(Name = "SHIPTOSTATE")]
        [StringLength(40)]
        public string StateOrProvince { get; set; }

        [NameValue(Name = "SHIPTOSTREET")]
        [StringLength(100)]
        public string Street1 { get; set; }

        [NameValue(Name = "SHIPTOSTREET2")]
        [StringLength(100)]
        public string Street2 { get; set; }

        [NameValue(Name = "SHIPTOZIP")]
        [StringLength(20)]
        public string PostalCode { get; set; }
        #endregion
    }

    public class PayPalPersonName : IPayPalModel
    {
        // reference:
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments

        #region Payer Name Fields
        /// <summary>
        /// Buyer’s salutation.
        /// </summary>
        /// <remarks>
        ///  <p>(Optional) Buyer’s salutation.</p>
        ///  <p>Character length and limitations: 20 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "SALUTATION")]
        [StringLength(20)]
        public string Salutation { get; set; }

        /// <summary>
        /// Buyer’s first name.
        /// </summary>
        /// <remarks>
        ///  <p>(Optional) Buyer’s first name.</p>
        ///  <p>Character length and limitations: 25 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "FIRSTNAME")]
        [StringLength(25)]
        public string FirstName { get; set; }

        /// <summary>
        /// Buyer’s middle name.
        /// </summary>
        /// <remarks>
        ///  <p>(Optional) Buyer’s middle name.</p>
        ///  <p>Character length and limitations: 25 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "MIDDLENAME")]
        [StringLength(25)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Buyer’s last name.
        /// </summary>
        /// <remarks>
        ///  <p>(Optional) Buyer’s last name.</p>
        ///  <p>Character length and limitations: 25 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "LASTNAME")]
        [StringLength(25)]
        public string LastName { get; set; }

        /// <summary>
        /// Buyer’s suffix.
        /// </summary>
        /// <remarks>
        ///  <p>(Optional) Buyer’s suffix.</p>
        ///  <p>Character length and limitations: 12 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "SUFFIX")]
        [StringLength(12)]
        public string Suffix { get; set; }
        #endregion
    }

    /// <summary>
    /// Address of the payer.
    /// </summary>
    public class PayPalPayerAddress : IPayPalModel
    {
        // reference:
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments

        #region Address Fields
        [NameValue(Name = "CITY")]
        public string CityName { get; set; }

        // Note: PayPal docs is crazy... they mention this field twice.
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments
        //[NameValue(Name = "COUNTRYCODE")]
        //public string PayerCountryCode { get; set; }

        // Note: PayPal docs is crazy... they mention this field twice.
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments
        // PayPal docs say SHIPTOPHONENUM... but they mention it twice, so I think it is really PHONENUM
        //[NameValue(Name = "SHIPTOPHONENUM")]
        //public string Phone { get; set; }

        [NameValue(Name = "STATE")]
        public string StateOrProvince { get; set; }

        [NameValue(Name = "STREET")]
        public string Street1 { get; set; }

        [NameValue(Name = "STREET2")]
        public string Street2 { get; set; }

        [NameValue(Name = "ZIP")]
        public string PostalCode { get; set; }
        #endregion
    }

    public class PayPalPayerInfo : IPayPalModel
    {
        // reference:
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments

        #region Payer Information Fields
        // Note: The property Payer is in the SOAP API.
        // So I guess that Payer is actually the e-mail,
        // as PayPal uses emails to identify members of PayPal.
        [NameValue(Name = "EMAIL")]
        public string Payer { get; set; }

        [NameValue(Name = "PAYERID")]
        [StringLength(13)]
        public string PayerID { get; set; }

        [NameValue(Name = "PAYERSTATUS")]
        public PayPalUserStatusCode PayerStatus { get; set; }

        // Note: PayPal docs is crazy... they mention this field twice.
        // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_r_CreateRecurringPayments
        [NameValue(Name = "COUNTRYCODE")]
        [StringLength(2)]
        public string CountryCode { get; set; }

        [NameValue(Name = "BUSINESS")]
        [StringLength(127)]
        public string PayerBusiness { get; set; }
        #endregion

        #region Payer Name Fields
        [NameValue]
        public PayPalPersonName PayerName { get; set; }
        #endregion

        #region Address Fields
        [NameValue]
        public PayPalPayerAddress Address { get; set; }
        #endregion

        // Missing fields:

        // Note: The property Payer is in the SOAP API.
        // I have not found any corresponding field.
        // There are other phone fields but they are already used by other properties in this object.
        //public string ContactPhone { get; set; }

        // Note: The property Payer is in the SOAP API.
        // I have not found any corresponding field.
        // There are other phone fields but they are already used by other properties in this object.
        // I found corresponding fields in the SetExpressCheckout API however.
        // This has to do with brazilian tax model.
        //public TaxIdDetailsType TaxIdDetails { get; set; }
    }

    public class PayPalCreditCardDetails
    {
        #region Credit Card Details Fields
        [NameValue(Name = "CREDITCARDTYPE")]
        public CreditCardType CreditCardType { get; set; }

        [NameValue(Name = "ACCT")]
        public string CreditCardNumber { get; set; }

        [NameValue(Name = "EXPDATE")]
        public string ExpirationDate
        {
            get { return string.Format("{0:00}{1:0000}", this.ExpMonth, this.ExpYear); }
            set
            {
                if (Regex.IsMatch(value, @"^(\d{6})?$"))
                    throw new ArgumentException("value must have 6 digits in the format MMYYYY, or be empty only when not using direct payments.", "value");

                if (string.IsNullOrEmpty(value))
                {
                    this.ExpMonth = null;
                    this.ExpYear = null;
                }
                else
                {
                    this.ExpMonth = int.Parse(value.Substring(0, 2));
                    this.ExpYear = int.Parse(value.Substring(2));
                }
            }
        }

        public int? ExpMonth
        {
            get { return this.expMonth; }
            set
            {
                if (value != null || value.Value < 1 || value.Value > 12)
                    throw new ArgumentException("Value of ExpMonth must be either null o between 1 and 12 inclusive.", "value");
                this.expMonth = value;
            }
        }
        int? expMonth;

        public int? ExpYear
        {
            get { return this.expYear; }
            set
            {
                if (value != null || value.Value < 1900 || value.Value > 2100)
                    throw new ArgumentException("Value of ExpYear must be either null or between 1900 and 2100.", "value");
                this.expYear = value;
            }
        }
        int? expYear;

        [NameValue(Name = "CVV2")]
        public string CVV2 { get; set; }

        [NameValue(Name = "STARTDATE")]
        public string StartDate
        {
            get { return string.Format("{0:00}{1:0000}", this.StartMonth, this.StartYear); }
            set
            {
                if (Regex.IsMatch(value, @"^(\d{6})?$"))
                    throw new ArgumentException("value must have 6 digits in the format MMYYYY.", "value");

                if (string.IsNullOrEmpty(value))
                {
                    this.StartMonth = null;
                    this.StartYear = null;
                }
                else
                {
                    this.StartMonth = int.Parse(value.Substring(0, 2));
                    this.StartYear = int.Parse(value.Substring(2));
                }
            }
        }

        public int? StartMonth
        {
            get { return this.startMonth; }
            set
            {
                if (value != null || value.Value < 1 || value.Value > 12)
                    throw new ArgumentException("Value of StartMonth must be either null o between 1 and 12 inclusive.", "value");
                this.startMonth = value;
            }
        }
        int? startMonth;

        public int? StartYear
        {
            get { return this.startYear; }
            set
            {
                if (value != null || value.Value < 1900 || value.Value > 2100)
                    throw new ArgumentException("Value of StartYear must be either null or between 1900 and 2100.", "value");
                this.startYear = value;
            }
        }
        int? startYear;

        [NameValue(Name = "ISSUENUMBER")]
        public string IssueNumber { get; set; }
        #endregion

        #region Payer Information Fields; Payer Name Fields; Address Fields
        [NameValue]
        public PayPalPayerInfo CardOwner { get; set; }
        #endregion
    }

    public class PayPalBillingPaymentPeriodDetails
    {
        #region Billing Period Details Fields
        [NameValue(Name = "BILLINGPERIOD")]
        public BillingPeriod BillingPeriod { get; set; }

        [NameValue(Name = "BILLINGFREQUENCY")]
        public int? BillingFrequency { get; set; }

        [NameValue(Name = "TOTALBILLINGCYCLES")]
        public int? TotalBillingCycles { get; set; }

        [NameValue(Name = "AMT")]
        public decimal? Amount { get; set; }
        #endregion
    }

    public class PayPalBillingTrialPeriodDetails
    {
        #region Billing Period Details Fields
        [NameValue(Name = "TRIALBILLINGPERIOD")]
        public BillingPeriod TrialBillingPeriod { get; set; }

        [NameValue(Name = "TRIALBILLINGFREQUENCY")]
        public int? TrialBillingFrequency { get; set; }

        [NameValue(Name = "TRIALTOTALBILLINGCYCLES")]
        public int? TrialTotalBillingCycles { get; set; }

        [NameValue(Name = "TRIALAMT")]
        public decimal? TrialAmount { get; set; }
        #endregion
    }

    public class PayPalScheduleDetails
    {
        #region Schedule Details Fields
        /// <summary>
        /// Description of the recurring payment.
        /// </summary>
        /// <remarks>
        ///  <p>
        ///  <em>(Required)</em> Description of the recurring payment.
        ///  </p>
        ///  <div class="note">
        ///  <span class="notetitle">Note:</span>
        ///  <p>
        ///      You must ensure that this field matches the corresponding billing agreement
        ///      description included in the <samp class="codeph">SetExpressCheckout</samp>
        ///      request.</p>
        ///  </div>
        ///  <p>Character length and limitations: 127 single-byte alphanumeric characters</p>
        /// </remarks>
        [NameValue(Name = "DESC", WriteDefault = true, EmptyIgnore = false)]
        public string Description { get; set; }

        /// <summary>
        /// Number of scheduled payments that can fail before the profile is automatically suspended.
        /// </summary>
        /// <remarks>
        ///  <p><em>(Optional)</em> Number of scheduled payments that can fail before the profile is
        ///     automatically suspended. An IPN message is sent to the merchant when the specified
        ///     number of failed payments is reached.</p>
        ///  <p>Character length and limitations: Number string representing an integer</p>
        /// </remarks>
        [NameValue(Name = "MAXFAILEDPAYMENTS")]
        public int? MaxFailedPayments { get; set; }

        /// <summary>
        /// Indicates whether you would like PayPal to automatically bill
        /// the outstanding balance amount in the next billing cycle.
        /// </summary>
        /// <remarks>
        ///  <p><em>(Optional)</em> Indicates whether you would like PayPal to automatically bill
        ///     the outstanding balance amount in the next billing cycle. The outstanding balance
        ///     is the total amount of any previously failed scheduled payments that have yet to be
        ///     successfully paid. It is one of the following values:</p>
        ///  <br />
        ///  <ul>
        ///     <li><p><samp class="codeph">NoAutoBill</samp> – PayPal does not automatically bill
        ///         the outstanding balance.</p></li>
        ///     <li><p><samp class="codeph">AddToNextBilling</samp> – PayPal automatically bills
        ///         the outstanding balance.</p></li></ul>
        /// </remarks>
        [NameValue(Name = "AUTOBILLOUTAMT")]
        public AutoBill AutoBillOutstandingAmount { get; set; }
        #endregion

        #region Billing Period Details Fields
        [NameValue]
        public PayPalBillingPaymentPeriodDetails PaymentPeriod { get; set; }

        [NameValue]
        public PayPalBillingTrialPeriodDetails TrialPeriod { get; set; }

        [NameValue(Name = "CURRENCYCODE")]
        public CurrencyCode CurrencyCode { get; set; }

        [NameValue(Name = "SHIPPINGAMT")]
        public decimal? ShippingAmount { get; set; }

        [NameValue(Name = "TAXAMT")]
        public decimal? TaxAmount { get; set; }
        #endregion

        #region Activation Details Fields
        [NameValue]
        public PayPalActivationDetails ActivationDetails { get; set; }
        #endregion
    }

    public class PayPalActivationDetails
    {
        #region Activation Details Fields
        [NameValue(Name = "INITAMT")]
        public string InitialAmount { get; set; }

        [NameValue(Name = "FAILEDINITAMTACTION")]
        public FailedPaymentAction FailedInitialAmountAction { get; set; }
        #endregion
    }

    public class PayPalRecurringPaymentsProfileDetails
    {
        #region Recurring Payments Profile Details Fields
        /// <summary>
        /// Full name of the person receiving the product or service paid for by the recurring payment.
        /// </summary>
        /// <remarks>
        /// <p>
        ///   (Optional) Full name of the person receiving the product or service paid for by the recurring payment.
        ///   If not present, the name in the buyer’s PayPal account is used.</p>
        /// <p>
        ///   Character length and limitations: 32 single-byte characters</p>
        /// </remarks>
        [NameValue(Name = "SUBSCRIBERNAME")]
        public string SubscriberName { get; set; }

        /// <summary>
        /// The date when billing for this profile begins.
        /// </summary>
        /// <remarks>
        /// <p>
        ///   (Required) The date when billing for this profile begins.</p>
        /// <p>
        ///   NOTE:The profile may take up to 24 hours for activation.</p>
        /// <p>
        ///   Character length and limitations: Must be a valid date, in UTC/GMT format.</p>
        /// </remarks>
        [NameValue(Name = "PROFILESTARTDATE")]
        public string BillingStartDate { get; set; }

        /// <summary>
        /// The merchant’s own unique reference or invoice number.
        /// </summary>
        /// <remarks>
        /// <p>
        ///   (Optional) The merchant’s own unique reference or invoice number.</p>
        /// <p>
        ///   Character length and limitations: 127 single-byte alphanumeric characters</p>
        /// </remarks>
        [NameValue(Name = "PROFILEREFERENCE")]
        public string ProfileReference { get; set; }
        #endregion

        #region Ship To Address Fields
        [NameValue]
        public PayPalSubscriberShippingAddress SubscriberShippingAddress { get; set; }
        #endregion
    }

    public class PayPalCreateRecurringPaymentsProfileOperation : PaypalOperation
    {
        public PayPalCreateRecurringPaymentsProfileOperation()
        {
            this.RecurringPaymentsProfileDetails = new PayPalRecurringPaymentsProfileDetails();
            this.ScheduleDetails = new PayPalScheduleDetails();
            this.CreditCard = new PayPalCreditCardDetails();
            this.PaymentRequests = new PayPalList<PayPalBasicPaymentRequest>();
        }

        #region CreateRecurringPaymentsProfile Request Fields
        public override string Method
        {
            get { return "CreateRecurringPaymentsProfile"; }
        }

        /// <summary>
        /// A timestamped token, the value of which was returned in the response to the first call to SetExpressCheckout.
        /// </summary>
        /// <remarks>
        /// <p>
        ///   A timestamped token, the value of which was returned in the response to the first call to SetExpressCheckout.
        ///   You can also use the token returned in the SetCustomerBillingAgreement response.
        ///   Either this token or a credit card number is required. If you include both token and credit card number,
        ///   the token is used and credit card number is ignored Call CreateRecurringPaymentsProfile once for each billing
        ///   agreement included in SetExpressCheckout request and use the same token for each call.
        ///   Each CreateRecurringPaymentsProfile request creates a single recurring payments profile.</p>
        /// <p>
        ///   NOTE:Tokens expire after approximately 3 hours.</p>
        /// </remarks>
        [NameValue(Name = "TOKEN")]
        public string Token { get; set; }
        #endregion

        #region Recurring Payments Profile Details Fields; Ship To Address Fields
        [NameValue]
        public PayPalRecurringPaymentsProfileDetails RecurringPaymentsProfileDetails { get; set; }
        #endregion

        #region Schedule Details Fields; Billing Period Details Fields; Activation Details Fields
        [NameValue]
        public PayPalScheduleDetails ScheduleDetails { get; set; }
        #endregion

        #region Credit Card Details Fields; Payer Information Fields; Payer Name Fields; Address Fields
        [NameValue]
        public PayPalCreditCardDetails CreditCard { get; set; }
        #endregion

        #region Payment Details Item Fields
        [NameValue(KeyOrIndexName = "PaymentIndex",
            NameRegex = @"^(L_PAYMENTREQUEST_(?<PaymentIndex>\d+)_\w+?\d+)$")]
        public PayPalList<PayPalBasicPaymentRequest> PaymentRequests
        {
            get { return this.paymentRequests; }
            set
            {
                // Validating the list.
                if (value != null)
                {
                    if (value.Count > 1 && value.SelectMany(x => x.Items).Any(x => x.Category == ItemCategory.Digital))
                        throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());

                    if (value.Count > 10)
                        throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
                }

                var old = PayPalParentableHelper.SetProperty(this, ref this.paymentRequests, value);

                var evtIns = new PayPalList<PayPalBasicPaymentRequest>.ListChangeFunc(InsertItemEvent);
                if (old != null) old.InsertItemEvent -= evtIns;
                if (value != null) value.InsertItemEvent += evtIns;

                var evtSet = new PayPalList<PayPalBasicPaymentRequest>.ListChangeFunc(SetItemEvent);
                if (old != null) old.SetItemEvent -= evtSet;
                if (value != null) value.SetItemEvent += evtSet;
            }
        }
        PayPalList<PayPalBasicPaymentRequest> paymentRequests;

        bool InsertItemEvent(PayPalList<PayPalBasicPaymentRequest> sender, int index, PayPalBasicPaymentRequest item)
        {
            if (item == null)
                throw new ArgumentException(PayPalResources.PaymentListDoesNotSupportNulls(), "item");

            var preview = sender.ContinueWith(item).ToList();
            var allItems = preview.SelectMany(x => x.Items);
            bool hasAnyDigitalGood = allItems.Any(x => x.Category == ItemCategory.Digital);

            int limit = hasAnyDigitalGood ? 1 : 10;

            if (preview.Count > limit)
            {
                if (hasAnyDigitalGood)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                else
                    throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
            }

            return true;
        }

        bool SetItemEvent(PayPalList<PayPalBasicPaymentRequest> sender, int index, PayPalBasicPaymentRequest item)
        {
            if (item == null)
                throw new ArgumentException(PayPalResources.PaymentListDoesNotSupportNulls(), "item");

            var preview = sender.ToList();
            preview[index] = item;

            var allItems = preview.SelectMany(x => x.Items);
            bool hasAnyDigitalGood = allItems.Any(x => x.Category == ItemCategory.Digital);

            int limit = hasAnyDigitalGood ? 1 : 10;

            if (preview.Count > limit)
            {
                if (hasAnyDigitalGood)
                    throw new Exception(PayPalResources.OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods());
                else
                    throw new Exception(PayPalResources.ExpressCheckoutSupportsUpTo10Payments());
            }

            return true;
        }
        #endregion
    }

    public class PayPalCreateRecurringPaymentsProfileResult : PayPalResult
    {
        #region CreateRecurringPaymentsProfile Response Fields
        /// <summary>
        /// An unique identifier for future reference to the details of this recurring payment.
        /// </summary>
        /// <remarks>
        ///  <p>An unique identifier for future reference to the details of this recurring payment.</p>
        ///  <p>Character length and limitations: Up to 14 single-byte alphanumeric characters.</p>
        /// </remarks>
        [NameValue(Name = "PROFILEID")]
        [StringLength(14)]
        public string ProfileId { get; set; }

        /// <summary>
        /// Status of the recurring payment profile.
        /// </summary>
        /// <remarks>
        ///       <p>Status of the recurring payment profile.</p>
        ///  <br />
        ///  <ul>
        ///    <li><p><samp class="codeph">ActiveProfile</samp> – The recurring payment profile has
        ///         been successfully created and activated for scheduled payments according the
        ///         billing instructions from the recurring payments profile.</p></li>
        ///    <li><p><samp class="codeph">PendingProfile</samp> – The system is in the process of
        ///         creating the recurring payment profile. Please check your IPN messages for an
        ///         update.</p></li>
        ///  </ul>
        /// </remarks>
        [NameValue(Name = "PROFILESTATUS")]
        public RecurringPaymentsProfileStatus ProfileStatus { get; set; }
        #endregion
    }
}
