using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

public class ReservationTimeoutTests
{
    [Fact]
    public void GivenReservation_WhenCreated_ShouldHoldSlotFor15Minutes()
    {
        var booking = NewVenue().Reserve(Tomorrow18, BookerType.Association, Now);

        booking.ReservedUntil.ShouldBe(Now.AddMinutes(15));
    }

    [Fact]
    public void GivenReservationJustBeforeTimeout_WhenAnotherReserves_ShouldThrow()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);
        var oneSecondBefore = AfterTimeout(Now).AddSeconds(-1);

        Should.Throw<DomainException>(() => venue.Reserve(Tomorrow18, BookerType.PrivatePerson, oneSecondBefore));
    }

    [Fact]
    public void GivenReservationTimedOut_WhenAnotherReserves_ShouldSucceed()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Association, Now);
        var later = AfterTimeout(Now);

        venue.IsAvailable(Tomorrow18, later).ShouldBeTrue();
        venue.Reserve(Tomorrow18, BookerType.PrivatePerson, later).Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void GivenConfirmedMunicipalityBooking_WhenTimeoutPasses_ShouldStillBlockSlot()
    {
        var venue = NewVenue();
        venue.Reserve(Tomorrow18, BookerType.Municipality, Now);

        venue.IsAvailable(Tomorrow18, AfterTimeout(Now)).ShouldBeFalse();
    }
}
