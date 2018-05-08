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

        public virtual string FighterBlueOrganization => string.Join(" / ",_match.FighterBlue?.Organizations.Select(x=>x.Name)?? new string[]{});
        public virtual string FighterRedOrganization => string.Join(" / ",_match.FighterRed?.Organizations.Select(x=>x.Name)?? new string[]{});
    }
}