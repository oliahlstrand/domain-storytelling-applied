namespace TE.DomainStorytellingApplied.Domain;

// FÖRKLARING: Alla regelbrott i domänen kastar detta undantag (exception).
// Då kan testerna skilja "en affärsregel bröts" (förväntat fel) från "det är en bugg".
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
