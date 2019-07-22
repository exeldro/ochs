using System;

namespace Ochs
{
    public class RankingView
    {
        private readonly Ranking _ranking;

        public RankingView(Ranking ranking)
        {
            _ranking = ranking;
        }

        public virtual Guid Id => _ranking.Id;
        public virtual PersonView Fighter => new PersonView(_ranking.Person);

        public virtual int? Rank => _ranking.Rank;
        public virtual bool Disqualified => _ranking.Disqualified;
        public virtual int MatchPoints => _ranking.MatchPoints;
        public virtual int Matches => _ranking.Matches;
        public virtual int Wins => _ranking.Wins;
        public virtual int Draws => _ranking.Draws;
        public virtual int Losses => _ranking.Losses;
        public virtual double WinRatio => _ranking.WinRatio;
        public virtual double MatchPointsPerMatch => _ranking.MatchPointsPerMatch;
        public virtual int DoubleHits => _ranking.DoubleHits;
        public virtual double DoubleHitsPerMatch => _ranking.DoubleHitsPerMatch;
        public virtual int Penalties => _ranking.Penalties;
        public virtual int Warnings => _ranking.Warnings;
        public virtual int Exchanges => _ranking.Exchanges;
        public virtual int HitsGiven => _ranking.HitsGiven;
        public virtual int HitsReceived => _ranking.HitsReceived;
        public virtual double HitRatio => _ranking.HitRatio;
        public virtual int SportsmanshipPoints => _ranking.SportsmanshipPoints;
        public virtual int Notes => _ranking.Notes;
    }
}