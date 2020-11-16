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

namespace SpaceEngineers.UWBlockPrograms.HealthScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
﻿float maxHealth = 0;

public void Main(string argument, UpdateType updateSource) {
   if (argument == "") argument = "0";
    setupCockpit(Convert.ToInt32(argument), 1.4f, Color.DarkRed);
    string printStr = "";
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); 
    
   GridTerminalSystem.GetBlocks(blocks); 

   if (maxHealth == 0 || blocks.Count > maxHealth) {
   maxHealth = blocks.Count;
}

    float fullHealth = 0;
    foreach (IMyTerminalBlock block in blocks) {
       float curHealth = getBlockHealth(block);
        fullHealth += curHealth;
        if (curHealth < 0.99f) {
            block.ShowOnHUD = true;
            printStr += block.CustomName + " damaged: " + (int)(curHealth * 100) + "%\n";
        } else {
           block.ShowOnHUD = false;
       }
    }

    printStr = "Ship Health: " + (int)(fullHealth * 100 / maxHealth) + "%\n" + printStr;
    writeToCockpit(Convert.ToInt32(argument), printStr);
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public float getBlockHealth(IMyTerminalBlock block) {
    IMySlimBlock slimblock = block.CubeGrid.GetCubeBlock(block.Position);  
    float MaxIntegrity = slimblock.MaxIntegrity;  
    float BuildIntegrity = slimblock.BuildIntegrity;  
    float CurrentDamage = slimblock.CurrentDamage;
    return (BuildIntegrity - CurrentDamage) / MaxIntegrity;  
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