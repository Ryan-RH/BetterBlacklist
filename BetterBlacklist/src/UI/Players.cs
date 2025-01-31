using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.VertexShader;
using static System.Net.Mime.MediaTypeNames;

namespace BetterBlacklist.UI;

internal unsafe static class Players
{
    public class PartyMembers
    {
        public string? name = null;
        public string? homeWorld = null;
        public string[] ultProg = new string[6];
        public string? job = null;
    }


    public static int PartyCount = 0;
    public static PartyMembers[] partyMembers = new PartyMembers[8];
    public static bool PopupWindow = false;
    private static Vector2 PosDiff = new Vector2(0, 0); 


    internal static void PartyDraw()
    {
        float progress = PartyCount / (float)8;
        ImGui.Text($"Party Size: [{PartyCount}/8]");
        ImGui.SameLine();
        float red = MathF.Min(2 * (1.0f - progress), 1.0f);
        float green = MathF.Min(2 * progress, 1.0f);
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, new Vector4(red, green, 0.0f, 1.0f)))
        {
            ImGui.ProgressBar(progress, new Vector2(250, 15), "");
        }
        //ImGui.SameLine();
        //ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 50);
        //ImGuiComponents.HelpMarker("Data is obtained through Tomestone.gg API. \nAccuracy of progression points of ultimates may not be accurate.");

        /*if (PopupWindow)
        {
            ImGui.SetWindowFocus("User Settings");
            var mainWindowPos = ImGui.GetWindowPos();
            ImGui.SetWindowPos("User Settings", mainWindowPos - PosDiff);
            //ImGui.SetWindowPos("User Settings", )
        }*/


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
            
            //foreach (var member in partyMembers)
            for (int j = 0; j < partyMembers.Length; j++)
            {
                var member = partyMembers[j];
                if (member != null) // Only render initialized members
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0); // Set column 7
                    if (j != -1)
                    {
                        Vector2 size = new Vector2(5, 5);
                        if (Util.DrawButtonIcon(FontAwesomeIcon.UserCog, size))
                        {
                            // Handle the button action here, e.g., add a member or open a menu
                            /*var mousePos = ImGui.GetMousePos();
                            PosDiff = ImGui.GetWindowPos() - mousePos;
                            ImGui.SetWindowPos("User Settings", mousePos);
                            PopupWindow = true;*/
                            ImGui.OpenPopup($"##UserSettings_{j}");
                            PluginLog.Information("Opened");
                            
                        }

                        DrawUserSettingsPopup($"##UserSettings_{j}");
                    }

                    // Member name column (first column)
                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(member.name);

                    // Render ultProg columns (UCOB, UWU, TEA, DSR, TOP, FRU)

                    for (int i = 0; i < member.ultProg.Length; i++)
                    {
                        ImGui.TableSetColumnIndex(i + 2);
                        ImGui.AlignTextToFramePadding();
                        if (member.ultProg[i] != null)
                        {
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
                                if (member.ultProg[i].Split(":").Length == 1)
                                {
                                    if (member.ultProg[i] == "I1")
                                    {
                                        if (i == 2)
                                            member.ultProg[i] = "P1: LC";
                                        else if (i == 3)
                                            member.ultProg[i] = "P4: RW";
                                    }
                                    else if (member.ultProg[i] == "I2")
                                        member.ultProg[i] = "P2: TS";
                                }


                                using var colour = (ImRaii.PushColor(ImGuiCol.Text, DecideColour(member.ultProg[i], i)));
                                ImGui.Text(member.ultProg[i]);
                                colour.Pop();
                            }
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

    private static float value = 5;

    public static void DrawUserSettingsPopup(string popupName)
    {
        if (!ImGui.IsPopupOpen(popupName))
        {
            value = 5;
        }

        if (!ImGui.BeginPopup(popupName, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysUseWindowPadding))
        {
            return;
        }
        //ImGui.SetWindowSize(new Vector2(300, 200));
        //ImGui.SetWindowFocus();


        float red = MathF.Min(2 * (1.0f - value/10f), 1.0f);
        float green = MathF.Min(2 * value / 10f, 1.0f);

        ImGui.PushItemWidth(195);
        using (ImRaii.PushColor(ImGuiCol.SliderGrab, new Vector4(red, green, 0.0f, 1.0f)))
        {
            using (ImRaii.PushColor(ImGuiCol.SliderGrabActive, new Vector4(red, green, 0.0f, 1.0f)))
            {
                ImGui.SliderFloat("", ref value, 0, 10, "%.1f");
            }
        }
        ImGui.PopItemWidth();
        ImGui.Dummy(new Vector2(5,5));

        //ImGuiEx.EnumCombo($"##1", ref type);
        
        ImGui.Dummy(new Vector2(2,5));
        ImGui.SameLine();
        Util.DrawButtonIcon(FontAwesomeIcon.ArrowRight, new Vector2(5, 5));
        Util.SetHoverTooltip("None");
        ImGui.SameLine();
        Util.DrawButtonIcon(FontAwesomeIcon.Running, new Vector2(5, 5));
        Util.SetHoverTooltip("Prog Skipper");
        ImGui.SameLine();
        Util.DrawButtonIcon(FontAwesomeIcon.TrashAlt, new Vector2(5, 5));
        Util.SetHoverTooltip("Terrible");
        ImGui.SameLine();
        Util.DrawButtonIcon(FontAwesomeIcon.Radiation, new Vector2(5, 5)); // PersonHarassing also good
        Util.SetHoverTooltip("Big Mouth");
        ImGui.SameLine();
        Util.DrawButtonIcon(FontAwesomeIcon.Crown, new Vector2(5, 5));
        Util.SetHoverTooltip("Goat");
        //ImGui.InputText("", text, 5); 
        ImGui.EndPopup();
    }

    internal static void HistoryDraw()
    {


    }
}
