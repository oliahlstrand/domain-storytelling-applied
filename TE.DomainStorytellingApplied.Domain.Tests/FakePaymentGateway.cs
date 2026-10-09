namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: En FEJK betaltjänst som bara finns i testerna. Den gör inget på riktigt, den antecknar
// bara vad vi bett den göra. Testet spelar sedan betaltjänstens roll genom att självt anropa
// BookingService.PaymentSucceeded. Så styr vi exakt vad som händer och när.
internal class FakePaymentGateway : IPaymentGateway
{
    public List<PaymentId> StartedPayments { get; } = new List<PaymentId>();
    public List<PaymentId> RefundedPayments { get; } = new List<PaymentId>();

    public void StartPayment(PaymentId paymentId, Money amount)
    {
        StartedPayments.Add(paymentId);
    }

    public void Refund(PaymentId paymentId, Money amount)
    {
        RefundedPayments.Add(paymentId);
    }
}
