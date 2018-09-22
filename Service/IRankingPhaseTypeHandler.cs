using System.Collections.Generic;

namespace Ochs
{
    public interface IRankingPhaseTypeHandler
    {
        int? GetRank(Person rankingPerson, IList<Match> matches);
    }
}