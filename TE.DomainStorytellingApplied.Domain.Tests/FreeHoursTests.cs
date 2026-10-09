using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: Tester för "vad är ledigt och vad är upptaget?".
public class FreeHoursTests
{
    private static readonly DateTime Tomorrow = Tomorrow18.Start.Date;

    [Fact]
    public void GivenEmptyVenue_WhenListingFreeHours_ShouldReturnAllOpeningHours()
    {
        var free = NewVenue().FreeHours(Tomorrow, Now);

        // Öppet 08-22 ger 14 lediga timmar.
        free.Count.ShouldBe(14);
        free[0].Start.Hour.ShouldBe(8);
        free[13].Start.Hour.ShouldBe(21);
    }

    [Fact]
    public void GivenTwoHourBooking_WhenListingFreeHours_ShouldExcludeBookedHours()
    {
        var venue = NewVenue();
        venue.Reserve(new TimeSlot(Tomorrow18.Start, 2), BookerType.Association, Now);

        var free = venue.FreeHours(Tomorrow, Now);

        free.Count.ShouldBe(12);
        free.ShouldNotContain(Tomorrow18);
        free.ShouldNotContain(new TimeSlot(Tomorrow18.End));
    }

    [Fact]
    public void GivenTimedOutReservation_WhenListingFreeHours_ShouldIncludeItAgain()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);

        var free = venue.FreeHours(Tomorrow, AfterTimeout(Now));

        free.ShouldContain(Tomorrow18);
    }

    [Fact]
    public void GivenToday_WhenListingFreeHours_ShouldExcludeHoursAlreadyStarted()
    {
        // Klockan är 08:30, så första lediga hela timmen är 09:00.
        var free = NewVenue().FreeHours(Now, Now);

        free[0].Start.Hour.ShouldBe(9);
    }
}
