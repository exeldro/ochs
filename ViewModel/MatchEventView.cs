using System;

namespace Ochs
{
    public class MatchEventView
    {
        private MatchEvent _matchEvent;

        public MatchEventView(MatchEvent matchEvent)
        {
            _matchEvent = matchEvent;
        }
        public virtual Guid Id => _matchEvent.Id;
        public virtual string Type => _matchEvent.Type.ToString();
        public virtual int Round => _matchEvent.Round;
        public virtual double Time => _matchEvent.MatchTime.TotalSeconds;
        public virtual int PointsBlue => _matchEvent.PointsBlue;
        public virtual int PointsRed => _matchEvent.PointsRed;
        public virtual DateTime CreatedDateTime => _matchEvent.CreatedDateTime;
    }
}