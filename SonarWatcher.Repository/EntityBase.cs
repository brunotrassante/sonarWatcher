using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarWatcher.Entities
{
    public abstract class EntityBase
    {
        const int _defaultid = default(int);
        /// <summary>
        /// Gets ID.
        /// </summary>
        public int Id { get; protected set; }
    }   
}
