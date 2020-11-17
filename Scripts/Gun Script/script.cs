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
using Sandbox.Game.Entities;
using System.Linq.Expressions;

namespace SpaceEngineers.UWBlockPrograms.GunScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource) {
    List<IMyLargeGatlingTurret> gatlings = new List<IMyLargeGatlingTurret>();
    List<IMyLargeMissileTurret> missiles = new List<IMyLargeMissileTurret>();
    List<IMyLargeInteriorTurret> interiors = new List<IMyLargeInteriorTurret>();
    
    GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret>(gatlings);
    GridTerminalSystem.GetBlocksOfType<IMyLargeMissileTurret>(missiles);
    GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret>(interiors);

    string printString = "--- Turret Supervision ---\n\nGun Name | Ammo Count | Health %\n";

    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName("GPanels");
    panelGroup.GetBlocksOfType<IMyTextPanel>(panels);
    if (panels.Count == 0) {
        return;
    }

    // Gatling Section
    foreach (var gatling in gatlings) {
        printString += constructGunString(gatling);
    }

    // Missile Section
    foreach (var missile in missiles) {
        printString += constructGunString(missile);
    }

    // Interior Turret Section
    foreach (var interior in interiors) {
        printString += constructGunString(interior);
    }
    
    foreach (var panel in panels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Red;
        panel.WriteText(printString);
    }
}

public string constructGunString(IMyFunctionalBlock block) {
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    block.GetInventory().GetItems(items);
    int ammo = (int)items[0].Amount;
    
    int health = (int)(getBlockHealth(block) * 100);
    return $"{block.CustomName}: {ammo}u | {health}%\n";
}

public float getBlockHealth(IMyTerminalBlock block) {  
    IMySlimBlock slimblock = block.CubeGrid.GetCubeBlock(block.Position);  
    float MaxIntegrity = slimblock.MaxIntegrity;  
    float BuildIntegrity = slimblock.BuildIntegrity;  
    float CurrentDamage = slimblock.CurrentDamage;
    return (BuildIntegrity - CurrentDamage) / MaxIntegrity;  
}

// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
