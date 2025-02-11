using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Game;

public static class Util
{
    public static IDalamudTextureWrap? GetJobIcon(uint? jobId)
    {
        if (jobId != null)
        {
            var jobTexture = Svc.Texture.GetFromGame("ui/icon/062000/0621" + jobId + ".tex").GetWrapOrEmpty();
            return jobTexture;
        }
        return null;
    }

    public static IDalamudTextureWrap? GetDutyImage(uint? dutyId)
    {
        if (dutyId != null)
        {
            var dutyTexture = Svc.Texture.GetFromGame("ui/icon/112000/" + dutyId + ".tex").GetWrapOrEmpty();
            return dutyTexture;
        }
        return null;
    }

    public unsafe static bool IsFriend(Game.Player player)
    {
        return InfoProxyFriendList.Instance()->CharDataSpan.ToArray().Any(friend =>
                friend.NameString == player.Name &&
                Svc.Data.GetExcelSheet<World>().First(world => world.RowId == friend.HomeWorld).InternalName.ExtractText() == player.HomeWorld);
    }
}
