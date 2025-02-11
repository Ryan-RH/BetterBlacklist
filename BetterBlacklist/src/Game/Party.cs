using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Game;

public class Party
{
    public int Size = 0;
    public List<Player> Members = new List<Player>();

    public unsafe static Party Collect()
    {
        var SRGroup = Svc.Party;
        var SRGroupCount = SRGroup.Count;

        var CRGroup = InfoProxyCrossRealm.Instance()->CrossRealmGroups[0];
        var CRGroupCount = CRGroup.GroupMemberCount;

        Party party = new Party();

        if (SRGroupCount > 0) // Same Realm party detected
        {
            foreach (var player in SRGroup)
            {
                var name = player.Name.TextValue;
                var world = player.World.Value.InternalName.ExtractText();
                var job = player.ClassJob.Value.RowId;
                Player member = new Player(name, world, job); 
                party.Members.Add(member);
            }
            party.Size = SRGroupCount;
        }
        else if (CRGroupCount > 0) // Cross Realm party detected
        {
            for (int i = 0; i < CRGroupCount; i++)
            {
                var player = CRGroup.GroupMembers[i];
                var name = player.NameString;
                var world = Svc.Data.GetExcelSheet<World>().First(world => world.RowId == player.HomeWorld).InternalName.ExtractText();
                var job = player.ClassJobId;
                Player member = new Player(name, world, job);
                party.Members.Add(member);
            }
            party.Size = CRGroupCount;
        }
        else // Solo party detected
        {
            var localPlayer = Svc.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                var name = localPlayer.Name.TextValue;
                var world = localPlayer.HomeWorld.Value.InternalName.ExtractText();
                var job = localPlayer.ClassJob.RowId;
                Player member = new Player(name, world, job);
                party.Members.Add(member);
                party.Size = 1;
            }
        }
        return party;
    }   
}
