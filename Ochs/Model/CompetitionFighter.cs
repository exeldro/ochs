namespace Ochs
{
    public class CompetitionFighter
    {
        public virtual Competition Competition { get; set; }
        public virtual Person Fighter { get; set; }
        public virtual double? Seed { get; set; } = null;
        public override bool Equals(object value) => value is CompetitionFighter cf && cf.Competition?.Id == Competition?.Id && cf.Fighter?.Id == Fighter?.Id;

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + (Competition?.GetHashCode()??0);
                hash = hash * 23 + (Fighter?.GetHashCode()??0);
                return hash;
            }
        }
    }
}