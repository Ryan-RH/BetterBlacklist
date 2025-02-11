using BetterBlacklist.Database;
using BetterBlacklist.Game;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class HistoryList
{
    public class DutyDisplay
    {
        public Duty duty = new Duty();
        public bool visible = false;
    }

    public static List<DutyDisplay> DutyHistory = new List<DutyDisplay>();

    public static void Update()
    {
        Task.Run(async () =>
        {
            try
            {
                var duties = await Database.Query.ExtractDutyHistory().ConfigureAwait(false);
                DutyHistory = duties.Select(duty => new DutyDisplay { duty = duty, visible = false }).ToList();
            }
            catch (Exception ex)
            {
                Svc.Log.Information($"Failed to Extract History: {ex.Message}");
            }
        });
    }

    public static void Draw()
    {
        ImGui.Text("Duty History");
        if (ImGui.BeginTable("dutyHistory##history", 8, ImGuiTableFlags.BordersH) && Svc.ClientState.LocalPlayer != null)
        {
            SetupColumns();
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0f, 0.5f));
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(10, 0));
            ImGui.TableHeadersRow();

            for (int i = 0; i < DutyHistory.Count; i++)
            {
                var content = DutyHistory[i];
                ImGui.TableNextRow();

                RenderImage(content.duty.Name);

                RenderContent(content, i);

                RenderTime(content.duty.UnixTimestamp);
            }

            style.Pop();
            ImGui.PopStyleVar();
            ImGui.EndTable();
        }
    }

    private static void SetupColumns()
    {
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 125);
        ImGui.TableSetupColumn("Duty", ImGuiTableColumnFlags.WidthFixed, 325);
        ImGui.TableSetupColumn("Time Ago", ImGuiTableColumnFlags.WidthFixed, 75);
    }

    private static void RenderImage(string? name)
    {
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();

        var duty = Svc.Data.GetExcelSheet<ContentFinderCondition>().First(duty => duty.Name == name);
        var dutyImage = Game.Util.GetDutyImage(duty.Image);
        if (dutyImage != null)
        {
            float scaleX = 125f / dutyImage.Size.X;
            float scaleY = 50f / dutyImage.Size.Y;
            float scale = Math.Min(scaleX, scaleY); 

            float scaledWidth = dutyImage.Size.X * scale;
            float scaledHeight = dutyImage.Size.Y * scale;

            ImGui.Image(dutyImage.ImGuiHandle, new Vector2(scaledWidth, scaledHeight));
        }
    }

    private static void RenderContent(DutyDisplay? content, int index)
    {
        if (content != null)
        {
            ImGui.TableSetColumnIndex(1);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(content.duty.Name);
            ImGuiDir buttonDir = content.visible ? ImGuiDir.Up : ImGuiDir.Down;

            if (ImGui.ArrowButton($"##dutydropdow_{index}", buttonDir))
            {
                //content.visible = !content.visible;
                Task.Run(async () =>
                {
                    try
                    {
                        await Command.FindDutyState(index);
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Information($"Render fail: {ex.Message}");
                    }
                });
            }

            if (content.visible)
            {
                var roleSorted = SortByJobs(content.duty.Players);
                RenderPlayers(roleSorted);
            }
        }
    }

    private static void RenderPlayers(Player[] players)
    {
        ImGui.Dummy(new Vector2(0, 3));
        bool nextLine = false;


        foreach (var player in players)
        {
            var jobIcon = Game.Util.GetJobIcon(player.JobId);
            float totalWidth = ImGui.CalcTextSize(player.Name).X + 30;

            var cursorPos = ImGui.GetCursorPos();
            var copyPos = new Vector2(cursorPos.X + 3, cursorPos.Y);
            ImGui.SetCursorPos(copyPos);
            var popupName = $"##US-{player.Name!.Replace(" ", "+")}_{player.HomeWorld}";

            Popup.Draw(player);
            if (ImGui.Selectable(popupName, false, ImGuiSelectableFlags.None, new Vector2(totalWidth, 20)))
            {
                ImGui.OpenPopup(popupName);
            }
            ImGui.SetCursorPos(cursorPos);

            var iconSize = new Vector2(20, 20);
            if (jobIcon != null)
            {
                ImGui.Image(jobIcon.ImGuiHandle, iconSize);
            }
            else
            {
                ImGui.Image(Game.Util.GetJobIcon(45)!.ImGuiHandle, iconSize);
            }
            ImGui.SameLine();

            using var colour = (ImRaii.PushColor(ImGuiCol.Text, Util.GetColourState(player.State)));
            ImGui.Text(player.Name);
            colour.Pop();

            if (!nextLine)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosX(300);
            }
            else
            {
                ImGui.Dummy(new Vector2(0, 3));
            }
            nextLine = !nextLine;
        }
    }

    private static Player[] SortByJobs(Player[] players)
    {
        var roleSorted = players.OrderBy(player =>
        {
            var job = Svc.Data.GetExcelSheet<ClassJob>().FirstOrDefault(job => job.RowId == player.JobId);
            return job.UIPriority;
        });
        return roleSorted.ToArray();
    }

    private static void RenderTime(uint timestamp)
    {
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
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

        ImGui.TableSetColumnIndex(2);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(time);
    }
}
