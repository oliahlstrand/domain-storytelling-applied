namespace TE.DomainStorytellingApplied.Domain.Tests;

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
