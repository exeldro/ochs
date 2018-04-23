using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class MatchWithEventsView : MatchView
    {
        public MatchWithEventsView(Match match) : base(match)
        {
        }

        public virtual IList<MatchEventView> Events => _match.Events.Select(x => new MatchEventView(x)).ToList();
    }
}