namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Kontrakt (interface) för den EXTERNA betaltjänsten. BookingService pratar bara med
// kontraktet, så i tester kan vi byta den riktiga betaltjänsten mot en fejk (FakePaymentGateway).
//
// Beskedet "betalningen gick igenom" kommer i andra riktningen: betaltjänsten anropar
// BookingService.PaymentSucceeded.
public interface IPaymentGateway
{
    // Be betaltjänsten ta emot en betalning på detta belopp.
    void StartPayment(PaymentId paymentId, Money amount);

    // Be betaltjänsten betala tillbaka pengar.
    void Refund(PaymentId paymentId, Money amount);
}
