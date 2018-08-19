using System;

namespace Ochs
{
    public class CompetitionView
    {
        protected Competition _competition;

        public CompetitionView(Competition competition)
        {
            _competition = competition;
        }

        public virtual Guid Id => _competition.Id;
        public Guid CompetitionId => _competition.Id;
        public virtual string Name => _competition.Name;
        public virtual string Organization => _competition.Organization?.Name;
        public virtual DateTime Start => _competition.Start;
    }
}