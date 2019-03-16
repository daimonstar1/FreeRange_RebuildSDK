using UnityEngine;

public class PayPalConfig : MonoBehaviour
{
    public enum PayPalEndpoint
    {
        SANDBOX,
        LIVE
    }
    public string currencyCode;
    public PayPalEndpoint payPalEndpoint;
    public string clientID;
    public string secret;
    public bool isUsingSandbox()
    {
        return payPalEndpoint == PayPalEndpoint.SANDBOX;
    }
}
