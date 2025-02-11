using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Game;

public enum State
{
    Friend,
    Good,
    Familiar,
    New,
    Poor,
    Bad,
    Avoid
}

public class Player
{
    public string? Name;
    public string? HomeWorld;
    public uint? JobId;
    public string[] UltimateProg = new string[6];
    public State State = State.New;


    public Player(string? name = null, string? homeWorld = null, uint? jobId = null, State state = State.New)
    {
        Name = name;
        HomeWorld = homeWorld;
        JobId = jobId;
        State = state;
    }
}
