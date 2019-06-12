using System;
using System.Collections;
using System.Collections.Generic;

namespace Ochs
{
    public class MatchRules
    {
        public virtual Guid Id { get; set; }
        //public virtual Organization Organization { get; set; }
        public virtual int Rounds { get; set; } = 1;
        public virtual int PointsMax { get; set; } = 0;
        public virtual int ExchangePointsMax { get; set; } = 3;
    
        public virtual TimeSpan TimeMax { get; set; } = new TimeSpan(0, 3, 0);
        public virtual double TimeMaxSeconds {
            get => TimeMax.TotalSeconds;
            set => TimeMax = new TimeSpan(0, 0, (int) value);
        }
        public virtual int ExchangesMax { get; set; } = 0;
        public virtual int DoubleHitMax { get; set; } = 2;
        //public virtual bool SuddenDeath { get; set; } = false;
        public virtual string Name { get; set; } = "DLC";

        public virtual bool BothCanScore { get; set; } = true;  // confirm scores
        public virtual bool SubtractPoints { get; set; } = true;

        public virtual bool ConfirmScores =>
            (RecordDoubleHits && DoubleHitScores) || BothCanScore || (RecordAfterblow && AfterblowScores);

        public virtual bool RecordSportsmanship { get; set; } = false;
        public virtual bool RecordWarnings { get; set; } = true;
        public virtual bool RecordProtests { get; set; } = false;
        public virtual bool RecordAfterblow { get; set; } = false;
        public virtual bool AfterblowScores { get; set; } = false;

        public virtual bool RecordDoubleHits { get; set; } = true;
        public virtual bool DoubleHitScores { get; set; } = false;

        public virtual bool RecordUnclearExchanges { get; set; } = true;

        public virtual bool RecordPenalties { get; set; } = true;
        public virtual int PenaltyPoints { get; set; } = 1;

    }

}