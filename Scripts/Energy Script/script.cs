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
using SpaceEngineers.Game.Entities.Blocks;
using VRageRender.Messages;

namespace SpaceEngineers.UWBlockPrograms.EnergyScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public void Main(string argument, UpdateType updateSource) {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    
    // define lists for object collection
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    List<IMySolarPanel> solars = new List<IMySolarPanel>();
    List<IMyReactor> reactors = new List<IMyReactor>();
    List<IMyPowerProducer> turbines = new List<IMyPowerProducer>();
    List<IMyPowerProducer> engines = new List<IMyPowerProducer>();

    // collect all power producers and batteries
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
    GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(solars);
    GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors);
    GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(turbines, t => t.BlockDefinition.SubtypeId.Contains("WindTurbine"));
    GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(engines, e => e.BlockDefinition.SubtypeId.Contains("HydrogenEngine"));

    // define collector variables
    float turbineWatts = 0;
    float solarWatts = 0;
    float reactorWatts = 0;
    float uranium = 0;
    float engineWatts = 0;
    
    // define battery variables
    float bMaxEnergy = 0;
    float bCurEnergy = 0;
    float bFillRatio = 0;
    float bPowerFlow = 0;
    float bTimeLeft = 0;
    

    // battery section
    if (batteries.Count > 0) {
        foreach (IMyBatteryBlock battery in batteries) {
            bMaxEnergy += battery.MaxStoredPower;
            bCurEnergy += battery.CurrentStoredPower;
            bPowerFlow += battery.CurrentInput;
            bPowerFlow -= battery.CurrentOutput;
        }
        if (batteries[0].IsCharging) {
            bTimeLeft = (bMaxEnergy - bCurEnergy) / bPowerFlow;
        } else {
            bTimeLeft = bCurEnergy / -bPowerFlow;
        }
        bTimeLeft *= 3600;
        bFillRatio = (bCurEnergy * 100) / bMaxEnergy;
    }
    

    // reactor section
    foreach (IMyReactor reactor in reactors) {
        reactorWatts += reactor.CurrentOutput*1000;
        uranium += (float)reactor.GetInventory().CurrentMass;
    }
    
    // hydrogen engine section
    foreach (IMyPowerProducer engine in engines) {
        engineWatts += engine.CurrentOutput*1000;
    }

    // solar section
    foreach (IMySolarPanel panel in solars) {
        solarWatts += panel.CurrentOutput*1000;
    }

    // windturbine section
    foreach (IMyPowerProducer turbine in turbines) {
        turbineWatts += turbine.CurrentOutput*1000;
    }

    // text panel section
    List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
    IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName("EPanels");
    panelGroup.GetBlocksOfType<IMyTextPanel>(textPanels);
    int entries = 0;

    //generate result string
    string lcdText = "";
    lcdText += "Energy Supervision:\n";
    if (solars.Count > 0) {
        lcdText += $"Solars: {solars.Count}\nOutput: {solarWatts:F1}kW\n\n";
        entries++;
    }
    if (turbines.Count > 0) {
        lcdText += $"Turbines: {turbines.Count}\nOutput: {turbineWatts:F1}kW\n\n";
        entries++;
    }
    if (reactors.Count > 0) {
        lcdText += $"Reactors: {reactors.Count}\nOutput: {reactorWatts:F1}kW\nUranium: {uranium:F2}U\n\n";
        entries++;
    }
    if (engines.Count > 0) {
        lcdText += $"H2Engines: {engines.Count}\nOutput: {engineWatts:F1}kW\n\n";
        entries++;
    }
    if (batteries.Count > 0) {
        lcdText += $"Batteries: {batteries.Count}\nFillLevel: {bFillRatio:F1}% and currently {(batteries[0].IsCharging ? "Charging" : "Discharging")}\nTime Left: ";
        lcdText += $"{Math.Floor(bTimeLeft/3600):00}:{Math.Floor((bTimeLeft%3600)/60):00}:{Math.Floor(bTimeLeft%60):00}\n\n";
        lcdText += $"Total Flow: {bPowerFlow:F2}MW\n";
        entries++;
    }
    lcdText += $"Total Production: {(solarWatts + turbineWatts + engineWatts + reactorWatts)/1000:F3}MW";

    // setup and write to LCDs
    foreach (IMyTextPanel panel in textPanels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        if (entries < 3) {
            panel.FontSize = 1.1f;
        } else if (entries > 4) {
            panel.FontSize = 0.8f;
        } else {
            panel.FontSize = 1.0f;
        }
        panel.FontColor = Color.LightGreen;
        panel.WriteText(lcdText);
    }
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
