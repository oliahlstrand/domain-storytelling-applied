namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: VALUE OBJECT. En tidsperiod som alltid är hela timmar, t.ex. 18:00-20:00.
// Reglerna ligger här så att ingen annan kod behöver kontrollera dem: en TimeSlot som finns är alltid giltig.
// Tider är lokal tid i kommunen (DateTime utan tidszon). Förenkling: sommartidsbyte hanteras inte.
//
// SYNTAX: "readonly record struct" förklaras i Money.cs.
public readonly record struct TimeSlot
{
    public DateTime Start { get; }

    public TimeSlot(DateTime start)
    {
        if (start.Minute != 0 || start.Second != 0 || start.Millisecond != 0)
        {
            throw new DomainException("En tidsslot måste starta på en hel timme.");
        }

        Start = start;
    }
}
