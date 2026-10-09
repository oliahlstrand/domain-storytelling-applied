namespace TE.DomainStorytellingApplied.Domain;

public readonly record struct TimeSlot
{
    public DateTime Start { get; }
    public int Hours { get; }

    public DateTime End
    {
        get { return Start.AddHours(Hours); }
    }

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

    public bool Overlaps(TimeSlot other)
    {
        return Start < other.End && other.Start < End;
    }
}
