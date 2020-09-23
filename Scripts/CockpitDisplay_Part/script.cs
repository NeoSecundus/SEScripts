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
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers.UWBlockPrograms.CockpitDisplay_Part {
    public sealed class MyProgram : MyGridProgram {
#endregion
// YOUR CODE BEGIN
private void writeToCockpit(int displayNum, string text, int cockPitNum=0) {
    List<IMyCockpit> blocks = new List<IMyCockpit>();
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(blocks);
    if (blocks.Count < cockPitNum) {
        Echo("No Cockpit with ID: " + cockPitNum);
        return;
    }

    IMyTextSurfaceProvider c = blocks[cockPitNum] as IMyCockpit;
    if (c.SurfaceCount < displayNum) {
        Echo("No Display with ID: " + displayNum);
        return;
    }

    c.GetSurface(displayNum).WriteText(text);
}

private void setupCockpit(int displayNum, float fontSize, Color fontColor, int cockPitNum = 0) {
   List<IMyCockpit> blocks = new List<IMyCockpit>();
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(blocks);
    if (blocks.Count < cockPitNum) {
        Echo("No Cockpit with ID: " + cockPitNum);
        return;
    }

    IMyTextSurfaceProvider c = blocks[cockPitNum] as IMyCockpit;
    if (c.SurfaceCount < displayNum) {
        Echo("No Display with ID: " + displayNum);
        return;
    }
    
   IMyTextSurface display = c.GetSurface(displayNum);
    display.ContentType = ContentType.TEXT_AND_IMAGE;
    display.FontColor = fontColor;
    display.FontSize = fontSize;
}


// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
