using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class PartyList
{
    public static Game.Party party = new Game.Party();

    public static void Draw()
    {
        RenderPartyProgress();
        var localPlayer = Svc.ClientState.LocalPlayer;


        if (ImGui.BeginTable("partyList", 8, ImGuiTableFlags.BordersInner) && localPlayer != null)
        {
            SetupColumns();
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(10, 0));

            ImGui.TableHeadersRow();

            foreach (var member in party.Members)
            {
                if (member != null)
                {
                    ImGui.TableNextRow();

                    RenderJobIcon(member.JobId);

                    RenderPlayerName(member);

                    RenderUltimateList(member);
                }
            }
            style.Pop();
            ImGui.EndTable();
        }
    }

    private static void RenderPartyProgress()
    {
        ImGui.Text($"Party Size: [{party.Size}/8]");
        float sizeProgress = party.Size / (float)8;
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Util.GetColourRange(sizeProgress)))
            ImGui.ProgressBar(sizeProgress, new Vector2(250, 15), "");
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

    private static void RenderJobIcon(uint? jobId)
    {
        var jobTexture = Game.Util.GetJobIcon(jobId);
        if (jobTexture != null)
        {
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Image(jobTexture.ImGuiHandle, new Vector2(27, 27));
        }
    }

    private static void RenderPlayerName(Game.Player player)
    {
        var localPlayer = Svc.ClientState.LocalPlayer;

        if (player.Name != null && player.HomeWorld != null && localPlayer != null)
        {
            ImGui.TableSetColumnIndex(1);
            ImGui.AlignTextToFramePadding();
            bool localPlayerName = (player.Name == localPlayer.Name.TextValue.ToString() && player.HomeWorld == localPlayer.HomeWorld.Value.InternalName.ExtractText());
            Vector4 textColour = new Vector4(1, 1, 1, 1);
            if (!localPlayerName)
            {
                textColour = Util.GetColourState(player.State);
            }

            Popup.Draw(player);
            using var colour = ImRaii.PushColor(ImGuiCol.Text, textColour);
            string popupName = $"##US-{player.Name.Replace(" ", "+")}_{player.HomeWorld}";
            if (ImGui.Selectable($"{player.Name}##{popupName}"))
            {
                ImGui.OpenPopup(popupName);
            }
            colour.Pop();
        }
    }

    private static void RenderUltimateList(Game.Player member)
    {
        for (int i = 0; i < 6; i++)
        {
            ImGui.TableSetColumnIndex(i + 2);
            ImGui.AlignTextToFramePadding();

            var ultimate = member.UltimateProg[i];
            if (ultimate == null)
            {
                return;
            }

            if (ultimate == FontAwesomeIcon.Check.ToIconString())
            {
                RenderUltimate(ultimate, new Vector4(0f, 1f, 0f, 1f));
            }
            else if (ultimate == FontAwesomeIcon.Times.ToIconString())
            {
                RenderUltimate(ultimate, new Vector4(1f, 0f, 0f, 1f));
            }
            else if (ultimate == FontAwesomeIcon.Minus.ToIconString() || ultimate == "")
            {
                RenderUltimate(FontAwesomeIcon.Minus.ToIconString(), new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                if (ultimate.Split(":").Length == 1)
                {
                    ultimate = AdjustUltimate(ultimate, i);
                }
                RenderUltimate(ultimate, Util.DecideColour(ultimate, i), true);
            }
        }
    }

    private static void RenderUltimate(string ultimate, Vector4 colour4, bool text = false)
    {
        using (var colour = (ImRaii.PushColor(ImGuiCol.Text, colour4)))
        {
            if (!text)
            {
                using (var font = ImRaii.PushFont(UiBuilder.IconFont))
                {
                    ImGui.Text(ultimate);
                }
            }
            else
            {
                ImGui.Text(ultimate);
            }
        }
    }

    private static string AdjustUltimate(string ultimate, int index)
    {
        if (ultimate == "I1")
        {
            if (index == 2)
                ultimate = "P1: LC";
            else if (index == 3)
                ultimate = "P4: RW";
        }
        else if (ultimate == "I2")
            ultimate = "P2: TS";

        return ultimate;
    }
}
