namespace TE.DomainStorytellingApplied.Domain;

// En bokare är en av dessa tre. Typen styr priset (kommunen betalar inte).
// Förenkling: vi modellerar inte personen eller kundregistret, bara det bokningen behöver veta.
public enum BookerType
{
    Municipality,
    Association,
    PrivatePerson
}
