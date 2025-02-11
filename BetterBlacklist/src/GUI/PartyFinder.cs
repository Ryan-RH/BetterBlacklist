using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.GUI;

public static class PartyFinder
{
    private static List<IPartyFinderListing> Listings = new List<IPartyFinderListing>();

    public static void Update(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
    {
        Svc.Log.Information($"{listing.Name.TextValue.ToString()}, {listing.HomeWorld.Value.InternalName.ExtractText()}, {listing.SecondsRemaining.ToString()}");

        // Listings in order of duty, then time since posted, then host alphabet(?)
        Listings.Add(listing);

        Svc.Log.Information(args.BatchNumber.ToString());
    }



    // -- UI party finder --
    // top results get replaced by bottom results
    // resolution?
    // relate 

    /// Listener for addon
    // Find number of pfs 
    // If Listings size is equal to number then all listings collected
    // See which List Component node is active (13 is small view, 14 is big view)
    /// Small view
    // [2] - [20] editable, [21] [22] non editable: reflect actual list
    // 1s = [3] - [21] editable, [2] [22] non editable: 2 now becomes next one in list
    // 2s = [2] [4] - [22] editable, [3] non editable: 3 now becomes next one in list
    // 3s = [

}
