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
using Sandbox.Game.GameSystems;
using System.Threading;
using System.Runtime.InteropServices;
using Sandbox.Definitions;

namespace SpaceEngineers.UWBlockPrograms.ElevatorScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public Program() {
}

const int LEVELS = 4;
const float LEVEL_HEIGHT = 7.4f;
const float SPEED = 1.5f;
public void Main(string argument, UpdateType updateSource) {
    var group = GridTerminalSystem.GetBlockGroupWithName("CrewElevator");
    List<IMyDoor> doors = new List<IMyDoor>(); 
    List<IMyPistonBase> pistons = new List<IMyPistonBase>();

    group.GetBlocksOfType<IMyPistonBase>(pistons);
    group.GetBlocksOfType<IMyDoor>(doors);
    
    foreach (IMyDoor door in doors) {
        door.ApplyAction("Open_Off");
    }

    try {
        int level = Convert.ToInt32(argument) - 1;
        if (level > LEVELS) {
            return;
        }

        movePistons(pistons, level);
    } catch (Exception e) {
        writeDisplay(e.StackTrace, Color.Red);
    }
}

public void movePistons(List<IMyPistonBase> pistons, int level) {
    foreach (IMyPistonBase piston in pistons) {
        piston.MaxLimit = LEVEL_HEIGHT * level / pistons.Count;
        piston.MinLimit = LEVEL_HEIGHT * level / pistons.Count;
        
        if (piston.CurrentPosition * pistons.Count > LEVEL_HEIGHT * level) {
            piston.Velocity = -SPEED / pistons.Count;
        } else {
            piston.Velocity = SPEED / pistons.Count;
        }
    }
}

public void writeDisplay(string msg, Color color) {
    IMyTextSurface display = Me.GetSurface(0);
    display.ContentType = ContentType.TEXT_AND_IMAGE;
    display.FontColor = Color.Red;
    display.FontSize = 0.7f;
    display.WriteText(msg);
}

// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
