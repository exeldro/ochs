using System;

namespace Ochs
{
    public class PhaseView
    {
        protected Phase _phase;
        public PhaseView(Phase phase)
        {
            _phase = phase;
        }

        public Guid Id => _phase.Id;
        public string Name => _phase.Name;
        public string Location => _phase.Location;
        public string PhaseType => _phase.PhaseType.ToString();
        public bool Elimination => _phase.Elimination;

        public string Competition => _phase.Competition?.Name;
        public Guid? CompetitionId => _phase.Competition?.Id;

    }
}