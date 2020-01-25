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
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers.UWBlockPrograms.CockpitDisplay_Part {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
IMyTerminalBlock deactivatedDoor;
int delay = 0;

public void Main(string argument, UpdateType updateSource) {
   var airVent = GridTerminalSystem.GetBlockWithName("Airlock Vent");
   var outDoor = GridTerminalSystem.GetBlockWithName("Sliding Door Outside");
   var inDoor = GridTerminalSystem.GetBlockWithName("Sliding Door Inside");

   outDoor.ApplyAction("OnOff_On");
   inDoor.ApplyAction("OnOff_On");
   Runtime.UpdateFrequency = UpdateFrequency.None;
 
   if (argument == "press") {
       outDoor.ApplyAction("Open_Off");
       inDoor.ApplyAction("Open_Off");
       airVent.ApplyAction("Depressurize_Off");
       Runtime.UpdateFrequency = UpdateFrequency.Update100;
       deactivatedDoor = outDoor;
       delay = 1;
       return;
   }

   if (argument == "depress") {
       outDoor.ApplyAction("Open_Off");
       inDoor.ApplyAction("Open_Off");
       airVent.ApplyAction("Depressurize_On");
       Runtime.UpdateFrequency = UpdateFrequency.Update100;
       deactivatedDoor = inDoor;
       delay = 1;
       return;
   }  
   
   if (deactivatedDoor != null && delay <= 0) {
       deactivatedDoor.ApplyAction("OnOff_Off");
   }  else if (delay > 0) {
       Runtime.UpdateFrequency = UpdateFrequency.Update100;
       delay--;
   }
} 

// YOUR CODE END
#region PreludeFooter
    }
}
#endregion