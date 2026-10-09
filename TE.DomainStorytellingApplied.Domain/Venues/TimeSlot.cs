namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: VALUE OBJECT. En tidsperiod som alltid är hela timmar, t.ex. 18:00-20:00.
// Reglerna ligger här så att ingen annan kod behöver kontrollera dem: en TimeSlot som finns är alltid giltig.
// Tider är lokal tid i kommunen (DateTime utan tidszon). Förenkling: sommartidsbyte hanteras inte.
//
// SYNTAX: "readonly record struct" förklaras i Money.cs.
public readonly record struct TimeSlot
{
    public DateTime Start { get; }
    public int Hours { get; }

    // SYNTAX: En egenskap utan lagring. Den räknas ut varje gång någon läser den.
    public DateTime End
    {
        get { return Start.AddHours(Hours); }
    }

    // SYNTAX: "int hours = 1" är ett standardvärde. new TimeSlot(start) betyder samma som new TimeSlot(start, 1).
    public TimeSlot(DateTime start, int hours = 1)
    {
        if (start.Minute != 0 || start.Second != 0 || start.Millisecond != 0)
        {
            throw new DomainException("En tidsslot måste starta på en hel timme.");
        }

        if (hours < 1)
        {
            throw new DomainException("En tidsslot måste vara minst en timme.");
        }

        Start = start;
        Hours = hours;
    }

    // FÖRKLARING: Två perioder överlappar om var och en startar innan den andra slutar.
    // Därför överlappar 10-11 och 11-12 INTE (de ligger precis intill varandra).
    public bool Overlaps(TimeSlot other)
    {
        return Start < other.End && other.Start < End;
    }
}
