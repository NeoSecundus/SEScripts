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

const float GATLING_AMMO_VOL = 12.0f;
const float INTERIOR_AMMO_VOL = 0.2f;
const float MISSILE_AMMO_VOL = 60.0f;
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
        printString += constructGunString(gatling, GATLING_AMMO_VOL);
    }

    // Missile Section
    foreach (var missile in missiles) {
        printString += constructGunString(missile, MISSILE_AMMO_VOL);
    }

    // Interior Turret Section
    foreach (var interior in interiors) {
        printString += constructGunString(interior, INTERIOR_AMMO_VOL);
    }
    
    foreach (var panel in panels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Red;
        panel.WriteText(printString);
    }
}

public string constructGunString(IMyFunctionalBlock block, float ammo_vol) {
    IMyInventory inv = block.GetInventory();
        float vol = (float)inv.CurrentVolume;
        int ammo = (int)(vol*12.501 / INTERIOR_AMMO_VOL);
        
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
