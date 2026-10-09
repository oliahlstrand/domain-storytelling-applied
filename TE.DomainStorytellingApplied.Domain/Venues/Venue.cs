namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Svaret när en betald bokning ska bekräftas.
// RefundRequired = betalningen kom för sent, tiden går inte att ge kunden. Domänen kan inte själv
// betala tillbaka (det gör betaltjänsten), så den talar om att det behövs.
public enum ConfirmResult
{
    Confirmed,
    RefundRequired
}

// FÖRKLARING: AGGREGATROT. Venue = en lokal som kommunen hyr ut (klassrum, hall, fotbollsplan ...).
// Venue äger alla sina bokningar. Regeln "ingen dubbelbokning" gäller alla bokningar för en lokal,
// så alla ändringar måste gå genom samma objekt. Två lokaler bokas helt oberoende av varandra.
public class Venue
{
    // SYNTAX: "private readonly" = bara Venue når listan, och listan byts aldrig ut (men kan fyllas på).
    private readonly List<Booking> _bookings = new List<Booking>();

    public VenueId Id { get; }
    public string Name { get; }
    public Money HourlyRate { get; }

    // FÖRKLARING: Öppettider i hela timmar, t.ex. 8 och 22.
    // Förenkling: samma alla dagar och ingen öppning över midnatt.
    public int OpensHour { get; }
    public int ClosesHour { get; }

    // SYNTAX: IReadOnlyList = utomstående får läsa listan men inte lägga till eller ta bort.
    // FÖRKLARING: Annars kunde någon lägga till en bokning och kringgå reglerna.
    public IReadOnlyList<Booking> Bookings
    {
        get { return _bookings; }
    }

    public Venue(string name, Money hourlyRate, int opensHour, int closesHour)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("En lokal måste ha ett namn.");
        }

        if (opensHour < 0 || closesHour > 24 || opensHour >= closesHour)
        {
            throw new DomainException("Öppettiderna är ogiltiga.");
        }

        Id = VenueId.New();
        Name = name;
        HourlyRate = hourlyRate;
        OpensHour = opensHour;
        ClosesHour = closesHour;
    }

    // FÖRKLARING: Är tiden ledig? Ledig = ingen bokning som blockerar och krockar med tiden.
    public bool IsAvailable(TimeSlot slot, DateTime now)
    {
        foreach (var booking in _bookings)
        {
            if (booking.BlocksSlot(now) && booking.Slot.Overlaps(slot))
            {
                return false;
            }
        }

        return true;
    }

    // FÖRKLARING: Skapar en bokning. Kommunen får Confirmed direkt (0 kr), övriga får Reserved och måste betala.
    public Booking Reserve(TimeSlot slot, BookerType bookerType, DateTime now)
    {
        // En tid som börjar exakt nu räknas som redan börjad.
        if (slot.Start <= now)
        {
            throw new DomainException("Tiden har redan börjat eller passerat.");
        }

        // Hela tiden måste rymmas inom samma dags öppettider.
        // SYNTAX: slot.Start.Date är samma dag klockan 00:00.
        var opensAt = slot.Start.Date.AddHours(OpensHour);
        var closesAt = slot.Start.Date.AddHours(ClosesHour);
        if (slot.Start < opensAt || slot.End > closesAt)
        {
            throw new DomainException("Tiden ligger utanför lokalens öppettider.");
        }

        if (!IsAvailable(slot, now))
        {
            throw new DomainException("Tiden är redan bokad.");
        }

        // Prisregeln: kommunen betalar inget, alla andra betalar timpris gånger antal timmar.
        var price = HourlyRate.Times(slot.Hours);
        if (bookerType == BookerType.Municipality)
        {
            price = Money.Zero;
        }

        var booking = new Booking(slot, bookerType, price, now);

        if (price.IsZero)
        {
            booking.Confirm();
        }

        _bookings.Add(booking);

        return booking;
    }

    // FÖRKLARING: Anropas när betalningen är klar. Tål att anropas flera gånger för samma bokning
    // (betaltjänster skickar ibland samma besked två gånger).
    // Sen betalning (reservationen hann gå ut):
    //   - tiden är fortfarande ledig och har inte börjat: bekräfta ändå, kunden har ju betalat.
    //   - någon annan har tagit tiden, eller tiden har börjat: RefundRequired.
    public ConfirmResult ConfirmBooking(BookingId id, DateTime now)
    {
        var booking = Find(id);

        if (booking.Status == BookingStatus.Confirmed)
        {
            return ConfirmResult.Confirmed;
        }

        // Utgången eller avbokad innan betalningen kom: pengarna måste tillbaka.
        if (booking.Status == BookingStatus.Expired || booking.Status == BookingStatus.Cancelled)
        {
            return ConfirmResult.RefundRequired;
        }

        if (booking.Slot.Start <= now || SlotTakenByAnother(booking, now))
        {
            booking.Expire();
            return ConfirmResult.RefundRequired;
        }

        booking.Confirm();
        return ConfirmResult.Confirmed;
    }

    // FÖRKLARING: Avbokning, bara innan tiden har börjat. Returnerar beloppet som ska betalas tillbaka:
    // hela priset om bokningen var bekräftad, annars 0 kr (inget var betalt).
    // Full återbetalning är ett antagande, de riktiga avbokningsreglerna är okända.
    public Money CancelBooking(BookingId id, DateTime now)
    {
        var booking = Find(id);

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Expired)
        {
            throw new DomainException("Bokningen är redan avslutad.");
        }

        if (booking.Slot.Start <= now)
        {
            throw new DomainException("Det går inte att avboka en tid som redan har börjat.");
        }

        var refund = Money.Zero;
        if (booking.Status == BookingStatus.Confirmed)
        {
            refund = booking.Price;
        }

        booking.Cancel();
        return refund;
    }

    // Finns det en ANNAN bokning som blockerar samma tid?
    private bool SlotTakenByAnother(Booking booking, DateTime now)
    {
        foreach (var other in _bookings)
        {
            if (other != booking && other.BlocksSlot(now) && other.Slot.Overlaps(booking.Slot))
            {
                return true;
            }
        }

        return false;
    }

    private Booking Find(BookingId id)
    {
        foreach (var booking in _bookings)
        {
            if (booking.Id == id)
            {
                return booking;
            }
        }

        throw new DomainException("Bokningen finns inte.");
    }
}
