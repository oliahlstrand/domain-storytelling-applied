namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: APPLIKATIONSLAGER. Innehåller INGA affärsregler. Den samordnar Venue, Payment och
// betaltjänsten i rätt ordning. Reglerna bor kvar i Venue och Payment.
//
// FÖRENKLINGAR i denna PoC:
//   - En lokal per BookingService (i verkligheten hämtas rätt Venue från en databas).
//   - Betalningarna sparas i en lista i minnet.
//   - Ingen hantering av samtidiga anrop.
public class BookingService
{
    private readonly Venue _venue;
    private readonly IPaymentGateway _gateway;
    private readonly List<Payment> _payments = new List<Payment>();

    public BookingService(Venue venue, IPaymentGateway gateway)
    {
        _venue = venue;
        _gateway = gateway;
    }

    public Venue Venue
    {
        get { return _venue; }
    }

    public IReadOnlyList<Payment> Payments
    {
        get { return _payments; }
    }

    // STEG 1: Bokaren väljer en tid. Venue kontrollerar reglerna.
    // Kommunen är redan Confirmed (0 kr), så ingen betalning startas.
    // Övriga är Reserved: vi skapar en Payment och ber betaltjänsten ta emot betalningen.
    public Booking Reserve(TimeSlot slot, BookerType bookerType, DateTime now)
    {
        var booking = _venue.Reserve(slot, bookerType, now);

        if (booking.Status == BookingStatus.Reserved)
        {
            var payment = Payment.ForReservation(booking);
            _payments.Add(payment);
            _gateway.StartPayment(payment.Id, payment.Amount);
        }

        return booking;
    }

    // STEG 2: Betaltjänsten säger "betalningen är genomförd". Tål samma besked flera gånger.
    // Svarar Venue RefundRequired (betalningen kom för sent) ber vi betaltjänsten betala tillbaka.
    public ConfirmResult PaymentSucceeded(PaymentId paymentId, DateTime now)
    {
        var payment = FindPayment(paymentId);

        // Redan återbetald: gör ingenting igen, så vi aldrig betalar tillbaka två gånger.
        if (payment.Status == PaymentStatus.Refunded)
        {
            return ConfirmResult.RefundRequired;
        }

        if (payment.Status == PaymentStatus.Pending)
        {
            payment.MarkPaid();
        }

        var result = _venue.ConfirmBooking(payment.BookingId, now);

        if (result == ConfirmResult.RefundRequired)
        {
            _gateway.Refund(payment.Id, payment.Amount);
            payment.MarkRefunded();
        }

        return result;
    }

    public Payment FindPaymentForBooking(BookingId bookingId)
    {
        foreach (var payment in _payments)
        {
            if (payment.BookingId == bookingId)
            {
                return payment;
            }
        }

        throw new DomainException("Det finns ingen betalning för bokningen.");
    }

    private Payment FindPayment(PaymentId paymentId)
    {
        foreach (var payment in _payments)
        {
            if (payment.Id == paymentId)
            {
                return payment;
            }
        }

        throw new DomainException("Betalningen finns inte.");
    }
}
