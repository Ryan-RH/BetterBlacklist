using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using static BetterBlacklist.UI.Players;

namespace BetterBlacklist.UI;

internal unsafe static class MenuBar
{
    public static bool PartyView = true;

    public static void Draw()
    {
        using var style = ImRaii.PushStyle(
            ImGuiStyleVar.ItemSpacing,
            new Vector2(6 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        if (ImGui.BeginMenuBar())
        {
            using var font = ImRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.MenuItem(FontAwesomeIcon.Redo.ToIconString()))
            {
                if (PartyView)
                {
                    var SRGroup = Svc.Party;
                    var SRGroupCount = SRGroup.Count;

                    var CRGroup = InfoProxyCrossRealm.Instance()->CrossRealmGroups[0];
                    var CRGroupCount = CRGroup.GroupMemberCount;

                    if (SRGroupCount > 0)
                    {
                        for (int i = 0; i < SRGroupCount && i < 8; i++)
                        {
                            partyMembers[i] = new PartyMembers();
                            partyMembers[i].name = SRGroup[i]!.Name.TextValue;
                            partyMembers[i].homeWorld = SRGroup[i]!.World.Value.Name.ExtractText();
                            partyMembers[i].ultProg = ["ucob", "uwu", "tea", "dsr", "top", "fru"];
                        }

                        PartyCount = SRGroupCount;
                    }
                    else if (CRGroupCount > 0)
                    {
                        for (int i = 0; i < CRGroupCount && i < 8; i++)
                        {
                            partyMembers[i] = new PartyMembers();
                            partyMembers[i].name = CRGroup.GroupMembers[i].NameString;
                            partyMembers[i].homeWorld = CRGroup.GroupMembers[i].HomeWorld.ToString();
                            partyMembers[i].ultProg = ["ucob2", "uwu2", "tea2", "dsr2", "top2", "fru2"];
                        }

                        PartyCount = CRGroupCount;
                    }
                    else
                    {
                        partyMembers[0] = new PartyMembers();
                        //partyMembers[0].name = "wwwwwwwwww wwwwwwwwww";
                        partyMembers[0].name = "Andre Bokmerker";
                        partyMembers[0].homeWorld = "Faerie";
                        partyMembers[0].ultProg = [FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Check.ToIconString(), FontAwesomeIcon.Check.ToIconString(), FontAwesomeIcon.Times.ToIconString(), FontAwesomeIcon.Times.ToIconString(), "P3: 81%%"];
                        PartyCount = 1;
                    }
                }
            }

            font.Pop();


            Util.SetHoverTooltip("Refresh");
            font.Push(UiBuilder.IconFont);
            if (PartyView)
            {
                if (ImGui.MenuItem(FontAwesomeIcon.History.ToIconString()))
                {
                    PartyView = false;
                }

                font.Pop();
                Util.SetHoverTooltip("History");
            }
            else
            {
                if (ImGui.MenuItem(FontAwesomeIcon.PeopleGroup.ToIconString()))
                {
                    PartyView = true;
                }
                font.Pop();
                Util.SetHoverTooltip("Party");
            }

            font.Push(UiBuilder.IconFont);
            if (ImGui.MenuItem(FontAwesomeIcon.Cog.ToIconString()))
            {

            }
            font.Pop();
            Util.SetHoverTooltip("Customise");
            ImGui.EndMenuBar();
        }
        else PluginLog.Information("Broken");
    }
}
