using Microsoft.Data.SqlClient;
using System.Data;

namespace DAABLike.Tests
{
    partial class DatabaseAccessTest
    {
        [Fact]
        public async Task ParameterCache()
        {
            void DoWork()
            {
                var db = new Database(ConnectionString, SqlClientFactory.Instance);
                var rand = new Random();
                for (int i = 0; i < 1000; i++)
                {
                    using var command = new SqlCommand();
                    command.CommandType = CommandType.StoredProcedure;

                    var num = rand.Next(10);
                    if (num == 0)
                    {
                        Database.ClearParameterCache();
                    }
                    else if (num >= 1 && num <= 3)
                    {
                        command.CommandText = "GetTestTableData";
                        db.AssignParameters(command, [1, 2]);
                        Assert.Equal("@FromId", command.Parameters[1].ParameterName);
                        Assert.Equal(SqlDbType.Int, command.Parameters[1].SqlDbType);
                        Assert.Equal("@ToId", command.Parameters[2].ParameterName);
                        Assert.Equal(SqlDbType.Int, command.Parameters[2].SqlDbType);
                    }
                    else if (num >= 4 && num <= 6)
                    {
                        command.CommandText = "GetTestData";
                        db.AssignParameters(command, [2]);
                        Assert.Equal("@Id", command.Parameters[1].ParameterName);
                        Assert.Equal(SqlDbType.Int, command.Parameters[1].SqlDbType);
                    }
                    else
                    {
                        command.CommandText = "UpdateNonQueryTableData";
                        db.AssignParameters(command, [1, "test"]);
                        Assert.Equal("@Id", command.Parameters[1].ParameterName);
                        Assert.Equal(SqlDbType.Int, command.Parameters[1].SqlDbType);
                        Assert.Equal("@Value", command.Parameters[2].ParameterName);
                        Assert.Equal(SqlDbType.NVarChar, command.Parameters[2].SqlDbType);
                    }
                }
            }

            Task[] tasks = new Task[50];
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(DoWork, TestContext.Current.CancellationToken);

            await Task.WhenAll(tasks);
        }
    }
}
