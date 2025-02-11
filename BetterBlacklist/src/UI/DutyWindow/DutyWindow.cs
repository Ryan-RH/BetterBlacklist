using BetterBlacklist.Database;
using BetterBlacklist.Game;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public class DutyWindow : Window, IDisposable
{
    public DutyWindow(BetterBlacklist plugin) : base("##dutyWin")
    {
        Size = new(260, 285);
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var localPlayer = Svc.ClientState.LocalPlayer;
        using var colour = (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.01f, 0.96f, 0.48f, 1.0f)));
        ImGui.Text("Duty Summary");
        colour.Pop();
        //Util.DrawButtonIcon(FontAwesomeIcon.Forward, new Vector2(5, 5));
        ImGui.SameLine();
        ImGui.Dummy(new Vector2(75, 0));
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if (ImGui.Button("Finish##"))
        {
            P.ToggleDutyUI();
            Task.Run(Tasks.Party.DWAddPlayers);
        }
        ImGui.PopStyleColor(3);

        if (Collection.currentDuty.Name != null && Collection.currentDuty.UnixTimestamp != 0 && localPlayer != null)
        {
            var timeDifference = (Collection.timeOfExit - DateTimeOffset.FromUnixTimeSeconds(Collection.currentDuty.UnixTimestamp).DateTime);
            ImGui.Text($"\tDuty:\n\t {Collection.currentDuty.Name}");
            ImGui.Text($"\tDuration: {(int)timeDifference.TotalMinutes} minutes");
            ImGui.Dummy(new Vector2(0, 5));

            if (ImGui.BeginTable("##dutysummary", 2))
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 85);
                foreach (var player in Collection.currentDuty.Players)
                {
                    if (Game.Util.IsFriend(player))
                        continue;
                    if (player.Name == localPlayer.Name.TextValue.ToString() && player.HomeWorld == localPlayer.HomeWorld.Value.InternalName.ExtractText())
                        continue;
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new Vector2(15, 0));
                    ImGui.SameLine();
                    Util.MakeIconAndPlayerSelectable(player);
                    ImGui.SameLine();
                    ImGui.TableSetColumnIndex(1);
                    StateModify(player);
                }
                ImGui.EndTable();
            }
            
        }
    }

    public static void StateModify(Game.Player player)
    {
        string id = $"{player.Name!.Replace(" ", "+")}_{player.HomeWorld}";
        ImGui.Dummy(new Vector2(3, 0));
        ImGui.SameLine();
        if (player.State != State.Avoid)
        {
            if (Util.DrawButtonIcon(FontAwesomeIcon.Minus, new Vector2(3, 3), id))
            {
                if (player.State == State.Familiar)
                    player.State++;
                player.State++;
            }
        }
        else
        {
            ImGui.Dummy(new Vector2(22, 0));
        }
        ImGui.SameLine();
        if (player.State != State.Good)
        {
            if (Util.DrawButtonIcon(FontAwesomeIcon.Plus, new Vector2(3, 3), id))
            {
                if (player.State == State.Poor)
                    player.State--;
                player.State--;
            }
        }
        else
        {
            ImGui.Dummy(new Vector2(3, 3));
        }
 
    }


    // Should Clear duty after submission!
}
