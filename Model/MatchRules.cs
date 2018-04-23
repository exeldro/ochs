using System;

namespace Ochs
{
    public class MatchRules
    {
        public virtual Guid Id { get; set; }
        public virtual int Rounds { get; set; } = 1;
        public virtual int PointsMax { get; set; } = 0;
        public virtual int ExchangePointsMax { get; set; } = 2;
        public virtual TimeSpan TimeMax { get; set; } = new TimeSpan(0, 3, 0);
        public virtual int ExhangesMax { get; set; } = 10;
        public virtual int DoubleHitMax { get; set; } = 0;
        public virtual bool SuddenDeath { get; set; } = false;
        public virtual string Name { get; set; }

        public virtual bool SubstractScores { get; set; }  // confirm scores

        public virtual bool RecordWarnings { get; set; }
        public virtual bool RecordProtests { get; set; }
        public virtual bool RecordProtestResults { get; set; }
        public virtual bool RecordAferblow { get; set; }

        public virtual bool RecordDoubleHits { get; set; }
        public virtual bool DoubleHitScores { get; set; }

        public virtual bool RecordUnclearExchanges { get; set; }
        public virtual bool CountUnclearExchange { get; set; }

        public virtual bool RecordPenalties { get; set; }

    }

}