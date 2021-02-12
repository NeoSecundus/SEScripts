#region Prelude
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using VRage.Import;
using VRageRender;
using System.Linq.Expressions;
using System.Threading;
using VRage.Library.Utils;
using Sandbox.Game.AI.Pathfinding.RecastDetour.Shapes;
using Sandbox.Game.AI.Pathfinding;
using SpaceEngineers.Game.Entities.UseObjects.VendingMachine;
using System.Runtime.ExceptionServices;
using Sandbox.Game.World.Generator;
using VRage.GameServices;
using VRage;
using System.Runtime.InteropServices.WindowsRuntime;
using Sandbox.Game.Entities;

namespace SpaceEngineers.UWBlockPrograms.CargoBoy {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public Program() {
    lastKnownEntity = new MyDetectedEntityInfo();
}

string status = "";
MyDetectedEntityInfo lastKnownEntity;
public void Main(string argument, UpdateType updateSource) {
    writeDisplay("");
    var remote = (IMyRemoteControl)GridTerminalSystem.GetBlockWithName("Remote Control");
    var psensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("PlayerSensor");
    var gear = (IMyLandingGear)GridTerminalSystem.GetBlockWithName("Landing Gear");

    if (status != argument && argument != "") {
        status = argument;
    }

    if (status == "follow") {
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
        if (gear.IsLocked) {
            gear.Unlock();
            var thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            foreach (var thruster in thrusters) {
                thruster.ApplyAction("OnOff_On");
            }
        }

        Follow(remote, psensor);

    } else if (status == "dock") {
        Runtime.UpdateFrequency = UpdateFrequency.Update100;
        if (gear.IsLocked) {
            var thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            foreach (var thruster in thrusters) {
                thruster.ApplyAction("OnOff_Off");
            }
            writeDisplay("Docked\n", true);
        } else {
            gear.AutoLock = false;
            Dock(remote, psensor, gear);
        }

    } else {
        Stop(remote);
    }

    CheckEnergy();
}

public void Stop(IMyRemoteControl remote) {
    remote.SetAutoPilotEnabled(false);
    writeDisplay("Stopped\n", true);
    ChangeLights(Color.LightBlue);
}

public Vector3D GetDockingWaypoint(IMySensorBlock psensor) {
    var myPos = Me.CubeGrid.GetPosition();
    var targetPos = psensor.LastDetectedEntity.BoundingBox.Center;
    var forwardVector = psensor.LastDetectedEntity.Orientation.Forward;
    var upVector = psensor.LastDetectedEntity.Orientation.Up;
    var rightVector = psensor.LastDetectedEntity.Orientation.Right;

    var normalV = new PlaneD(targetPos, targetPos + forwardVector, targetPos + upVector).Normal;
    var normalH = new PlaneD(targetPos, targetPos + forwardVector, targetPos + rightVector).Normal;
    var normalF = new PlaneD(targetPos, targetPos + upVector, targetPos + rightVector).Normal;

    Vector3D[] vectors = {myPos + normalV*30, myPos - normalV*30, myPos + normalH*30, myPos - normalH*30, myPos + normalF*30, myPos - normalF*30};
    Vector3D minimal = vectors[0];
    foreach (var vector in vectors) {
        if (Vector3D.Distance(vector, targetPos) < Vector3D.Distance(minimal, targetPos))
            minimal = vector;
    }
    return minimal;
}

public void Dock(IMyRemoteControl remote, IMySensorBlock psensor, IMyLandingGear gear) {
    remote.SetCollisionAvoidance(false);
    remote.SetDockingMode(true);
    remote.Direction = Base6Directions.Direction.Backward;
    remote.SpeedLimit = 2;
    psensor.DetectPlayers = false;
    psensor.DetectLargeShips = true;
    psensor.DetectStations = true;

    FlyTo(remote, GetDockingWaypoint(psensor));
    writeDisplay("Docking...\n", true);
    ChangeLights(Color.DarkGreen);
    if (gear.LockMode == LandingGearMode.ReadyToLock) {
        gear.Lock();
    }
}

public void Follow(IMyRemoteControl remote, IMySensorBlock psensor) {
    psensor.DetectPlayers = true;
    psensor.DetectLargeShips = false;
    psensor.DetectStations = false;

    var gsensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GridSensor");
    if (!gsensor.LastDetectedEntity.IsEmpty() && lastKnownEntity.Velocity.Length() < 7 && 
            Vector3D.Distance(lastKnownEntity.BoundingBox.Center, Me.CubeGrid.GetPosition()) < 25) {
        remote.SetCollisionAvoidance(false);
        remote.SetDockingMode(true);
        ChangeLights(Color.Orange);
    } else {
        remote.SetCollisionAvoidance(true);
        remote.SetDockingMode(false);
        ChangeLights(Color.White);
    }
    remote.Direction = Base6Directions.Direction.Forward;

    var player = psensor.LastDetectedEntity;
    if (player.IsEmpty()) {
        writeDisplay("Owner Out of Range!\n", true);
        ChangeLights(Color.Red);
        remote.SetCollisionAvoidance(true);
        remote.SetDockingMode(false);
        if (Vector3D.Distance(Me.CubeGrid.GetPosition(), lastKnownEntity.BoundingBox.Center) > 5) {
            FlyTo(remote, lastKnownEntity.BoundingBox.Center);
        } else {
            remote.SetAutoPilotEnabled(false);
        }
        return;
    }

    var pos = player.BoundingBox.Center;
    var posI = (Vector3I)pos;
    lastKnownEntity = player;

    var distance = Vector3D.Distance(Me.CubeGrid.GetPosition(), pos);
    var shipVelocity = remote.GetShipVelocities().LinearVelocity;
    if (distance < 10) {
        writeDisplay("Keeping Distance...\n", true);
        Stop(remote);
        return;
    } else if (player.Velocity.Length() > 16) {
        remote.SpeedLimit = player.Velocity.Length()+3;
        pos += player.Velocity * player.Velocity.Length()/10.4f;
        var crossVec = Vector3D.Normalize(player.Velocity);
        var tempX = crossVec.X;
        crossVec.X = crossVec.Y;
        crossVec.Y = -tempX;
        pos += (crossVec*player.Velocity.Length()/7.2)+10;
        ChangeLights(Color.Turquoise);
    } else {
        remote.SpeedLimit = (float)distance - 9;
    }

    writeDisplay($"Currently Following!\n{player.Name}\nX: {posI.X}\nY: {posI.Y}\nZ: {posI.Z}\n", true);
    FlyTo(remote, pos);
}

public void FlyTo(IMyRemoteControl remote, Vector3D pos) {
    remote.ControlThrusters = true;
    remote.FlightMode = FlightMode.OneWay;
    remote.IsMainCockpit = true;
    remote.ClearWaypoints();
    remote.AddWaypoint(pos, "Target");
    remote.SetAutoPilotEnabled(true);
}

public void CheckEnergy() {
    var reactors = new List<IMyReactor>();
    GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors);

