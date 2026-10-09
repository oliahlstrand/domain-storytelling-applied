using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

public class CancelBookingTests
{
    [Fact]
    public void GivenConfirmedBooking_WhenCancelling_ShouldFreeSlotAndRefundFullPrice()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        venue.ConfirmBooking(booking.Id, Now.AddMinutes(1));

        var refund = venue.CancelBooking(booking.Id, Now.AddHours(1));

        refund.ShouldBe(new Money(100));
        booking.Status.ShouldBe(BookingStatus.Cancelled);
        venue.IsAvailable(Tomorrow18, Now.AddHours(1)).ShouldBeTrue();
    }

    [Fact]
    public void GivenUnpaidReservation_WhenCancelling_ShouldRefundNothing()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.PrivatePerson, Now);

        var refund = venue.CancelBooking(booking.Id, Now.AddMinutes(1));

        refund.IsZero.ShouldBeTrue();
        venue.IsAvailable(Tomorrow18, Now.AddMinutes(1)).ShouldBeTrue();
    }

    [Fact]
    public void GivenMunicipalityBooking_WhenCancelling_ShouldFreeSlotWithoutRefund()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Municipality, Now);

        venue.CancelBooking(booking.Id, Now).IsZero.ShouldBeTrue();
        venue.IsAvailable(Tomorrow18, Now).ShouldBeTrue();
    }

    [Fact]
    public void GivenAlreadyCancelled_WhenCancellingAgain_ShouldThrow()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        venue.CancelBooking(booking.Id, Now);

        Should.Throw<DomainException>(() => venue.CancelBooking(booking.Id, Now));
    }

    [Fact]
    public void GivenSlotAlreadyStarted_WhenCancelling_ShouldThrow()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Municipality, Now);

        Should.Throw<DomainException>(() => venue.CancelBooking(booking.Id, Tomorrow18.Start));
    }

    [Fact]
    public void GivenCancelledBeforePayment_WhenPaymentArrives_ShouldRequireRefund()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        venue.CancelBooking(booking.Id, Now.AddMinutes(1));

        var result = venue.ConfirmBooking(booking.Id, Now.AddMinutes(2));

        result.ShouldBe(ConfirmResult.RefundRequired);
        booking.Status.ShouldBe(BookingStatus.Cancelled);
    }
}
