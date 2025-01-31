using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BetterBlacklist.UI.Players;

namespace BetterBlacklist.FWM;

internal unsafe static class FrameworkManager
{

    private static bool Collected = false;

    public static void Framework_Update(object framework)
    {
        var currentContentId = GameMain.Instance()->CurrentContentFinderConditionId;
        var SRGroup = Svc.Party;
        var SRGroupCount = SRGroup.Count;

        if (currentContentId != 0 && !Collected && SRGroupCount > 0) 
        {
            // collect that mf
            PartyMembers[] tempMembers = new PartyMembers[8];

            if (SRGroupCount > 0)
            {
                for (int i = 1; i < SRGroupCount && i < 8; i++)
                {
                    PartyMembers member = new PartyMembers();
                    member.name = SRGroup[i]!.Name.TextValue;
                    member.job = SRGroup[i]!.ClassJob.Value.ToString();

                    var worldSheet = Svc.Data.GetExcelSheet<World>();
                    var luminaWorlds = worldSheet.Where(world => !world.InternalName.IsEmpty && world.DataCenter.RowId != 0 && char.IsUpper(world.InternalName.ExtractText()[0]));

                    foreach (var world in luminaWorlds)
                    {
                        if (world.RowId == SRGroup[i]!.World.RowId)
                        {
                            member.homeWorld = world.InternalName.ExtractText();
                            break;
                        }
                    }
                    tempMembers[i] = member;
                }
            }
            foreach (var member in tempMembers)
            {
                if (member != null && member.name != null && member.homeWorld != null && member.job != null)
                {
                    // Search database, check if user exists
                    // If yes:
                    //      Extract previous entry's rating and note, delete previous entry, make new entry
                    // If no:
                    //      Make new entry

                    var territoryId = Svc.ClientState.TerritoryType;
                    if (Svc.Data.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                    {

                        var (rating, note) = Database.SearchPlayer(member.name, member.homeWorld, true);

                        if (rating != -1f)
                        {
                            Database.AddPlayer(member.name, member.homeWorld, territoryRow.PlaceName.Value.Name.ExtractText(), member.job, 5.0f, "");
                        }
                        else
                        {
                            Database.AddPlayer(member.name, member.homeWorld, territoryRow.PlaceName.Value.Name.ExtractText(), member.job, rating, note);
                        }
                    }
                    


                    // Add to database
                }
            }



            Collected = true;
        }


        if (currentContentId == 0)
            Collected = false;
    }

}
