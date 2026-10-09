namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: VALUE OBJECT (värdeobjekt). Ett värde utan egen identitet: två Money(100) är "samma sak",
// precis som två femhundralappar är samma sak. Det är oföränderligt och validerar sig självt,
// så ett ogiltigt belopp (t.ex. -5 kr) kan aldrig finnas i systemet.
// Förenkling: bara svenska kronor, så vi slipper valutahantering i en PoC.
//
// SYNTAX: "readonly record struct" betyder:
//   - struct: ett litet värde som lagras direkt, inte som referens till ett objekt.
//   - record: C# skriver jämförelsen åt oss. Två Money med samma belopp är lika (==).
//   - readonly: inget i värdet kan ändras efter att det skapats.
public readonly record struct Money
{
    // SYNTAX: "{ get; }" är en egenskap som går att läsa men inte ändra utifrån.
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainException("Ett belopp kan inte vara negativt.");
        }

        Amount = amount;
    }

    // SYNTAX: "static" hör till typen, inte till ett enskilt belopp: Money.Zero.
    public static Money Zero
    {
        get { return new Money(0); }
    }

    public bool IsZero
    {
        get { return Amount == 0; }
    }

    // Returnerar ett NYTT Money. Det gamla ändras aldrig.
    public Money Times(int factor)
    {
        return new Money(Amount * factor);
    }
}
