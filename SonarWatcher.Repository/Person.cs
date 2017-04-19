using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarWatcher.Entities
{
    public class Person : EntityBase
    {
        private Person()
        {
        }

        public Person(string name, string email, int role)
        {
            this.Name = name;
            this.Email = email;
            this.Role = (RoleEnum)role;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public RoleEnum Role { get; private set; }
    }
}
