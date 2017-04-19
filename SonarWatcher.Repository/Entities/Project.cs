
namespace SonarWatcher.Repository
{
    public class Project : EntityBase
    {
        private Project()
        {
        }

        public Project(string name, string sonarKey)
        {
            this.Name = name;
            this.SonarKey = sonarKey;
        }

        public string Name { get; private set; }
        public string SonarKey { get; private set; }
    }
}
