namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: AGGREGATROT. Venue = en lokal som kommunen hyr ut (klassrum, hall, fotbollsplan ...).
// Venue äger alla sina bokningar. Regeln "ingen dubbelbokning" gäller alla bokningar för en lokal,
// så alla ändringar måste gå genom samma objekt. Två lokaler bokas helt oberoende av varandra.
public class Venue
{
    public VenueId Id { get; }
    public string Name { get; }
    public Money HourlyRate { get; }

    // FÖRKLARING: Öppettider i hela timmar, t.ex. 8 och 22.
    // Förenkling: samma alla dagar och ingen öppning över midnatt.
    public int OpensHour { get; }
    public int ClosesHour { get; }

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
}
