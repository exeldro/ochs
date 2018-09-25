using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class MatchWithEventsView : MatchView
    {
        public MatchWithEventsView(Match match) : base(match)
        {
            var exchanges = 0;
            Events = _match.Events.OrderBy(x=>x.MatchTime).ThenBy(x=>x.CreatedDateTime).Select(x => new MatchEventView(x, x.IsExchange?++exchanges:(int?)null)).OrderByDescending(x=>x.Time).ThenByDescending(x=>x.CreatedDateTime).ToList();
        }
 
        public virtual IList<MatchEventView> Events { get; }
        public virtual string FighterBlueOrganization => string.Join(" / ",_match.FighterBlue?.Organizations.Select(x=>x.Name)?? new string[]{});
        public virtual string FighterRedOrganization => string.Join(" / ",_match.FighterRed?.Organizations.Select(x=>x.Name)?? new string[]{});
    }
}