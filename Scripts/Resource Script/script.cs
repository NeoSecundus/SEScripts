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
using Sandbox.Game.WorldEnvironment;
using VRage.Network;
using Sandbox.Game;
using System.Security.Policy;
using VRage;

namespace SpaceEngineers.UWBlockPrograms.ResourceScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource) {
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

    string printString = "--- Resources ---\n";

    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName("RPanels");
    panelGroup.GetBlocksOfType<IMyTextPanel>(panels);
    if (panels.Count == 0) {
        return;
    }

    Dictionary<string, float> ores = new Dictionary<string, float>();
    Dictionary<string, float> ingots = new Dictionary<string, float>();
    string[] resIds = new string[]{
        "Uranium", "Platinum", "Gold", 
        "Silver", "Cobalt", "Nickel", 
        "Silicon", "Magnesium", "Iron", 
        "Stone", "Ice"};
    
    foreach (string id in resIds) {
        ores.Add(id, 0);
        ingots.Add(id, 0);
    }

    foreach (var block in blocks) {
        if (!block.HasInventory) continue;

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        IMyInventory inv = block.GetInventory();
        inv.GetItems(items);

        if (block.InventoryCount > 1) {
            inv = block.GetInventory(1);
            List<MyInventoryItem> items_2 = new List<MyInventoryItem>();
            inv.GetItems(items_2);
            ListExtensions.AddList<MyInventoryItem>(items, items_2);
        }

        foreach (var item in items) {
            string itemName = item.Type.SubtypeId;
            if (item.Type.TypeId == "MyObjectBuilder_Ingot") {
                ingots[itemName] += (float)item.Amount;
            } else if (item.Type.TypeId == "MyObjectBuilder_Ore") {
                ores[itemName] += (float)item.Amount;
            }
        }
    }

    foreach (string key in resIds) {
        string ingStr = ingots[key] > 1000 ? $"{ingots[key]/1000:F2}k" : $"{ingots[key]:F2}";
        string oreStr = ores[key] > 1000 ? $"{ores[key]/1000:F2}k" : $"{ores[key]:F2}";

        printString += key + ": " + ingStr + " | Ore: " + oreStr + "\n";
    }

    foreach (var panel in panels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Silver;
        panel.WriteText(printString);
    }
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
