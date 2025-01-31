using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BetterBlacklist.UI;
using Lumina.Excel.Sheets;
using NetStone;

namespace BetterBlacklist.Network;

internal static class NetStoneFind
{
    private static LodestoneClient LodestoneClient = null!;

    public static void Start()
    {
        SetupClient().Wait();
    }

    public static async Task SetupClient()
    {
        try
        {
            LodestoneClient = await LodestoneClient.GetClientAsync().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            PluginLog.Error("Failed to setup client");
        }
    }

    public static void GetLodestoneId(string playerName, string worldName)
    {
        uint lodestoneId = 0;

        Task.Run(async () =>
        {
            try
            {
                lodestoneId = await GetLodestoneIdAsync(playerName, worldName).ConfigureAwait(false);
                PluginLog.Information(lodestoneId.ToString());
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to open lodestone profile {ex}");
            }
        });
    }

    public static async Task<uint> GetLodestoneIdAsync(string playerName, string worldName)
    {
        try
        {
            var searchResponse = await LodestoneClient.SearchCharacter(new NetStone.Search.Character.CharacterSearchQuery
            {
                CharacterName = playerName,
                World = worldName
            }).ConfigureAwait(false);

            if (searchResponse == null)
            {
                PluginLog.Error("Failed1");
                return 0;
            }

            var result = searchResponse.Results.FirstOrDefault(entry => entry.Name == playerName);
            return result != null ? Convert.ToUInt32(result.Id) : 0;
        }
        catch (HttpRequestException ex)
        {
            PluginLog.Error("Failed2");
            return 0;
        }
    }
}
