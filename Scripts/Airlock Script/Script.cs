IMyTerminalBlock deactivatedDoor;
int delay = 0;

public void Main(string argument, UpdateType updateSource) {
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
