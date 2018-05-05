using System.Collections.Generic;

namespace Ochs
{
    public class RankingRules
    {
        public virtual int WinPoints { get; set; } = 9;
        public virtual int LossPoints { get; set; } = 3;
        public virtual int DrawPoints { get; set; } = 6;
        public virtual int ForfeitPoints { get; set; } = 9;
        public virtual int DisqualificationPoints { get; set; } = 9;
        public virtual int DoubleReduction { get; set; } = 2; //three double hits = -1MP
        public virtual IList<RankingStat> Sorting { get; set; } = new List<RankingStat>{RankingStat.Disqualified, RankingStat.MatchPoints, RankingStat.HitRatio, RankingStat.DoubleHits, RankingStat.WinRatio};
    }

    public enum RankingStat
    {
        Disqualified,
        MatchPoints,
        HitRatio,
        DoubleHits,
        WinRatio,
        Penalties,
        Warnings
    }
}