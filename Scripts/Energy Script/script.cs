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

namespace SpaceEngineers.UWBlockPrograms.EnergyScript {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
public void Main(string argument, UpdateType updateSource) {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);

    List<IMySolarPanel> solars = new List<IMySolarPanel>();
    
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
