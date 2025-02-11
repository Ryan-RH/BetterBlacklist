using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Game;

public class Duty
{
    public string? Name;
    public Player[] Players = new Player[8];
    public uint UnixTimestamp = 0;


    public void Clear()
    {
        Name = null;
        for (int i = 0; i < 8; i++)
        {
            Players[i] = new Game.Player();
        }
        UnixTimestamp = 0;
    }
}
