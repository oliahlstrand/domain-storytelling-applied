using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

public class VenueTests
{
    [Fact]
    public void GivenValidValues_WhenCreatingVenue_ShouldKeepThem()
    {
        var venue = NewVenue();

        venue.Name.ShouldBe("Sporthallen");
        venue.HourlyRate.ShouldBe(new Money(100));
        venue.OpensHour.ShouldBe(8);
        venue.ClosesHour.ShouldBe(22);
    }

    [Fact]
    public void GivenTwoVenues_WhenCreated_ShouldHaveDifferentIds()
    {
        NewVenue().Id.ShouldNotBe(NewVenue().Id);
    }

    [Fact]
    public void GivenEmptyName_WhenCreatingVenue_ShouldThrow()
    {
        Should.Throw<DomainException>(() => new Venue(" ", new Money(100), 8, 22));
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(22, 8)]
    [InlineData(-1, 8)]
    [InlineData(8, 25)]
    public void GivenInvalidOpeningHours_WhenCreatingVenue_ShouldThrow(int opens, int closes)
    {
        Should.Throw<DomainException>(() => new Venue("Sporthallen", new Money(100), opens, closes));
    }
}
