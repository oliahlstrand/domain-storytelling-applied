namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Egna id-typer i stället för råa Guid. Då stoppar kompilatorn dig om du skickar
// ett BookingId där ett VenueId förväntas.
//
// SYNTAX: "readonly record struct VenueId(Guid Value)" är en kortform för en typ som innehåller
// ett Guid, åtkomligt som id.Value. Två id med samma Guid är lika.
public readonly record struct VenueId(Guid Value)
{
    public static VenueId New()
    {
        return new VenueId(Guid.NewGuid());
    }
}

public readonly record struct BookingId(Guid Value)
{
    public static BookingId New()
    {
        return new BookingId(Guid.NewGuid());
    }
}

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New()
    {
        return new PaymentId(Guid.NewGuid());
    }
}
