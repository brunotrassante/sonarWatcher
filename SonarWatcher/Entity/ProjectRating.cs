namespace SonarWatcher.Entity
{
    public class ProjectRating
    {
        public Rating Reliability { get; private set; }
        public Rating Security { get; private set; }
        public Rating Maintainability { get; private set; }

        public ProjectRating(ushort reliability, ushort security, ushort maintainability)
        {
            this.Reliability = new Rating(reliability);
            this.Security = new Rating(security);
            this.Maintainability = new Rating(maintainability);
        }
    }
}
