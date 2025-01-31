using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NetStone;
using HtmlAgilityPack;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Component.Text;
using BetterBlacklist.UI;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.VertexShader;
using Lumina.Excel.Sheets;
using static BetterBlacklist.UI.Players;

namespace BetterBlacklist.Network;

internal static class Tomestone
{
    public static void FetchProg(Players.PartyMembers[] partyMembers)
    {
        Task.Run(async () =>
        {
            try
            {
                await PartySearch(partyMembers).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to connect: {ex}");
                MenuBar.Refreshing = false;
            }
        });
    }

    private static async Task PartySearch(Players.PartyMembers[] partyMembers)
    {
        var tasks = new List<Task<Players.PartyMembers>>();
        if (partyMembers != null)
        {
            foreach (var member in partyMembers)
            {
                if (member != null && member.name != null && member.homeWorld != null)
                {
                    //Network.NetStoneFind.GetLodestoneId(member.name, member.homeWorld);
                    tasks.Add(Start(member.name, member.homeWorld));
                }
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                foreach (var player in partyMembers)
                {
                    if (player != null)
                    {
                        if (result.name == "Hidden")
                        {
                            break;
                        }

                        if (result.name == player.name)
                        {
                            player.ultProg = result.ultProg;
                        }
                    }
                }
            }


            Players.partyMembers = partyMembers;
            Players.PartyCount = MenuBar.countMembers;
            MenuBar.Refreshing = false;
        }
    }


    private static async Task<Players.PartyMembers> Start(string playerName, string worldName)
    {
        // tomestone api does not provide an easy way of grabbing prog points, therefore a non efficient method must be used
        // the goal is to eliminate unnecessary network requests by computing certain logic
        // trial and error must occur, tomestone works by retrieving the most recent ultimate by patch prog point
        // taking ucob as an initial starting point therefore makes sense
        // base url: https:///tomestone.gg/character/{loadstone_id}/{name}

        // get loadstone id using NetStone, name and world
        // ~~use NetStone to find achievements relating to completed ults~~ (do not do this, we want to limit total requests, just go to page)
        // find most suitable of finding prog point -> stormblood/endwalker are best as they contain two ults each, ucob/uwu will take priority over dsr/top
        // check specified prog and then visit page with other ultimates for their prog

        // UCOB+UWU:    /progress?encounterCategory=ultimate&encounterExpansion=stormblood
        // TEA:         /progress?encounterCategory=ultimate&encounterExpansion=shadowbringers
        // DSR+TOP:     /progress?encounterCategory=ultimate&encounterExpansion=endwalker
        // FRU:         /progress?encounterCategory=ultimate&encounterExpansion=dawntrail

        // maximum number of requests per player is 4, total maximum per party is 32 requests (that is quite a lot)
        // luckily we can do requests in parallel so although it will be max of 32 requests (40 including lodestone id request), it will take as long as 4 (5) requests which is roughly 2 seconds

        // Searching via lodestone 
        uint lodestoneId = await NetStoneFind.GetLodestoneIdAsync(playerName, worldName);
        PluginLog.Information($"{playerName}, {worldName}: {lodestoneId.ToString()}");

        if (lodestoneId != 0)
        {
            string[] expansions = ["stormblood", "shadowbringers", "endwalker", "dawntrail"];

            var tasks = new List<Task<string>>();
            foreach (var expansion in expansions)
            {
                tasks.Add(FetchExpansionData(expansion, playerName.Replace(" ", "-").ToLower(), lodestoneId));
            }

            var results = await Task.WhenAll(tasks);


            string[] playerProg = [FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString()];

            foreach (var result in results)
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

            Players.PartyMembers partyMembers = new Players.PartyMembers();
            partyMembers.name = playerName;
            partyMembers.homeWorld = worldName;
            partyMembers.ultProg = playerProg;
            PluginLog.Information($"{playerName} Worked");
            return partyMembers;
        }
        PartyMembers hiddenMember = new PartyMembers();
        hiddenMember.name = "Hidden";
        hiddenMember.homeWorld = "Hidden";
        return hiddenMember;
    }

    private static async Task<string> FetchExpansionData(string expansion, string playerName, uint lodestoneId)
    {
        string url = $"https://tomestone.gg/character-contents/{lodestoneId}/{playerName}/progress?encounterCategory=ultimate&encounterExpansion={expansion}";

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
}
