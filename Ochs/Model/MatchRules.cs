using System;
using System.Collections;
using System.Collections.Generic;

namespace Ochs
{
    public class MatchRules
    {
        public virtual Guid Id { get; set; }
        public virtual int Rounds { get; set; } = 1;
        public virtual int PointsMax { get; set; } = 0;
        public virtual int ExchangePointsMax { get; set; } = 2;
    
        public virtual TimeSpan TimeMax { get; set; } = new TimeSpan(0, 3, 0);
        public virtual double TimeMaxSeconds => TimeMax.TotalSeconds;
        public virtual int ExchangesMax { get; set; } = 10;
        public virtual int DoubleHitMax { get; set; } = 2;
        public virtual bool SuddenDeath { get; set; } = false;
        public virtual string Name { get; set; }

        public virtual bool BothCanScore { get; set; } = false;  // confirm scores
        public virtual bool SubstractScores { get; set; } = true;

        public virtual bool ConfirmScores =>
            (RecordDoubleHits && DoubleHitScores) || BothCanScore || (RecordAfterBlow && AfterBlowScores);

        public virtual bool RecordSportsmanship { get; set; } = true;
        public virtual bool RecordWarnings { get; set; } = true;
        public virtual bool RecordProtests { get; set; } = false;
        public virtual bool RecordProtestSuccess { get; set; } = false;
        public virtual bool RecordAfterBlow { get; set; } = true;
        public virtual bool AfterBlowScores { get; set; } = false;

        public virtual bool RecordDoubleHits { get; set; } = true;
        public virtual bool DoubleHitScores { get; set; } = false;

        public virtual bool RecordUnclearExchanges { get; set; } = true;

        public virtual bool RecordPenalties { get; set; } = false;
        public virtual int PenaltyScore { get; set; } = -1;

    }

}