namespace TE.DomainStorytellingApplied.Domain;

public enum ConfirmResult
{
    Confirmed,
    RefundRequired
}

public class Venue
{
    private readonly List<Booking> _bookings = new List<Booking>();

    public VenueId Id { get; }
    public string Name { get; }
    public Money HourlyRate { get; }

    public int OpensHour { get; }
    public int ClosesHour { get; }

    public IReadOnlyList<Booking> Bookings
    {
        get { return _bookings; }
    }

    public Venue(string name, Money hourlyRate, int opensHour, int closesHour)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("En lokal måste ha ett namn.");
        }

        if (opensHour < 0 || closesHour > 24 || opensHour >= closesHour)
        {
            throw new DomainException("Öppettiderna är ogiltiga.");
        }

        Id = VenueId.New();
        Name = name;
        HourlyRate = hourlyRate;
        OpensHour = opensHour;
        ClosesHour = closesHour;
    }

    public bool IsAvailable(TimeSlot slot, DateTime now)
    {
        foreach (var booking in _bookings)
        {
            if (booking.BlocksSlot(now) && booking.Slot.Overlaps(slot))
            {
                return false;
            }
        }

        return true;
    }

    public IReadOnlyList<TimeSlot> FreeHours(DateTime day, DateTime now)
    {
        var free = new List<TimeSlot>();

        for (var hour = OpensHour; hour < ClosesHour; hour++)
        {
            var slot = new TimeSlot(day.Date.AddHours(hour));

            if (slot.Start > now && IsAvailable(slot, now))
            {
                free.Add(slot);
            }
        }

        return free;
    }

    public Booking Reserve(TimeSlot slot, BookerType bookerType, DateTime now)
    {
        if (slot.Start <= now)
        {
            throw new DomainException("Tiden har redan börjat eller passerat.");
        }

        var opensAt = slot.Start.Date.AddHours(OpensHour);
        var closesAt = slot.Start.Date.AddHours(ClosesHour);
        if (slot.Start < opensAt || slot.End > closesAt)
        {
            throw new DomainException("Tiden ligger utanför lokalens öppettider.");
        }

        if (!IsAvailable(slot, now))
        {
            throw new DomainException("Tiden är redan bokad.");
        }

        var price = HourlyRate.Times(slot.Hours);
        if (bookerType == BookerType.Municipality)
        {
            price = Money.Zero;
        }

        var booking = new Booking(slot, bookerType, price, now);

        if (price.IsZero)
        {
            booking.Confirm();
        }

        _bookings.Add(booking);

        return booking;
    }

    public ConfirmResult ConfirmBooking(BookingId id, DateTime now)
    {
        var booking = Find(id);

        if (booking.Status == BookingStatus.Confirmed)
        {
            return ConfirmResult.Confirmed;
        }

        if (booking.Status == BookingStatus.Expired || booking.Status == BookingStatus.Cancelled)
        {
            return ConfirmResult.RefundRequired;
        }

        if (booking.Slot.Start <= now || SlotTakenByAnother(booking, now))
        {
            booking.Expire();
            return ConfirmResult.RefundRequired;
        }

        booking.Confirm();
        return ConfirmResult.Confirmed;
    }

    public Money CancelBooking(BookingId id, DateTime now)
    {
        var booking = Find(id);

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Expired)
        {
            throw new DomainException("Bokningen är redan avslutad.");
        }

        if (booking.Slot.Start <= now)
        {
            throw new DomainException("Det går inte att avboka en tid som redan har börjat.");
        }

        var refund = Money.Zero;
        if (booking.Status == BookingStatus.Confirmed)
        {
            refund = booking.Price;
        }

        booking.Cancel();
        return refund;
    }

    private bool SlotTakenByAnother(Booking booking, DateTime now)
    {
        foreach (var other in _bookings)
        {
            if (other != booking && other.BlocksSlot(now) && other.Slot.Overlaps(booking.Slot))
            {
                return true;
            }
        }

        return false;
    }

    private Booking Find(BookingId id)
    {
        foreach (var booking in _bookings)
        {
            if (booking.Id == id)
            {
                return booking;
            }
        }

        throw new DomainException("Bokningen finns inte.");
    }
}
