namespace TE.DomainStorytellingApplied.Domain;

public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}

public class Payment
{
    public PaymentId Id { get; }
    public BookingId BookingId { get; }
    public Money Amount { get; }
    public PaymentStatus Status { get; private set; }

    private Payment(BookingId bookingId, Money amount)
    {
        Id = PaymentId.New();
        BookingId = bookingId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public static Payment ForReservation(Booking booking)
    {
        if (booking.Status != BookingStatus.Reserved)
        {
            throw new DomainException("Bara en reserverad bokning kan betalas.");
        }

        if (booking.Price.IsZero)
        {
            throw new DomainException("En bokning utan kostnad behöver ingen betalning.");
        }

        return new Payment(booking.Id, booking.Price);
    }

    public void MarkPaid()
    {
        ChangeStatus(PaymentStatus.Pending, PaymentStatus.Paid);
    }

    public void MarkRefunded()
    {
        ChangeStatus(PaymentStatus.Paid, PaymentStatus.Refunded);
    }

    private void ChangeStatus(PaymentStatus requiredCurrent, PaymentStatus newStatus)
    {
        if (Status != requiredCurrent)
        {
            throw new DomainException("Ogiltig betalningsändring: " + Status + " till " + newStatus + ".");
        }

        Status = newStatus;
    }
}
