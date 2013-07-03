namespace PayPal.Version940
{
    public interface IPayPalApiSettings
    {
        string LocalCode { get; }
        string CurrencyCode { get; }
        string PayPalUser { get; }
        string PayPalPassword { get; }
        string PayPalSignature { get; }
        string ApiType { get; }
        string MessageToChangeProperty(string propertyName);
    }
}