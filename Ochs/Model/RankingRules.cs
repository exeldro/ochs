﻿using System.Collections.Generic;

namespace Ochs
{
    public class RankingRules
    {
        public virtual int WinPoints { get; set; } = 9;
        public virtual int LossPoints { get; set; } = 3;
        public virtual int DrawPoints { get; set; } = 6;
        public virtual int ForfeitPoints { get; set; } = 9;
        public virtual bool RemoveForfeitedFromRanking { get; set; } = true;
        public virtual int DisqualificationPoints { get; set; } = 9;
        public virtual bool RemoveDisqualifiedFromRanking { get; set; } = true;
        public virtual int DoubleReductionThreshold { get; set; } = 0;
        public virtual int DoubleReductionFactor { get; set; } = 2;
        public virtual IList<RankingStat> Sorting { get; set; } = new List<RankingStat>{RankingStat.MatchPoints, RankingStat.HitRatio, RankingStat.DoubleHits, RankingStat.WinRatio};
    }

    public enum RankingStat
    {
        MatchPoints,
        HitRatio,
        DoubleHits,
        WinRatio,
        Penalties,
        Warnings
    }
}