    float output = 0;
    float uranium = 0;
    foreach (var reactor in reactors) {
        output += reactor.CurrentOutput;
        var inv = reactor.GetInventory();
        uranium += (float)inv.GetItemAmount(MyItemType.MakeIngot("Uranium"));
    }

    writeDisplay($"\nReactor output: {(int)output*1000}kW\nUranium: {uranium}u\n", true);
    if (uranium < 2) {
        writeDisplay("--> FUEL LOW <--\n", true);
        ChangeLights(Color.Red, true);
    }
}

public void ChangeLights(Color color, bool blinking = false) {
    var lights = new List<IMyInteriorLight>();
    GridTerminalSystem.GetBlockGroupWithName("Lights").GetBlocksOfType<IMyInteriorLight>(lights);
    if (lights[0].Color == color) {
        return;
    }

    foreach (var light in lights) {
        light.Color = color;
        if (blinking) {
            light.BlinkIntervalSeconds = 2;
            light.BlinkLength = 50f;
        } else {
            light.BlinkIntervalSeconds = 0;
        }
    }
}

public void writeDisplay(string msg, bool append = false) {
    writeDisplay(msg, Color.Green, append);
}

public void writeDisplay(string msg, Color color, bool append = false) {
    IMyTextSurface display = Me.GetSurface(0);
    display.ContentType = ContentType.TEXT_AND_IMAGE;
    display.FontColor = color;
    display.Alignment = TextAlignment.CENTER;
    display.FontSize = 1.6f;
    display.WriteText(msg, append);
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
