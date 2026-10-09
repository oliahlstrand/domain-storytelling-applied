namespace TE.DomainStorytellingApplied.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void GivenNegativeAmount_WhenCreatingMoney_ShouldThrow()
    {
        Should.Throw<DomainException>(() => new Money(-1));
    }

    [Fact]
    public void GivenZeroAmount_WhenCreatingMoney_ShouldBeZero()
    {
        new Money(0).IsZero.ShouldBeTrue();
    }

    [Fact]
    public void GivenTwoMoneyWithSameAmount_WhenComparing_ShouldBeEqual()
    {
        new Money(100).ShouldBe(new Money(100));
    }
}
