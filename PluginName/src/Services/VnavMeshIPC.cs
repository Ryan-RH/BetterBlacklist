using ECommons.EzIpcManager;
using System.Threading;

namespace PluginName.Services;

public class VnavMeshIPC
{
    // Example IPC functions
    [EzIPC("Nav.IsReady", true)] public Func<bool> Nav_IsReady;
    [EzIPC("Nav.BuildProgress", true)] public Func<float> Nav_BuildProgress;
    [EzIPC("Nav.Reload", true)] public Action Nav_Reload;
    [EzIPC("Nav.Rebuild", true)] public Action Nav_Rebuild;
    [EzIPC("Nav.Pathfind", true)] public Func<Vector3, Vector3, bool, Task<List<Vector3>>> Nav_Pathfind;
    [EzIPC("Nav.PathfindCancelable", true)] public Func<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>> Nav_PathfindCancelable;
    [EzIPC("Nav.PathfindCancelAll", true)] public Action Nav_PathfindCancelAll;
    [EzIPC("Nav.PathfindInProgress", true)] public Func<bool> Nav_PathfindInProgress;
    [EzIPC("Nav.PathfindNumQueued", true)] public Func<int> Nav_PathfindNumQueued;
    [EzIPC("Nav.IsAutoLoad", true)] public Func<bool> Nav_IsAutoLoad;
    [EzIPC("Nav.SetAutoLoad", true)] public Action<bool> Nav_SetAutoLoad;

    [EzIPC("Query.Mesh.NearestPoint", true)] public Func<Vector3, float, float, Vector3> Query_Mesh_NearestPoint;
    [EzIPC("Query.Mesh.PointOnFloor", true)] public Func<Vector3, bool, float, Vector3> Query_Mesh_PointOnFloor;

    [EzIPC("Path.MoveTo", true)] public Action<List<Vector3>, bool> Path_MoveTo;
    [EzIPC("Path.Stop", true)] public Action Path_Stop;
    [EzIPC("Path.IsRunning", true)] public Func<bool> Path_IsRunning;
    [EzIPC("Path.NumWaypoints", true)] public Func<int> Path_NumWaypoints;
    [EzIPC("Path.GetMovementAllowed", true)] public Func<bool> Path_GetMovementAllowed;
    [EzIPC("Path.SetMovementAllowed", true)] public Action<bool> Path_SetMovementAllowed;
    [EzIPC("Path.GetAlignCamera", true)] public Func<bool> Path_GetAlignCamera;
    [EzIPC("Path.SetAlignCamera", true)] public Action<bool> Path_SetAlignCamera;
    [EzIPC("Path.GetTolerance", true)] public Func<float> Path_GetTolerance;
    [EzIPC("Path.SetTolerance", true)] public Action<float> Path_SetTolerance;

    [EzIPC("SimpleMove.PathfindAndMoveTo", true)] public Func<Vector3, bool, bool> SimpleMove_PathfindAndMoveTo;
    [EzIPC("SimpleMove.PathfindInProgress", true)] public Func<bool> SimpleMove_PathfindInProgress;

    [EzIPC("Window.IsOpen", true)] public Func<bool> Window_IsOpen;
    [EzIPC("Window.SetOpen", true)] public Action<bool> Window_SetOpen;

    [EzIPC("DTR.IsShown", true)] public Func<bool> DTR_IsShown;
    [EzIPC("DTR.SetShown", true)] public Action<bool> DTR_SetShown;

    // Exeample IPC Constructor
    private VnavMeshIPC()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }
}
