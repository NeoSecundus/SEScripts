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
using VRage.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers.UWBlockPrograms.CargoScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
﻿public void Main(string argument, UpdateType updateSource) {
    if (argument == "") {
       argument = "0";
   }
    setupCockpit(Convert.ToInt32(argument),  1.5f, Color.Orange);

    List<IMyFunctionalBlock> blocks = new List<IMyFunctionalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(blocks);

    float tempVal;
    string printString = "";
    float maxInv = 0;
    float curInv = 0;
    foreach (IMyFunctionalBlock block in blocks) {
        if (!block.HasInventory || block is IMyReactor || !block.ShowInInventory) continue;

        IMyInventory inv = block.GetInventory();
        maxInv += (float)inv.MaxVolume;
        curInv += (float)inv.CurrentVolume;
        tempVal = (float)inv.CurrentVolume * 100 / (float)inv.MaxVolume;
        printString += block.CustomName + ": " + Convert.ToInt32(tempVal) + "%\n";
    }

   int fillRatio = (int)(curInv * 100 / maxInv);
   printString = "Fill Ratio: " + fillRatio + "%\n" + "----------\n" + printString;
   writeToCockpit(Convert.ToInt32(argument), printString);
   Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

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