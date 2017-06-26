using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace SonarWatcher.Repository
{
    public class ProjectRepository
    {
        public IEnumerable<Project> FindAllWithNoProjectKey()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["sonarWatcherConnection"].ConnectionString;
            var sql = @"SELECT Id, [Name] 
                        FROM [dbo].[Project]
                        WHERE Project.SonarKey is NULL";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<Project>(sql);
            }
        }
    }
}