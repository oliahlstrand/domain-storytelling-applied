using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

public class BookingFlowTests
{
    private readonly FakePaymentGateway _gateway = new FakePaymentGateway();
    private readonly BookingService _service;

    public BookingFlowTests()
    {
        _service = new BookingService(NewVenue(), _gateway);
    }

    [Fact]
    public void GivenAssociation_WhenPaymentSucceedsInTime_ShouldConfirmBooking()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        booking.Status.ShouldBe(BookingStatus.Reserved);
        _gateway.StartedPayments.Count.ShouldBe(1);

        var result = _service.PaymentSucceeded(_gateway.StartedPayments[0], Now.AddMinutes(5));

        result.ShouldBe(ConfirmResult.Confirmed);
        booking.Status.ShouldBe(BookingStatus.Confirmed);
        _service.FindPaymentForBooking(booking.Id).Status.ShouldBe(PaymentStatus.Paid);
        _gateway.RefundedPayments.ShouldBeEmpty();
    }

    [Fact]
    public void GivenMunicipality_WhenReserving_ShouldConfirmDirectlyWithoutPayment()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Municipality, Now);

        booking.Status.ShouldBe(BookingStatus.Confirmed);
        _gateway.StartedPayments.ShouldBeEmpty();
        _service.Payments.ShouldBeEmpty();
    }

    [Fact]
    public void GivenReservedSlot_WhenSomeoneElseTriesSameSlot_ShouldBeRejected()
    {
        _service.Reserve(Tomorrow18, BookerType.Association, Now);

        Should.Throw<DomainException>(() =>
            _service.Reserve(Tomorrow18, BookerType.PrivatePerson, Now.AddMinutes(1)));
    }

    [Fact]
    public void GivenPaymentNeverArrives_WhenTimeoutPasses_ShouldFreeSlotForOthers()
    {
        _service.Reserve(Tomorrow18, BookerType.Association, Now);

        var second = _service.Reserve(Tomorrow18, BookerType.PrivatePerson, AfterTimeout(Now));

        second.Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void GivenLatePayment_WhenSlotTakenByAnother_ShouldRefund()
    {
        var first = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        var later = AfterTimeout(Now);
        _service.Reserve(Tomorrow18, BookerType.PrivatePerson, later);

        var result = _service.PaymentSucceeded(_gateway.StartedPayments[0], later.AddMinutes(1));

        result.ShouldBe(ConfirmResult.RefundRequired);
        first.Status.ShouldBe(BookingStatus.Expired);
        _gateway.RefundedPayments.Count.ShouldBe(1);
        _service.FindPaymentForBooking(first.Id).Status.ShouldBe(PaymentStatus.Refunded);
    }

    [Fact]
    public void GivenSamePaymentMessageTwice_WhenHandled_ShouldNotChangeAnything()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        var paymentId = _gateway.StartedPayments[0];

        _service.PaymentSucceeded(paymentId, Now.AddMinutes(1));
        var second = _service.PaymentSucceeded(paymentId, Now.AddMinutes(2));

        second.ShouldBe(ConfirmResult.Confirmed);
        booking.Status.ShouldBe(BookingStatus.Confirmed);
        _gateway.RefundedPayments.ShouldBeEmpty();
    }

    [Fact]
    public void GivenRefundedPayment_WhenSameMessageArrivesAgain_ShouldNotRefundTwice()
    {
        _service.Reserve(Tomorrow18, BookerType.Association, Now);
        var paymentId = _gateway.StartedPayments[0];
        _service.PaymentSucceeded(paymentId, Tomorrow18.Start);

        var result = _service.PaymentSucceeded(paymentId, Tomorrow18.Start.AddMinutes(1));

        result.ShouldBe(ConfirmResult.RefundRequired);
        _gateway.RefundedPayments.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenUnknownPayment_WhenPaymentSucceeds_ShouldThrow()
    {
        Should.Throw<DomainException>(() => _service.PaymentSucceeded(PaymentId.New(), Now));
    }

    [Fact]
    public void GivenConfirmedBooking_WhenCancelled_ShouldRefundAndFreeSlot()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        _service.PaymentSucceeded(_gateway.StartedPayments[0], Now.AddMinutes(1));

        var refund = _service.Cancel(booking.Id, Now.AddHours(1));

        refund.ShouldBe(booking.Price);
        _gateway.RefundedPayments.Count.ShouldBe(1);
        _service.FindPaymentForBooking(booking.Id).Status.ShouldBe(PaymentStatus.Refunded);
        _service.Venue.IsAvailable(Tomorrow18, Now.AddHours(1)).ShouldBeTrue();
    }

    [Fact]
    public void GivenCancelledBeforePayment_WhenPaymentArrivesLater_ShouldRefund()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        _service.Cancel(booking.Id, Now.AddMinutes(2));

        var result = _service.PaymentSucceeded(_gateway.StartedPayments[0], Now.AddMinutes(3));

        result.ShouldBe(ConfirmResult.RefundRequired);
        _gateway.RefundedPayments.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenCancelledAndRefunded_WhenSamePaymentMessageArrivesAgain_ShouldNotRefundTwice()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Association, Now);
        var paymentId = _gateway.StartedPayments[0];
        _service.PaymentSucceeded(paymentId, Now.AddMinutes(1));
        _service.Cancel(booking.Id, Now.AddHours(1));

        var result = _service.PaymentSucceeded(paymentId, Now.AddHours(2));

        result.ShouldBe(ConfirmResult.RefundRequired);
        _gateway.RefundedPayments.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenMunicipalityBooking_WhenCancelled_ShouldNotRefundAnything()
    {
        var booking = _service.Reserve(Tomorrow18, BookerType.Municipality, Now);

        var refund = _service.Cancel(booking.Id, Now.AddHours(1));

        refund.IsZero.ShouldBeTrue();
        _gateway.RefundedPayments.ShouldBeEmpty();
    }
}
