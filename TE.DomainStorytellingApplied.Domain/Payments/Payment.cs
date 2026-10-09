namespace TE.DomainStorytellingApplied.Domain;

// Tillåtna övergångar: Pending -> Paid -> Refunded.
// Förenkling: misslyckad betalning modelleras inte. Bokningen släpper tiden av sig själv efter 15 min.
public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}

// FÖRKLARING: EGET AGGREGAT, skilt från Venue. En betalning har en egen livscykel och sköts av en
// extern betaltjänst. Den pekar på bokningen bara via BookingId, så de två aggregaten kan ändras
// och sparas var för sig. BookingService kopplar ihop dem.
public class Payment
{
    public PaymentId Id { get; }
    public BookingId BookingId { get; }
    public Money Amount { get; }
    public PaymentStatus Status { get; private set; }

    // SYNTAX: "private" konstruktor = ingen utanför klassen kan skriva "new Payment(...)".
    // De måste använda ForReservation, som kontrollerar reglerna först.
    private Payment(BookingId bookingId, Money amount)
    {
        Id = PaymentId.New();
        BookingId = bookingId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    // FÖRKLARING: Fabriksmetod. Beloppet tas från bokningen, så det kan aldrig skilja sig från priset.
    // Kommunbokningar (0 kr) och redan bekräftade bokningar får ingen betalning.
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

    // En status får bara bytas från rätt tidigare läge, t.ex. kan Pending inte bli Refunded.
    private void ChangeStatus(PaymentStatus requiredCurrent, PaymentStatus newStatus)
    {
        if (Status != requiredCurrent)
        {
            throw new DomainException("Ogiltig betalningsändring: " + Status + " till " + newStatus + ".");
        }

        Status = newStatus;
    }
}
