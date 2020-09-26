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
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

string persistentArg;
﻿public void Main(string argument, UpdateType updateSource) {
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

    if (argument != "") {
        persistentArg = argument;
    }

    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName("CPanels");
    panelGroup.GetBlocksOfType<IMyTextPanel>(panels);

    float tempVal;
    string printString = "";
    float maxInv = 0;
    float curInv = 0;
    foreach (IMyTerminalBlock block in blocks) {
        if (!block.HasInventory || block is IMyReactor || !block.ShowInInventory || block.CubeGrid.CustomName != persistentArg) continue;

        IMyInventory inv = block.GetInventory();
        maxInv += (float)inv.MaxVolume;
        curInv += (float)inv.CurrentVolume;
        tempVal = (float)inv.CurrentVolume * 100 / (float)inv.MaxVolume;
        printString += $"{block.CustomName}: {tempVal:F1}%\n";
    }

    int fillRatio = (int)(curInv * 100 / maxInv);
    printString = $"Total Ratio: {fillRatio}%\n----------\n{printString}";
    foreach (IMyTextPanel panel in panels) {
        if (panel.CubeGrid.CustomName != persistentArg) continue;

        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Orange;
        panel.WriteText(printString);
    }
}

// YOUR CODE END
#region PreludeFooter
    }
}
#endregion