using static TE.DomainStorytellingApplied.Domain.Tests.TestData;

namespace TE.DomainStorytellingApplied.Domain.Tests;

// FÖRKLARING: SCENARIOTESTER. De spelar upp hela domain storyn: bokaren reserverar, vi låtsas vara
// betaltjänsten och skickar besked, och vi kontrollerar slutläget. Läs dem som små berättelser.
public class BookingFlowTests
{
    private readonly FakePaymentGateway _gateway = new FakePaymentGateway();
    private readonly BookingService _service;

    // SYNTAX: xUnit skapar en ny instans av testklassen för varje test, så varje test får en egen
    // lokal och en egen fejk-betaltjänst.
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
}
