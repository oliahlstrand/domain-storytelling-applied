namespace TE.DomainStorytellingApplied.Domain.Tests;

public class TimeSlotTests
{
    private static readonly DateTime Day18 = new DateTime(2026, 10, 10, 18, 0, 0);

    [Fact]
    public void GivenOnlyStart_WhenCreatingSlot_ShouldBeOneHour()
    {
        var slot = new TimeSlot(Day18);

        slot.Hours.ShouldBe(1);
        slot.End.ShouldBe(Day18.AddHours(1));
    }

    [Fact]
    public void GivenZeroHours_WhenCreatingSlot_ShouldThrow()
    {
        Should.Throw<DomainException>(() => new TimeSlot(Day18, 0));
    }

    [Fact]
    public void GivenSlotsRightNextToEachOther_WhenCheckingOverlap_ShouldNotOverlap()
    {
        var first = new TimeSlot(Day18);
        var next = new TimeSlot(first.End);

        first.Overlaps(next).ShouldBeFalse();
    }

    [Fact]
    public void GivenTwoHourSlotAndItsSecondHour_WhenCheckingOverlap_ShouldOverlapBothWays()
    {
        var twoHours = new TimeSlot(Day18, 2);
        var secondHour = new TimeSlot(Day18.AddHours(1));

        twoHours.Overlaps(secondHour).ShouldBeTrue();
        secondHour.Overlaps(twoHours).ShouldBeTrue();
    }

    [Fact]
    public void GivenMinutesPastTheHour_WhenCreatingSlot_ShouldThrow()
    {
        Should.Throw<DomainException>(() => new TimeSlot(new DateTime(2026, 10, 10, 18, 30, 0)));
    }
}
