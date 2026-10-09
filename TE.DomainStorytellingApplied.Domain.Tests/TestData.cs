namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: Gemensamma byggstenar så att varje test bara visar det som är viktigt för just det testet.
// "Now" är en fast tid som skickas in i domänen, så testerna ger samma resultat varje gång.
internal static class TestData
{
    public static readonly DateTime Now = new DateTime(2026, 10, 9, 8, 30, 0);

    public static readonly TimeSlot Tomorrow18 = new TimeSlot(new DateTime(2026, 10, 10, 18, 0, 0));

    // Sporthallen: 100 kr/timme, öppet 08-22.
    public static Venue NewVenue()
    {
        return new Venue("Sporthallen", new Money(100), 8, 22);
    }
}
