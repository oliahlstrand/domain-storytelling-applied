namespace TE.DomainStorytellingApplied.Domain;

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

    public ConfirmResult PaymentSucceeded(PaymentId paymentId, DateTime now)
    {
        var payment = FindPayment(paymentId);

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

    public Money Cancel(BookingId bookingId, DateTime now)
    {
        var refund = _venue.CancelBooking(bookingId, now);

        if (!refund.IsZero)
        {
            var payment = FindPaymentForBooking(bookingId);
            _gateway.Refund(payment.Id, refund);
            payment.MarkRefunded();
        }

        return refund;
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
