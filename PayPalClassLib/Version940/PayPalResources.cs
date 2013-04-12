namespace PayPal.Version940
{
    public static class PayPalResources
    {
        public const string SandboxDefaultAppId = "APP-80W284485P519543T";

        public const string SandboxDefaultSignature = "A-IzJhZZjhg29XQ2qnhapuwxIDzyAZQ92FRP5dqBzVesOkzbdUONzmOU";
        public const string SandboxDefaultUser = "sdk-three_api1.sdk.com";
        public const string SandboxDefaultPassword = "QFZCWN5HZM8VBG7Q";

        public static string OnlyOnePaymentIsSupportedWhenThereAreDigitalGoods()
        {
            return "Cannot have digital goods when there is more than one payment request.";
        }

        public static string ExpressCheckoutSupportsUpTo10Payments()
        {
            return "Express checkout supports up to 10 payments in a single operation.";
        }

        public static string ObjectAlreadyAttachedToAnotherOwner()
        {
            return "The value object is already attached to another owner.";
        }

        public static string PaymentListDoesNotSupportNulls()
        {
            return "Payment request list does not support null values.";
        }

        public static string CannotChangePropertyToUndefined(string property, object currentValue)
        {
            return string.Format("Cannot change the value of the property from {1} to Undefined: {0}.", property, currentValue);
        }
    }
}
