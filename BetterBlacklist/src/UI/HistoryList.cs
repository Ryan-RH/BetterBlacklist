using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class HistoryList
{
    public static void Draw()
    {
        List<Database.DBPlayerData> PlayerHistory = Database.ExtractHistory();

        ImGui.Text("Player History");
        if (ImGui.BeginTable("playerHistory", 8, ImGuiTableFlags.BordersH) && Svc.ClientState.LocalPlayer != null)
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 20);
            ImGui.TableSetupColumn("Player", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("Duty", ImGuiTableColumnFlags.WidthFixed, 280);
            ImGui.TableSetupColumn("Last Seen", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0f, 0.5f));
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(10, 0));
            ImGui.TableHeadersRow();

            foreach (var player in PlayerHistory)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                var classJob = Svc.Data.GetExcelSheet<ClassJob>().First(job => job.Abbreviation == player.JobPlayed);
                var jobIcon = Svc.Texture.GetFromGame("ui/icon/062000/0621" + classJob.RowId + ".tex").GetWrapOrEmpty();
                ImGui.Image(jobIcon.ImGuiHandle, new Vector2(20, 20));
                Util.SetHoverTooltip($"{player.Name!.Split(" ")[0]} played as {classJob.Name.ExtractText()}.");

                ImGui.TableSetColumnIndex(1);
                ImGui.AlignTextToFramePadding();
                float rating = (player.Rating ?? 5.0f) / 10;
                string selectableTag = $"{player.Name.Replace(" ", "-")}_{player.HomeWorld}";
                Popups.DrawLinks(selectableTag, player.Name, player.HomeWorld!);
                using var colour = (ImRaii.PushColor(ImGuiCol.Text, Util.GetColourRange(rating)));
                if (ImGui.Selectable(player.Name))
                {
                    ImGui.OpenPopup($"User##UserOptions_{selectableTag}");
                }
                colour.Pop();
                Util.SetHoverTooltip($"{player.Name} - {player.HomeWorld}");
                ImGui.TableSetColumnIndex(2);
                ImGui.AlignTextToFramePadding();
                ImGui.Text(player.DutySeen);
                ImGui.TableSetColumnIndex(3);
                ImGui.AlignTextToFramePadding();

                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds((long)player.UnixTimestamp!).DateTime;
                var timeDifference = DateTime.Now - dateTime;
                string time = "";
                if (timeDifference.TotalDays >= 1)
                {
                    int days = (int)timeDifference.TotalDays;
                    time = $"{days}d ago";
                }
                else if (timeDifference.TotalHours >= 1)
                {
                    int hours = (int)timeDifference.TotalHours;
                    time = $"{hours}h ago";
                }
                else if (timeDifference.TotalMinutes >= 1)
                {
                    int minutes = (int)timeDifference.TotalMinutes;
                    time = $"{minutes}m ago";
                }
                else
                {
                    time = "Just now";
                }
                ImGui.Text(time);
                Util.SetHoverTooltip(dateTime.Date.ToShortDateString().ToString());
            }
            style.Pop();
            ImGui.PopStyleVar();
            ImGui.EndTable();
        }
    }
}
