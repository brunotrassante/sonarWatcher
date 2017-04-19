using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarWatcher.Entities
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
