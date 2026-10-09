namespace TE.DomainStorytellingApplied.Domain;

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

        // Prisregeln: kommunen betalar inget, alla andra betalar timpris gånger antal timmar.
        var price = HourlyRate.Times(slot.Hours);
        if (bookerType == BookerType.Municipality)
        {
            price = Money.Zero;
        }

        var booking = new Booking(slot, bookerType, price);

        if (price.IsZero)
        {
            booking.Confirm();
        }

        _bookings.Add(booking);

        return booking;
    }
}
