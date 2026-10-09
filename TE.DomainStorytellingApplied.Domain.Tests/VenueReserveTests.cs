using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

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

    [Fact]
    public void GivenTimeInThePast_WhenReserving_ShouldThrow()
    {
        var pastSlot = new TimeSlot(new DateTime(2026, 10, 9, 8, 0, 0));

        Should.Throw<DomainException>(() => NewVenue().Reserve(pastSlot, BookerType.Association, Now));
    }

    [Fact]
    public void GivenSlotStartingExactlyNow_WhenReserving_ShouldThrow()
    {
        var slot = new TimeSlot(new DateTime(2026, 10, 9, 9, 0, 0));
        var now = slot.Start;

        Should.Throw<DomainException>(() => NewVenue().Reserve(slot, BookerType.Association, now));
    }

    [Theory]
    [InlineData(7, 1)]
    [InlineData(21, 2)]
    [InlineData(22, 1)]
    public void GivenOutsideOpeningHours_WhenReserving_ShouldThrow(int startHour, int hours)
    {
        var slot = new TimeSlot(new DateTime(2026, 10, 10, startHour, 0, 0), hours);

        Should.Throw<DomainException>(() => NewVenue().Reserve(slot, BookerType.Association, Now));
    }

    [Theory]
    [InlineData(8, 1)]
    [InlineData(21, 1)]
    [InlineData(8, 14)]
    public void GivenWithinOpeningHours_WhenReserving_ShouldSucceed(int startHour, int hours)
    {
        var slot = new TimeSlot(new DateTime(2026, 10, 10, startHour, 0, 0), hours);

        Should.NotThrow(() => NewVenue().Reserve(slot, BookerType.Association, Now));
    }

    [Fact]
    public void GivenReservedSlot_WhenCheckingAvailability_ShouldBeTaken()
    {
        var venue = NewVenue();

        venue.IsAvailable(Tomorrow18, Now).ShouldBeTrue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);

        venue.IsAvailable(Tomorrow18, Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenSlotAlreadyReserved_WhenReservingAgain_ShouldThrow()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);

        Should.Throw<DomainException>(() => venue.Reserve(Tomorrow18, BookerType.Municipality, Now));
    }

    [Fact]
    public void GivenOverlappingLongerBooking_WhenReserving_ShouldThrow()
    {
        var venue = NewVenue();
        venue.Reserve(new TimeSlot(Tomorrow18.Start.AddHours(1)), BookerType.Association, Now);

        var twoHours = new TimeSlot(Tomorrow18.Start, 2);

        Should.Throw<DomainException>(() => venue.Reserve(twoHours, BookerType.PrivatePerson, Now));
    }

    [Fact]
    public void GivenBookingTheHourBefore_WhenReservingNextHour_ShouldSucceed()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);

        Should.NotThrow(() => venue.Reserve(new TimeSlot(Tomorrow18.End), BookerType.PrivatePerson, Now));
    }

    [Fact]
    public void GivenSameSlotInAnotherVenue_WhenReserving_ShouldSucceed()
    {
        NewVenue().Reserve(Tomorrow18, BookerType.Association, Now);

        Should.NotThrow(() => NewVenue().Reserve(Tomorrow18, BookerType.Association, Now));
    }
}
