namespace Ochs
{
    public class RulesView
    {
        private readonly MatchRules _matchRules;
        private readonly RankingRules _rankingRules;

        public RulesView(MatchRules matchRules, RankingRules rankingRules)
        {
            _matchRules = matchRules;
            _rankingRules = rankingRules;
        }


    }
}