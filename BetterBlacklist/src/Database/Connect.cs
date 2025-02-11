using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Database;

public static class Connect
{
    public static SQLiteConnection? Connection;

    public static void CloseConnection()
    {
        if (Connection != null)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Connection.CloseAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Svc.Log.Information($"Failed to Close Connection: {ex}");
                }
            });
        }
    }
}

