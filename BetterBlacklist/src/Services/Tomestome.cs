using BetterBlacklist.UI;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using NetStone;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BetterBlacklist.Services;

public static class Tomestone
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
            Svc.Log.Error("Failed to setup client");
        }
    }

    public class PlayerInfo
    {
        public uint lodestoneId = 0;
        public string Name = "Hidden";
        public string worldName = "Hidden";
    }

    public static async Task FetchPartyProg()
    {
        var tasksLodestone = new List<Task<PlayerInfo>>();
        var fetchParty = PartyList.party;
        foreach (var member in fetchParty.Members)
        {
            if (member != null && member.Name != null && member.HomeWorld != null)
            {
                tasksLodestone.Add(FetchLodestoneId(member.Name, member.HomeWorld));
            }
        }
        var lodestoneInfo = await Task.WhenAll(tasksLodestone);

        var tasksProg = new List<Task>();
        foreach (var info in lodestoneInfo)
        {
            tasksProg.Add(ProgManager(info));
        }
        await Task.WhenAll(tasksProg);
    }

    private static async Task<PlayerInfo> FetchLodestoneId(string playerName, string worldName)
    {
        // This should fetch from tomestone itself. NetStone doesn't work if lodestone is private
        //  But that doesn't mean someone's tomestone is private too!
        //  I can also look into grabbing additional info to speed up the time it takes to fetch
        //  I will keep it to NetStone currently though.
        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.Name = playerName;
        playerInfo.worldName = worldName;
        try
        {
            var searchResponse = await LodestoneClient.SearchCharacter(new NetStone.Search.Character.CharacterSearchQuery
            {
                CharacterName = playerName,
                World = worldName
            }).ConfigureAwait(false);

            if (searchResponse == null)
            {
                Svc.Log.Error("Failed1");
                return playerInfo;
            }

            var result = searchResponse.Results.FirstOrDefault(entry => entry.Name == playerName);
            playerInfo.lodestoneId = result != null ? Convert.ToUInt32(result.Id) : 0;
            return playerInfo;
        }
        catch (HttpRequestException ex)
        {
            Svc.Log.Error("Failed2");
            return playerInfo;
        }
    }

    private static async Task ProgManager(PlayerInfo playerInfo)
    {
        string[] playerProg = [ FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString() ];
        string[] hiddenProg = [ FontAwesomeIcon.Minus.ToIconString(), FontAwesomeIcon.Minus.ToIconString(), FontAwesomeIcon.Minus.ToIconString(), FontAwesomeIcon.Minus.ToIconString(), FontAwesomeIcon.Minus.ToIconString(), FontAwesomeIcon.Minus.ToIconString() ];

        if (playerInfo.lodestoneId != 0)
        {
            string[] expansions = ["stormblood", "shadowbringers", "endwalker", "dawntrail"];

            var tasks = new List<Task<string>>();
            foreach (var expansion in expansions)
            {
                tasks.Add(FetchUltimateProg(expansion, playerInfo));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                try
                {
                    var jsonResponse = JObject.Parse(result);
                    var encounters = jsonResponse["encounters"] as JObject;
                    if (encounters != null)
                    {
                        var selectedExpansion = encounters["selectedExpansion"] as JObject;
                        if (selectedExpansion != null)
                        {
                            var ultimates = selectedExpansion["ultimate"] as JArray;
                            if (ultimates != null)
                            {
                                foreach (var ultimate in ultimates)
                                {
                                    var compactName = ultimate["compactName"]?.ToString();
                                    var progression = ultimate["progression"] as JObject;
                                    var contentText = FontAwesomeIcon.Check.ToIconString();
                                    if (progression != null)
                                    {
                                        var percentage = progression["percent"]?.ToString();
                                        //PluginLog.Information($"{compactName}: {percentage!}");
                                        contentText = Decipher(percentage!);
                                    }

                                    switch (compactName)
                                    {
                                        case "UCOB":
                                            playerProg[0] = contentText!;
                                            break;
                                        case "UWU":
                                            playerProg[1] = contentText!;
                                            break;
                                        case "TEA":
                                            playerProg[2] = contentText!;
                                            break;
                                        case "DSR":
                                            playerProg[3] = contentText!;
                                            break;
                                        case "TOP":
                                            playerProg[4] = contentText!;
                                            break;
                                        case "FRU":
                                            playerProg[5] = contentText!;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    AssignUltimateProg(playerInfo, hiddenProg);
                }
            }
            AssignUltimateProg(playerInfo, playerProg);
        }
        else
        {
            AssignUltimateProg(playerInfo, hiddenProg);
        }

    }

    private static string Decipher(string text)
    {
        string[] parts = text.Split(' ');
        if (parts.Length > 1)
            return $"{parts[1]}: {parts[0]}%";
        else
        {
            return text;
        }
    }

    private static async Task<string> FetchUltimateProg(string expansion, PlayerInfo playerinfo)
    {
        string url = $"https://tomestone.gg/character-contents/{playerinfo.lodestoneId}/{playerinfo.Name.Replace(" ", "-").ToLower()}/progress?encounterCategory=ultimate&encounterExpansion={expansion}";
        
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        return "";

    }

    private static void AssignUltimateProg(PlayerInfo playerInfo, string[] ultimateProg)
    {
        for (int i = 0; i < PartyList.party.Size; i++)
        {
            var member = PartyList.party.Members[i];
            if (member != null && member.Name == playerInfo.Name && member.HomeWorld == playerInfo.worldName)
            {
                PartyList.party.Members[i].UltimateProg = ultimateProg;
            }
        }
    }
}
