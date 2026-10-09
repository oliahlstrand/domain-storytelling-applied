namespace TE.DomainStorytellingApplied.Domain;

public interface IPaymentGateway
{
    void StartPayment(PaymentId paymentId, Money amount);

    void Refund(PaymentId paymentId, Money amount);
}
