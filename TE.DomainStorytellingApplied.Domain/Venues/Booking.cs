namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Livscykeln för en bokning.
//   Reserved --betalning ok--> Confirmed
//   Kommunen går direkt till Confirmed (kostar 0 kr).
public enum BookingStatus
{
    Reserved,
    Confirmed
}

// FÖRKLARING: ENTITET. Har en identitet (Id) och förändras över tid (Status).
// Booking ligger INUTI aggregatet Venue och får bara ändras via Venue. Därför är konstruktorn och
// metoderna som ändrar något "internal": inget annat projekt kan kringgå Venues regler.
//
// SYNTAX: "internal" = får bara användas inom detta projekt (Domain).
// SYNTAX: "{ get; private set; }" = alla kan läsa, men bara den här klassen kan ändra.
public class Booking
{
    public BookingId Id { get; }
    public TimeSlot Slot { get; }
    public BookerType BookerType { get; }
    public Money Price { get; }
    public BookingStatus Status { get; private set; }

    internal Booking(TimeSlot slot, BookerType bookerType, Money price)
    {
        Id = BookingId.New();
        Slot = slot;
        BookerType = bookerType;
        Price = price;
        Status = BookingStatus.Reserved;
    }

    internal void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }
}
