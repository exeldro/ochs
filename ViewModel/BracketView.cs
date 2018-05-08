using System.Collections.Generic;

namespace Ochs
{
    public class BracketView
    {
        public virtual IList<PersonView> Fighters { get; set; } = new List<PersonView>();
        public virtual IList<IList<MatchView>> Matches { get; set; } = new List<IList<MatchView>>();
    }
}