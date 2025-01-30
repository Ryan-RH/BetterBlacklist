using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

internal unsafe static class Players
{
    public class PartyMembers
    {
        public string? name = null;
        public string? homeWorld = null;
        public string[] ultProg = new string[6];
    }


    public static int PartyCount = 0;
    public static PartyMembers[] partyMembers = new PartyMembers[8];


    internal static void PartyDraw()
    {

        float progress = PartyCount / (float)8;
        ImGui.Text($"Party Size: [{PartyCount}/8]");
        ImGui.SameLine();
        float red = MathF.Min(2 * (1.0f - progress), 1.0f);
        float green = MathF.Min(2 * progress, 1.0f);
        //float red = 1.0f - progress;
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, new Vector4(red, green, 0.0f, 1.0f)))
        {
            ImGui.ProgressBar(progress, new Vector2(250, 15), "");
        }



        if (ImGui.BeginTable("partyList", 8, ImGuiTableFlags.BordersInner))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 25);
            ImGui.TableSetupColumn("Member", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("UCOB", ImGuiTableColumnFlags.WidthFixed, 55);
            ImGui.TableSetupColumn("UWU", ImGuiTableColumnFlags.WidthFixed, 55);
            ImGui.TableSetupColumn("TEA", ImGuiTableColumnFlags.WidthFixed, 55);
            ImGui.TableSetupColumn("DSR", ImGuiTableColumnFlags.WidthFixed, 55);
            ImGui.TableSetupColumn("TOP", ImGuiTableColumnFlags.WidthFixed, 55);
            ImGui.TableSetupColumn("FRU", ImGuiTableColumnFlags.WidthFixed, 55);

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0f, 5f));

            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(10, 0));
            ImGui.TableHeadersRow();


            foreach (var member in partyMembers)
            {
                if (member?.name != null) // Only render initialized members
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0); // Set column 7
                    Vector2 size = new Vector2(5, 5);
                    if (Util.DrawButtonIcon(FontAwesomeIcon.UserCog, size))
                    {
                        // Handle the button action here, e.g., add a member or open a menu
                    }

                    // Member name column (first column)
                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(member.name);

                    // Render ultProg columns (UCOB, UWU, TEA, DSR, TOP, FRU)
                    for (int i = 0; i < member.ultProg.Length; i++)
                    {
                        ImGui.TableSetColumnIndex(i + 2); // Columns 1 to 6 for UCOB, UWU, etc.
                        ImGui.AlignTextToFramePadding();
                        if (member.ultProg[i] == FontAwesomeIcon.Check.ToIconString())
                        {
                            using var font = ImRaii.PushFont(UiBuilder.IconFont);
                            using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0f, 1f, 0f, 1f)));
                            ImGui.Text(member.ultProg[i]);
                            colour.Pop();
                            font.Pop();
                        }
                        else if (member.ultProg[i] == FontAwesomeIcon.Times.ToIconString())
                        {
                            using var font = ImRaii.PushFont(UiBuilder.IconFont);
                            using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f)));
                            ImGui.Text(member.ultProg[i]);
                            colour.Pop();
                            font.Pop();
                        }
                        else
                        {
                            using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(1f, 0.647f, 0f, 1f)));
                            ImGui.Text(member.ultProg[i]);
                            colour.Pop();
                        }
                    }
                }
            }
        }
        ImGui.EndTable();
        ImGui.PopStyleVar();

        /*
        foreach (var member in Svc.Party)
        {
            ImGui.Text(member.Name.ToString());
            ImGui.SameLine();
            ImGui.SetCursorPosX(200);
            ImGui.PushItemWidth(100.0f);
            ImGui.SliderFloat("", ref value, -10, 10, "%.1f");
            ImGui.PopItemWidth();
        }*/
    }

    internal static void HistoryDraw()
    {


    }
}
