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
using Sandbox.Game.WorldEnvironment.Definitions;

namespace SpaceEngineers.UWBlockPrograms.SupervisionScript {
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
    writeDisplay("");

    try {
        EnergySupervision();
    } catch (Exception e) {
        writeDisplay("Problem with Energy Supervision!\n" + e, Color.Red);
    }

    try {
        CargoSupervision();
    } catch (Exception e) {
        writeDisplay("Problem with Cargo Supervision!\n" + e, Color.Red);
    }

    try {
        ResourceSupervision();
    } catch (Exception e) {
        writeDisplay("Problem with Resource Supervision!\n" + e, Color.Red);
    }

    try {
        GunSupervision();
    } catch (Exception e) {
        writeDisplay("Problem with Gun Supervision!\n" + e, Color.Red);
    }
}

public void EnergySupervision() {
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

public void GunSupervision() {
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

    bool targetNeutrals = false;

    // Gatling Section
    foreach (var gatling in gatlings) {
        if (gatling.GetValue<bool>("TargetNeutrals")) {
            targetNeutrals = true;
        }
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

    if (targetNeutrals) {
        printString += "\n>>> Targeting Neutrals! <<<\n";
    }
    
    foreach (var panel in panels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Red;
        panel.WriteText(printString);
    }
}

public void ResourceSupervision() {
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
            } else if (item.Type.TypeId == "MyObjectBuilder_Ore" && itemName != "Scrap") {
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
        panel.FontSize = 1.2f;
        panel.FontColor = Color.Aqua;
        panel.WriteText(printString);
    }
}

public void CargoSupervision() {
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName("CPanels");
    panelGroup.GetBlocksOfType<IMyTextPanel>(panels);

    float tempVal;
    string printString = "";
    float maxInv = 0;
    float curInv = 0;
    foreach (IMyTerminalBlock block in blocks) {
        if (!block.HasInventory || block is IMyReactor 
        || !block.ShowInInventory || block.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;

        IMyInventory inv = block.GetInventory();
        maxInv += (float)inv.MaxVolume;
        curInv += (float)inv.CurrentVolume;
        tempVal = (float)inv.CurrentVolume * 100 / (float)inv.MaxVolume;
        printString += $"{block.CustomName}: {tempVal:F1}%\n";
    }

    int fillRatio = (int)(curInv * 100 / maxInv);
    printString = $"Total Ratio: {fillRatio}%\n----------\n{printString}";
    foreach (IMyTextPanel panel in panels) {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.FontSize = 1.0f;
        panel.FontColor = Color.Orange;
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

public void writeDisplay(string msg) {
    writeDisplay(msg, Color.Green);
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
