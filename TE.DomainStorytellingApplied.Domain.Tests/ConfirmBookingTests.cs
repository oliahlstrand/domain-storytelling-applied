using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: Tester för att bekräfta en bokning när betalningen är klar.
public class ConfirmBookingTests
{
    [Fact]
    public void GivenPaymentInTime_WhenConfirming_ShouldConfirm()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);

        var result = venue.ConfirmBooking(booking.Id, Now.AddMinutes(5));

        result.ShouldBe(ConfirmResult.Confirmed);
        booking.Status.ShouldBe(BookingStatus.Confirmed);
    }

    [Fact]
    public void GivenSameConfirmationTwice_WhenConfirming_ShouldStayConfirmed()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        venue.ConfirmBooking(booking.Id, Now.AddMinutes(1));

        venue.ConfirmBooking(booking.Id, Now.AddMinutes(2)).ShouldBe(ConfirmResult.Confirmed);
    }

    [Fact]
    public void GivenLatePaymentAndSlotStillFree_WhenConfirming_ShouldConfirm()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);

        var result = venue.ConfirmBooking(booking.Id, AfterTimeout(Now).AddMinutes(5));

        result.ShouldBe(ConfirmResult.Confirmed);
        booking.Status.ShouldBe(BookingStatus.Confirmed);
    }

    [Fact]
    public void GivenLatePaymentAndSlotTakenByAnother_WhenConfirming_ShouldExpireAndRequireRefund()
    {
        var venue = NewVenue();
        var late = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        var later = AfterTimeout(Now);
        var other = venue.Reserve(Tomorrow18, BookerType.PrivatePerson, later);

        var result = venue.ConfirmBooking(late.Id, later.AddMinutes(1));

        result.ShouldBe(ConfirmResult.RefundRequired);
        late.Status.ShouldBe(BookingStatus.Expired);
        other.Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void GivenExpiredBooking_WhenConfirmingAgain_ShouldStillRequireRefund()
    {
        var venue = NewVenue();
        var late = venue.Reserve(Tomorrow18, BookerType.Association, Now);
        var later = AfterTimeout(Now);
        venue.Reserve(Tomorrow18, BookerType.PrivatePerson, later);
        venue.ConfirmBooking(late.Id, later.AddMinutes(1));

        // Den andra reservationen har också gått ut, men den sena bokningen ska inte väckas till liv.
        var result = venue.ConfirmBooking(late.Id, AfterTimeout(later));

        result.ShouldBe(ConfirmResult.RefundRequired);
        late.Status.ShouldBe(BookingStatus.Expired);
    }

    [Fact]
    public void GivenPaymentAfterSlotStarted_WhenConfirming_ShouldRequireRefund()
    {
        var venue = NewVenue();
        var booking = venue.Reserve(Tomorrow18, BookerType.Association, Now);

        var result = venue.ConfirmBooking(booking.Id, Tomorrow18.Start);

        result.ShouldBe(ConfirmResult.RefundRequired);
        booking.Status.ShouldBe(BookingStatus.Expired);
    }

    [Fact]
    public void GivenUnknownBooking_WhenConfirming_ShouldThrow()
    {
        Should.Throw<DomainException>(() => NewVenue().ConfirmBooking(BookingId.New(), Now));
    }
}
