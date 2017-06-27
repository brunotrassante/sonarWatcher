using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace SonarWatcher.Repository
{
    public class PersonRepository
    {
        public IEnumerable<Person> FindAllByProjectKey(string sonarKey)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["sonarWatcherConnection"].ConnectionString;
            var sql = @"SELECT [Person].Id, [Person].Name, [Person].Email,[ProjectPersonRole].RoleId as Role
                        FROM [Person] 
                        Join [ProjectPersonRole] ON [ProjectPersonRole].PersonId = [Person].Id
                        Join [Project] on [Project].Id = [ProjectPersonRole].ProjectId
                        WHERE[Project].SonarKey = @SonarKey AND [Project].Active = 1 AND [ProjectPersonRole].Active = 1";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<Person>(sql, new { SonarKey = sonarKey });
            }
        }

        public IEnumerable<Person> FindAllByProjectId(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["sonarWatcherConnection"].ConnectionString;
            var sql = @"SELECT [Person].Id, [Person].Name, [Person].Email,[ProjectPersonRole].RoleId as Role
                        FROM [Person] 
                        Join [ProjectPersonRole] ON [ProjectPersonRole].PersonId = [Person].Id
                        Join [Project] on [Project].Id = [ProjectPersonRole].ProjectId
                        WHERE[Project].Id = @Id AND [Project].Active = 1 AND [ProjectPersonRole].Active = 1";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<Person>(sql, new { Id = id });
            }
        }
    }
}