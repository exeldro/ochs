using System;

namespace Ochs
{
    public class Ranking
    {
        public virtual Guid Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual int? Rank { get; set; }
        public virtual bool Disqualified { get; set; }
        public virtual bool Forfeited { get; set; }
        public virtual int MatchPoints { get; set; }
        public virtual int Matches { get; set; }
        public virtual int Wins { get; set; }
        public virtual int Draws { get; set; }
        public virtual int Losses { get; set; }
        public virtual double WinRatio => Matches == 0 ? 0 : (double)Wins / Matches;
        public virtual double MatchPointsPerMatch => Matches == 0 ? 0 : (double)MatchPoints / Matches;
        public virtual int DoubleHits { get; set; }
        public virtual double DoubleHitsPerMatch => Matches == 0 ? 0 : (double)DoubleHits / Matches;
        public virtual int Penalties { get; set; }
        public virtual int Warnings { get; set; }
        public virtual int Exchanges { get; set; }
        public virtual int HitsGiven { get; set; }
        public virtual int HitsReceived { get; set; }
        public virtual double HitRatio => Matches == 0 ? 0 : (double)(HitsGiven - HitsReceived) / Matches;
        public virtual int SportsmanshipPoints { get; set; }
        public virtual int Notes { get; set; }
    }
}