using System;

namespace Ochs
{
    public class MatchEventView
    {
        private readonly MatchEvent _matchEvent;

        public MatchEventView(MatchEvent matchEvent, int? exchangeNumber)
        {
            _matchEvent = matchEvent;
            ExchangeNumber = exchangeNumber;
        }
        public virtual Guid Id => _matchEvent.Id;
        public virtual string Type => _matchEvent.Type.ToString();
        public virtual int Round => _matchEvent.Round;
        public virtual double Time => _matchEvent.MatchTime.TotalSeconds;
        public virtual int PointsBlue => _matchEvent.PointsBlue;
        public virtual int PointsRed => _matchEvent.PointsRed;
        public virtual DateTime CreatedDateTime => _matchEvent.CreatedDateTime;
        public virtual string Note => _matchEvent.Note;
        public virtual int? ExchangeNumber { get; }
    }
}