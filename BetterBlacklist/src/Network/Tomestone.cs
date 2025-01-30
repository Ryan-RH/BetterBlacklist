using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetStone;

namespace BetterBlacklist.Network;

internal static class Tomestone
{
    public static async Task FetchProg()
    {
        await Initial();
    }


    private static async Task Initial()
    {
        // tomestone api does not provide an easy way of grabbing prog points, therefore a non efficient method must be used
        // the goal is to eliminate unnecessary network requests by computing certain logic
        // trial and error must occur, tomestone works by retrieving the most recent ultimate by patch prog point
        // taking ucob as an initial starting point therefore makes sense
        // base url: https:///tomestone.gg/character/{loadstone_id}/{name}

        // get loadstone id using NetStone, name and world
        // use NetStone to find achievements relating to completed ults
        // find most suitable of finding prog point -> stormblood/endwalker are best as they contain two ults each, ucob/uwu will take priority over dsr/top
        // check specified prog and then visit page with other ultimates for their prog

        // UCOB+UWU:    /progress?encounterCategory=ultimate&encounterExpansion=stormblood
        // TEA:         /progress?encounterCategory=ultimate&encounterExpansion=shadowbringers
        // DSR+TOP:     /progress?encounterCategory=ultimate&encounterExpansion=endwalker
        // FRU:         /progress?encounterCategory=ultimate&encounterExpansion=dawntrail

        // maximum number of requests per player is 4, total maximum per party is 32 requests (that is quite a lot)
    }
}
