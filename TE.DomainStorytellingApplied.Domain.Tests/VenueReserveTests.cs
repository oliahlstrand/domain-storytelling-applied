using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: Tester för att reservera en tid i en lokal.
public class VenueReserveTests
{
    [Fact]
    public void GivenAssociation_WhenReserving_ShouldBeReservedAndCostHourlyRate()
    {
        var venue = NewVenue();

        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);

        booking.Status.ShouldBe(BookingStatus.Reserved);
        booking.Price.ShouldBe(new Money(100));
        booking.Slot.ShouldBe(Tomorrow18);
        venue.Bookings.ShouldContain(booking);
    }

    [Fact]
    public void GivenMunicipality_WhenReserving_ShouldBeConfirmedImmediatelyAndFree()
    {
        var booking = NewVenue().Reserve(Tomorrow18, BookerType.Municipality, Now);

        booking.Status.ShouldBe(BookingStatus.Confirmed);
        booking.Price.IsZero.ShouldBeTrue();
    }

    [Fact]
    public void GivenPrivatePersonBookingTwoHours_WhenReserving_ShouldCostRateTimesHours()
    {
        var twoHours = new TimeSlot(Tomorrow18.Start, 2);

        var booking = NewVenue().Reserve(twoHours, BookerType.PrivatePerson, Now);

        booking.Price.ShouldBe(new Money(200));
    }
}
