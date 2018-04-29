using System;
using System.Runtime.InteropServices;

namespace Ochs
{
    public class MatchView
    {
        protected Match _match;

        public MatchView(Match match)
        {
            _match = match;
        }

        public virtual Guid Id => _match.Id;
        public virtual string Name => _match.Name;

        public virtual string Location => !string.IsNullOrWhiteSpace(_match.Location)
            ? _match.Location
            : (!string.IsNullOrWhiteSpace(_match.Pool?.Location) ? _match.Pool.Location : _match.Phase?.Location);
        public virtual string FighterBlue => _match.FighterBlue?.ToString();
        public virtual Guid? FighterBlueId => _match.FighterBlue?.Id;
        public virtual string FighterBlueCountryCode => _match.FighterBlue?.CountryCode;
        public virtual string FighterRed => _match.FighterRed?.ToString();
        public virtual Guid? FighterRedId => _match.FighterRed?.Id;
        public virtual string FighterRedCountryCode => _match.FighterRed?.CountryCode;
        public virtual string Result => _match.Result.ToString();
        public virtual string Organization => _match.Competition?.Organization?.Name;
        public virtual Guid? OrganizationId => _match.Competition?.Organization?.Id;
        public virtual string Competition => _match.Competition?.Name;
        public virtual Guid? CompetitionId => _match.Competition?.Id;
        public virtual string Phase => _match.Phase?.Name;
        public virtual Guid? PhaseId => _match.Phase?.Id;
        public virtual string Pool => _match.Pool?.Name;
        public virtual Guid? PoolId => _match.Pool?.Id;
        public virtual int ScoreRed => _match.ScoreRed;
        public virtual int ScoreBlue => _match.ScoreBlue;
        public virtual int ExchangeCount => _match.ExchangeCount;
        public virtual int DoubleCount => _match.DoubleCount;
        public virtual int Round => _match.Round;
        public virtual bool TimeRunning => _match.TimeRunningSince.HasValue;
        public virtual double Time => _match.Time.TotalSeconds;
        public virtual double LiveTime => _match.LiveTime.TotalSeconds;
        public virtual DateTime? TimeRunningSince => _match.TimeRunningSince;
        public virtual DateTime? PlannedDateTime => _match.PlannedDateTime;
        public virtual bool Planned => _match.Planned;
        public virtual DateTime? StartedDateTime => _match.StartedDateTime;
        public virtual bool Started => _match.Started;
        public virtual DateTime? FinishedDateTime => _match.FinishedDateTime;
        public virtual bool Finished => _match.Finished;
        public virtual DateTime? ValidatedDateTime => _match.ValidatedDateTime;
        public virtual bool Validated => _match.Validated;
    }
}