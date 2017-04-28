namespace SonarWatcher.Repository
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
            this.Role = (Role)role;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public Role Role { get; private set; }
    }
}
