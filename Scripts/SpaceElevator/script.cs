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

namespace SpaceEngineers.UWBlockPrograms.SpaceElevator {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
// In m/s
const float FAST_SPEED = 12;
const float SLOW_SPEED = 5;

string speLastArg = "";
Vector3D lastPos;
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    lastPos = Me.Position;
}

public void Main(string argument, UpdateType updateSource) {
    List<IMyMotorSuspension> suspensions = new List<IMyMotorSuspension>();
    GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(suspensions, s => s.CubeGrid.CustomName == Me.CubeGrid.CustomName);
    
    IMyTextPanel statusPanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("SpaceElevatorStatus");
    
    List<IMyCockpit> cockpits = new List<IMyCockpit>();
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits, c => c.CubeGrid.CustomName == Me.CubeGrid.CustomName);
    IMyCockpit cockpit = cockpits[0];

    if (argument == "Slow") {
        foreach (var s in suspensions) {
            suspensions[0].SetValue<float>("Speed Limit", SLOW_SPEED*3.6f);
        }
        return;
    }

    if (argument == "Fast") {
        foreach (var s in suspensions) {
            suspensions[0].SetValue<float>("Speed Limit", FAST_SPEED*3.6f);
        }
        return;
    }
    
    if (argument != speLastArg && argument != "") {
        if (speLastArg != "Stop") {
            speLastArg = "Stop";
        } else {
            speLastArg = argument;
        }
    }

    double speed = (Me.CubeGrid.GetPosition() - lastPos).Length() / Runtime.TimeSinceLastRun.Duration().TotalSeconds;
    double maxSpeed = suspensions[0].GetValue<float>("Speed Limit")/3.6;
    lastPos = Me.CubeGrid.GetPosition();
    float propulsion = suspensions[0].GetValue<float>("Propulsion override");

    switch (speLastArg) {
        case "Up":
            cockpit.HandBrake = false;
            if (speed < maxSpeed*0.75) {
                propulsion += 0.001f;
            } else if (speed > maxSpeed) {
                propulsion -= 0.001f;
            }
            break;

        case "Down":
            if (speed < maxSpeed*0.75) {
                propulsion -= 0.001f;
                cockpit.HandBrake = false;
            } else if (speed > maxSpeed) {
                propulsion += 0.001f;
                cockpit.HandBrake = true;
            }
            break;

        case "Stop":
            cockpit.HandBrake = true;
            propulsion = (float)(0.14 * cockpit.GetNaturalGravity().Length() / 10);
            break;
    }

    foreach (var s in suspensions) {
        s.SetValue<float>("Propulsion override", propulsion);
    }
    LCDWrite(statusPanel, $"Max Speed: {maxSpeed:F1}m/s\nCur Speed: {speed:F1}m/s\nPropulsion: {propulsion*100:F2}%\nBraking: {cockpit.HandBrake}\n");
}

void LCDWrite(IMyTextPanel lcd, string msg, bool clear = true) {
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.FontSize = 1.25f;
    lcd.FontColor = Color.Red;
    lcd.WriteText(msg, !clear);
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
