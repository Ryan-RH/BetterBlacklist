using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class PartyList
{
    public class Party
    {
        public int Size = 0;
        public PlayerData[] Members = new PlayerData[8];
    }

    public class ProgUpdate
    {
        public string? Name = null;
        public string? HomeWorld = null;
        public string[] UltimateProg = new string[6];
    }

    public static Party party = new Party();
    private static bool partyCreated = false;

    public static void Draw()
    {
        if (!partyCreated)
        {
            for (int i =0; i< 8;i++)
                party.Members[i] = new PlayerData();
            partyCreated = true;
        }
        UpdateParty();

        // Party Size Display
        ImGui.Text($"Party Size: [{party.Size}/8]");
        //Util.AddTextVertical("Testing", ImGui.GetColorU32(ImColor(255, 0, 0, 255)), 50f);
        float sizeProgress = party.Size / (float)8;
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Util.GetColourRange(sizeProgress)))
            ImGui.ProgressBar(sizeProgress, new Vector2(250, 15), "");


        // Party List Table
        if (ImGui.BeginTable("partyList", 8, ImGuiTableFlags.BordersInner) && Svc.ClientState.LocalPlayer != null)
        {
            // Setup Base Table
            SetupColumns();
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(10, 0));
            ImGui.TableHeadersRow();


            // Fill Table
            for (int i = 0; i < 8; i++)  // Check each party slot
            {
                var member = party.Members[i];
                if (member != null && member.Name != null && member.HomeWorld != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0); // First column - Job Icon
                    ImGui.AlignTextToFramePadding();
                    var jobIcon = Svc.Texture.GetFromGame("ui/icon/062000/0621" + member.Job + ".tex").GetWrapOrEmpty();
                    ImGui.Image(jobIcon.ImGuiHandle, new Vector2(27, 27));

                    ImGui.TableSetColumnIndex(1); // Second column - Name with hyperlink
                    ImGui.AlignTextToFramePadding();
                    if (member.Name == Svc.ClientState.LocalPlayer.Name.TextValue && member.HomeWorld == Svc.ClientState.LocalPlayer.HomeWorld.Value.InternalName)
                        ImGui.Text(member.Name);
                    else
                    {
                        Popups.DrawUserRating(member);
                        string popupName = member.Name.Replace(" ", "+") + "-" + member.HomeWorld;
                        if (member.Rating != null)
                        {
                            float rating = (member.Rating ?? 5.0f) / 10;
                            using var colour = (ImRaii.PushColor(ImGuiCol.Text, Util.GetColourRange(rating)));
                            if (ImGui.Selectable(member.Name))
                                ImGui.OpenPopup($"User##UserSettings_{popupName}");
                            colour.Pop();
                            if (member.Note != null && member.Note != "")
                                Util.SetHoverTooltip(member.Note);
                        }
                        else
                        {
                            float rating = member.Rating ?? 5.0f / 10;
                            using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.678f, 0.847f, 0.902f, 1f)));
                            if (ImGui.Selectable(member.Name))
                                ImGui.OpenPopup($"User##UserSettings_{popupName}");
                            colour.Pop();
                        }
                    }

                    // Next columns for ultimate prog
                    for (int j = 0; j < 6; j++)
                    {
                        ImGui.TableSetColumnIndex(j + 2);
                        ImGui.AlignTextToFramePadding();
                        //Svc.Log.Information(member.UltimateProg[j]);
                        if (member.UltimateProg[j] != null)
                        {
                            if (member.UltimateProg[j] == FontAwesomeIcon.Check.ToIconString())
                            {
                                using var font = ImRaii.PushFont(UiBuilder.IconFont);
                                using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0f, 1f, 0f, 1f)));
                                ImGui.Text(member.UltimateProg[j]);
                                colour.Pop();
                                font.Pop();
                            }
                            else if (member.UltimateProg[j] == FontAwesomeIcon.Times.ToIconString())
                            {
                                using var font = ImRaii.PushFont(UiBuilder.IconFont);
                                using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f)));
                                ImGui.Text(member.UltimateProg[j]);
                                colour.Pop();
                                font.Pop();
                            }
                            else if (member.UltimateProg[j] == FontAwesomeIcon.Minus.ToIconString())
                            {
                                using var font = ImRaii.PushFont(UiBuilder.IconFont);
                                using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f)));
                                ImGui.Text(member.UltimateProg[j]);
                                colour.Pop();
                                font.Pop();
                            }
                            else
                            {
                                if (member.UltimateProg[j].Split(":").Length == 1)
                                {
                                    if (member.UltimateProg[j] == "I1")
                                    {
                                        if (j == 2)
                                            member.UltimateProg[j] = "P1: LC";
                                        else if (j == 3)
                                            member.UltimateProg[j] = "P4: RW";
                                    }
                                    else if (member.UltimateProg[j] == "I2")
                                        member.UltimateProg[j] = "P2: TS";
                                }

                                if (member.UltimateProg[j] != "")
                                {
                                    using var colour = (ImRaii.PushColor(ImGuiCol.Text, DecideColour(member.UltimateProg[j], j)));
                                    ImGui.Text(member.UltimateProg[j]);
                                    colour.Pop();
                                }
                                else
                                {
                                    using var font = ImRaii.PushFont(UiBuilder.IconFont);
                                    using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f)));
                                    ImGui.Text(FontAwesomeIcon.Minus.ToIconString());
                                    colour.Pop();
                                    font.Pop();
                                }
                            }
                        }
                    }
                }
            }
        }
        ImGui.EndTable();
        //ImGui.PopStyleVar();
    }

    private static Vector4 DecideColour(string progPoint, int ultimate)
    {
        string[] parts = progPoint.Split(':');
        string numberPart = parts[0].Substring(1);
        int phase = int.Parse(numberPart);

        int divider = 0;

        switch (ultimate)
        {
            case 0:
            case 1:
            case 5:
                divider = 7;
                break;
            case 2:
                divider = 6;
                break;
            case 3:
                divider = 9;
                break;
            case 4:
                divider = 8;
                break;
        }

        var progress = (float)phase / (float)divider;
        float red = MathF.Min(2 * (1.0f - progress), 1.0f);
        float green = MathF.Min(2 * (progress), 1.0f);

        return new Vector4(red, green, 0.0f, 1.0f);
    }

    private static void SetupColumns()
    {
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 25);
        ImGui.TableSetupColumn("Member", ImGuiTableColumnFlags.WidthFixed, 150);
        ImGui.TableSetupColumn("UCOB", ImGuiTableColumnFlags.WidthFixed, 55);
        ImGui.TableSetupColumn("UWU", ImGuiTableColumnFlags.WidthFixed, 55);
        ImGui.TableSetupColumn("TEA", ImGuiTableColumnFlags.WidthFixed, 55);
        ImGui.TableSetupColumn("DSR", ImGuiTableColumnFlags.WidthFixed, 55);
        ImGui.TableSetupColumn("TOP", ImGuiTableColumnFlags.WidthFixed, 55);
        ImGui.TableSetupColumn("FRU", ImGuiTableColumnFlags.WidthFixed, 55);
    }

    private unsafe static void UpdateParty()
    {
        // Collects party members
        //party.Members.Clear();
        var tempParty = CollectParty();
        party.Size = tempParty.Size;
        for (int i = 0; i < tempParty.Size; i++)
        {
            party.Members[i].Name = tempParty.Members[i].Name;
            party.Members[i].HomeWorld = tempParty.Members[i].HomeWorld;
            party.Members[i].Job = tempParty.Members[i].Job;
        }

        UpdateRating();

    }

    public static void UpdateRating()
    {
        // called when new note/rating submitted
        // called when database update
        // directly goes into party.Members[i].Note and .Rating
        //  and then changes them so that it can match name and world

        for (int i = 0; i < 8; i++)
        {
            var member = party.Members[i];
            if (member != null && member.Name != null && member.HomeWorld != null)
            {
                var (Rating, Note) = Database.SearchPlayerRating(member.Name, member.HomeWorld);
                if (Rating != -1.0f)
                {
                    party.Members[i].Rating = Rating;
                    party.Members[i].Note = Note;
                }

            }
        }


    }

    private unsafe static Party CollectParty()
    {
        var SRGroup = Svc.Party;
        var SRGroupCount = SRGroup.Count;

        var CRGroup = InfoProxyCrossRealm.Instance()->CrossRealmGroups[0];
        var CRGroupCount = CRGroup.GroupMemberCount;

        Party tempParty = new Party();

        if (SRGroupCount > 0) // Same Realm party detected
        {
            for (int i = 0; i < SRGroupCount; i++)
            {
                PlayerData member = new PlayerData();
                member.Name = SRGroup[i]!.Name.TextValue;
                var worldSheet = Svc.Data.GetExcelSheet<World>().Where(world => !world.InternalName.IsEmpty && world.DataCenter.RowId != 0 && char.IsUpper(world.InternalName.ExtractText()[0]));
                member.Job = SRGroup[i]!.ClassJob.RowId;

                foreach (var world in worldSheet)
                {
                    if (world.RowId == SRGroup[i]!.World.RowId)
                    {
                        member.HomeWorld = world.InternalName.ExtractText();
                        break;
                    }
                }
                tempParty.Members[i] = member;
            }
            tempParty.Size = SRGroupCount;
        }
        else if (CRGroupCount > 0) // Cross Realm party detected
        {
            for (int i = 0; i < CRGroupCount; i++)
            {
                PlayerData member = new PlayerData();
                member.Name = CRGroup.GroupMembers[i].NameString;
                var worldSheet = Svc.Data.GetExcelSheet<World>().Where(world => !world.InternalName.IsEmpty && world.DataCenter.RowId != 0 && char.IsUpper(world.InternalName.ExtractText()[0]));
                member.Job = CRGroup.GroupMembers[i].ClassJobId;

                foreach (var world in worldSheet)
                {
                    if (world.RowId == CRGroup.GroupMembers[i].HomeWorld)
                    {
                        member.HomeWorld = world.InternalName.ExtractText();
                        break;
                    }
                }
                tempParty.Members[i] = member;
            }
            tempParty.Size = CRGroupCount;
        }
        else // Solo party detected
        {
            PlayerData member = new PlayerData();
            var localPlayer = Svc.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                member.Name = localPlayer.Name.TextValue;
                member.HomeWorld = localPlayer.HomeWorld.Value.InternalName.ExtractText();
                member.Job = localPlayer.ClassJob.RowId;

                tempParty.Members[0] = member;
                tempParty.Size = 1;
            }
        }
        return tempParty;
    }
}
