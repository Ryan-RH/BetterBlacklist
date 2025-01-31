using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using static BetterBlacklist.UI.Players;
using Lumina.Excel.Sheets;
using BetterBlacklist;
using ImGuiNET;

namespace BetterBlacklist.UI;

internal unsafe static class MenuBar
{
    
    public static bool Refreshing = false;
    public static int countMembers = 0;

    public static void Draw()
    {
        using var style = ImRaii.PushStyle(
            ImGuiStyleVar.ItemSpacing,
            new Vector2(6 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        if (ImGui.BeginMenuBar())
        {
            // Refreshing
            using var font = ImRaii.PushFont(UiBuilder.IconFont);
            if (!Refreshing)
            {
                if (ImGui.MenuItem(FontAwesomeIcon.Redo.ToIconString()))
                {
                    if (MainWindow.PartyView)
                    {
                        // Refresh Party
                        var (tempMembers, countMembers) = FindPartyMembers();
                        // Find corresponding lodestone ids and prog points of party members

                        //partyMembers = tempMembers;
                        //PartyCount = countMembers;
                        Refreshing = true;
                        Network.Tomestone.FetchProg(tempMembers);

                    }
                    else
                    {
                        // Refresh History
                    }
                }
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                ImGui.BeginDisabled(true);
                ImGui.MenuItem(FontAwesomeIcon.Spinner.ToIconString());
                ImGui.EndDisabled();
                ImGui.PopStyleColor();
            }
            font.Pop();
            Util.SetHoverTooltip("Refresh");



            // Change View
            font.Push(UiBuilder.IconFont);
            if (MainWindow.PartyView)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(7, 4)); // why does it move?????
                if (ImGui.MenuItem(FontAwesomeIcon.History.ToIconString()))
                {
                    MainWindow.PartyView = false;
                }
                ImGui.PopStyleVar(1);
                font.Pop();
                Util.SetHoverTooltip("History");
            }
            else
            {
                if (ImGui.MenuItem(FontAwesomeIcon.Users.ToIconString()))
                {
                    MainWindow.PartyView = true;
                }
                font.Pop();
                Util.SetHoverTooltip("Party");
            }

            //  Customise
            font.Push(UiBuilder.IconFont);
            if (ImGui.MenuItem(FontAwesomeIcon.Cog.ToIconString()))
            {
                P.ToggleConfigUI();
            }
            font.Pop();
            Util.SetHoverTooltip("Customise");
            ImGui.EndMenuBar();
        }
        else PluginLog.Information("Broken");
    }

    



    private static (PartyMembers[], int) FindPartyMembers()
    {
        var SRGroup = Svc.Party;
        var SRGroupCount = SRGroup.Count;

        var CRGroup = InfoProxyCrossRealm.Instance()->CrossRealmGroups[0];
        var CRGroupCount = CRGroup.GroupMemberCount;

        PartyMembers[] tempMembers = new PartyMembers[8];


        if (SRGroupCount > 0)
        {
            // Same Realm group
            for (int i = 0; i < SRGroupCount && i < 8; i++)
            {
                PartyMembers member = new PartyMembers();
                member.name = SRGroup[i]!.Name.TextValue;

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

            countMembers = SRGroupCount;
        }
        else if (CRGroupCount > 0)
        {
            // Cross Realm Group
            for (int i = 0; i < CRGroupCount && i < 8; i++)
            {
                PartyMembers member = new PartyMembers();
                member.name = CRGroup.GroupMembers[i].NameString;

                var worldSheet = Svc.Data.GetExcelSheet<World>();
                var luminaWorlds = worldSheet.Where(world => !world.InternalName.IsEmpty && world.DataCenter.RowId != 0 && char.IsUpper(world.InternalName.ExtractText()[0]));

                foreach (var world in luminaWorlds)
                {
                    if (world.RowId == CRGroup.GroupMembers[i].HomeWorld)
                    {
                        member.homeWorld = world.InternalName.ExtractText();
                        break;
                    }
                }
                tempMembers[i] = member;
            }

            countMembers = CRGroupCount;
        }
        else
        {
            // Solo Group
            //partyMembers[0].name = "wwwwwwwwww wwwwwwwwww";
            //partyMembers[0].ultProg = [FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Check.ToIconString(), FontAwesomeIcon.Check.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), "P3: 81%%"];

            PartyMembers member = new PartyMembers();
            var localPlayer = Svc.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                member.name = localPlayer.Name.ExtractText();
                member.homeWorld = localPlayer.HomeWorld.Value.InternalName.ExtractText();

                tempMembers[0] = member;
                countMembers = 1;
            }
            

            //uint lodestoneId = Network.NetStoneFind.GetLodestoneId("Andre Bokmerker", "Faerie");
            //PluginLog.Information(lodestoneId.ToString());
        }

        /*
        PartyMembers member = new PartyMembers();
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer != null)
        {
            member.name = localPlayer.Name.ExtractText();
            member.homeWorld = localPlayer.HomeWorld.Value.InternalName.ExtractText();

            tempMembers[0] = member;
            countMembers = 1;
        }*/

        return (tempMembers, countMembers);
    }
}
