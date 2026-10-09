namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Livscykeln för en bokning.
//   Reserved --betalning ok--> Confirmed
//   Reserved --15 min utan betalning--> tiden blir ledig (status står kvar som Reserved)
//   Reserved --betalning för sent och tiden är tagen--> Expired
//   Reserved/Confirmed --avbokas före start--> Cancelled (tiden blir ledig)
//   Kommunen går direkt till Confirmed (kostar 0 kr).
public enum BookingStatus
{
    Reserved,
    Confirmed,
    Expired,
    Cancelled
}

// FÖRKLARING: ENTITET. Har en identitet (Id) och förändras över tid (Status).
// Booking ligger INUTI aggregatet Venue och får bara ändras via Venue. Därför är konstruktorn och
// metoderna som ändrar något "internal": inget annat projekt kan kringgå Venues regler.
//
// SYNTAX: "internal" = får bara användas inom detta projekt (Domain).
// SYNTAX: "{ get; private set; }" = alla kan läsa, men bara den här klassen kan ändra.
public class Booking
{
    // FÖRKLARING: Hur länge en obetald reservation håller tiden. Antagande, inte ett verifierat krav.
    // SYNTAX: "static readonly" = ett gemensamt värde för alla bokningar som inte kan ändras.
    public static readonly TimeSpan ReservationTimeout = TimeSpan.FromMinutes(15);

    public BookingId Id { get; }
    public TimeSlot Slot { get; }
    public BookerType BookerType { get; }
    public Money Price { get; }
    public DateTime ReservedUntil { get; }
    public BookingStatus Status { get; private set; }

    internal Booking(TimeSlot slot, BookerType bookerType, Money price, DateTime now)
    {
        Id = BookingId.New();
        Slot = slot;
        BookerType = bookerType;
        Price = price;
        Status = BookingStatus.Reserved;
        ReservedUntil = now + ReservationTimeout;
    }

    // FÖRKLARING: Lösningen på "glömda lokaler". En bokning blockerar tiden bara om den är bekräftad,
    // eller reserverad och inte har gått ut. Vi räknar ut det från klockan (now), så tiden blir ledig
    // i samma ögonblick som reservationen går ut, utan något städjobb.
    internal bool BlocksSlot(DateTime now)
    {
        if (Status == BookingStatus.Confirmed)
        {
            return true;
        }

        return Status == BookingStatus.Reserved && now < ReservedUntil;
    }

    internal void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

    internal void Expire()
    {
        Status = BookingStatus.Expired;
    }

    internal void Cancel()
    {
        Status = BookingStatus.Cancelled;
    }
}
