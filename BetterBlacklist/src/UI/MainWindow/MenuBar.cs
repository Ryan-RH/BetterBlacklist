using BetterBlacklist.Services;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class MenuBar
{
    public static bool Refreshing = false;

    public static void Draw()
    {
        // Draw Menu Bar
        if (ImGui.BeginMenuBar())
        {
            using var font = ImRaii.PushFont(UiBuilder.IconFont);

            // Open Config Window
            if (ImGui.MenuItem(FontAwesomeIcon.Cog.ToIconString()))
                P.ToggleConfigUI();
            font.Pop();
            Util.SetHoverTooltip("Customise");
            font.Push(UiBuilder.IconFont);

            // Toggle Party/History List
            if (MainWindow.PartyView)
            {
                // Party Visible, Show History List Button
                if (ImGui.MenuItem(FontAwesomeIcon.History.ToIconString()))
                {
                    MainWindow.PartyView = false;
                    HistoryList.Update();
                }
                font.Pop();
                Util.SetHoverTooltip("History");
            }
            else
            {
                // History Visible, Show Party List Button
                if (ImGui.MenuItem(FontAwesomeIcon.Users.ToIconString()))
                    MainWindow.PartyView = true;
                font.Pop();
                Util.SetHoverTooltip("Party");
            }

            // Refresh Button
            if (MainWindow.PartyView)
            {
                font.Push(UiBuilder.IconFont);
                if (!Refreshing)
                {
                    // Refresh Party List
                    if (ImGui.MenuItem(FontAwesomeIcon.Redo.ToIconString()))
                    {
                        Refreshing = true;
                        Task.Run(async () =>
                        {
                            try
                            {
                                await Tasks.Party.Refresh().ConfigureAwait(false);
                            }
                            catch
                            {
                                Svc.Log.Information("Refresh Failure");
                            }
                        });
                    }
                }
                else
                {
                    // Disable - Currently refreshing
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                    ImGui.BeginDisabled(true);
                    ImGui.MenuItem(FontAwesomeIcon.Spinner.ToIconString());

                    ImGui.EndDisabled();
                    ImGui.PopStyleColor();
                }
                font.Pop();
                Util.SetHoverTooltip("Refresh");
            }
            ImGui.EndMenuBar();
        }
    }
}
