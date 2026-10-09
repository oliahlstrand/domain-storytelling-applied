using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: Tester för Payment-aggregatet.
public class PaymentTests
{
    [Fact]
    public void GivenReservedBooking_WhenCreatingPayment_ShouldCopyBookingPrice()
    {
        var booking = NewVenue().Reserve(Tomorrow18, BookerType.Association, Now);

        var payment = Payment.ForReservation(booking);

        payment.BookingId.ShouldBe(booking.Id);
        payment.Amount.ShouldBe(booking.Price);
        payment.Status.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void GivenFreeMunicipalityBooking_WhenCreatingPayment_ShouldThrow()
    {
        var booking = NewVenue().Reserve(Tomorrow18, BookerType.Municipality, Now);

        Should.Throw<DomainException>(() => Payment.ForReservation(booking));
    }

    [Fact]
    public void GivenPendingPayment_WhenMarkingPaid_ShouldBePaid()
    {
        var payment = NewPendingPayment();

        payment.MarkPaid();

        payment.Status.ShouldBe(PaymentStatus.Paid);
    }

    [Fact]
    public void GivenPaidPayment_WhenRefunding_ShouldBeRefunded()
    {
        var payment = NewPendingPayment();
        payment.MarkPaid();

        payment.MarkRefunded();

        payment.Status.ShouldBe(PaymentStatus.Refunded);
    }

    [Fact]
    public void GivenPendingPayment_WhenRefunding_ShouldThrow()
    {
        var payment = NewPendingPayment();

        Should.Throw<DomainException>(() => payment.MarkRefunded());
    }

    [Fact]
    public void GivenPaidPayment_WhenMarkingPaidAgain_ShouldThrow()
    {
        var payment = NewPendingPayment();
        payment.MarkPaid();

        Should.Throw<DomainException>(() => payment.MarkPaid());
    }

    private static Payment NewPendingPayment()
    {
        var booking = NewVenue().Reserve(Tomorrow18, BookerType.PrivatePerson, Now);
        return Payment.ForReservation(booking);
    }
}
