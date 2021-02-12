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
using System.Linq.Expressions;
using Sandbox.Game;
using System.Diagnostics.Contracts;
using VRage.Network;
using System.Diagnostics;
using Sandbox.Graphics.GUI;
using Sandbox.Game.Entities;
using System.Runtime.InteropServices.WindowsRuntime;
using VRage.Game.VisualScripting;

namespace SpaceEngineers.UWBlockPrograms.SpaceElevator {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
string arg = "";
int delay = 0;
const int DELAY = 4;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}


public void Main(string argument, UpdateType updateSource) {
    Elevator.lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("SPE_Status");
    Elevator.connector = (IMyShipConnector)GridTerminalSystem.GetBlockWithName("SPE_Connector");
    Elevator.piston = (IMyPistonBase)GridTerminalSystem.GetBlockWithName("SPE_Piston");
    Elevator.mergerTop = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("SPE_MergerTop");
    Elevator.mergerBottom = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("SPE_MergerBottom");

    if (arg != argument && argument != "") {
        arg = argument;
    }
    if (delay > 0) {
        delay--;
        return;
    } else {
        delay = DELAY;
    }

    Elevator.CheckStatus(arg);
    if (Elevator.Step(arg)) {
        delay = DELAY;
    }
}


class Elevator {
    public const float PISTON_SPEED = 2.7f;
    public static IMyShipMergeBlock mergerBottom;
    public static IMyShipMergeBlock mergerTop;
    public static IMyShipConnector connector;
    public static IMyPistonBase piston;
    public static IMyTextPanel lcd;
    public enum STATUS {
        Extending,
        Retracting,
        Stopped
    }
    private static STATUS curStatus;

    public static bool Step(string command) {
        if (command == "Up") {
            LCDWrite("ASCENDING\n----------\n");
            LCDWrite($"Status: {curStatus}\n", false);
            switch (curStatus) {
                case STATUS.Extending:
                    return ExtendUp();
                case STATUS.Retracting:
                    return RetractUp();
            }

        } else if (command == "Down") {
            LCDWrite("DESCENDING\n----------\n");
            LCDWrite($"Status: {curStatus}\n", false);
            switch (curStatus) {
                case STATUS.Extending:
                    return ExtendDown();
                case STATUS.Retracting:
                    return RetractDown();
            }
        } else {
            piston.Velocity = 0;
            LCDWrite("STOPPED\n----------\n");
            LCDWrite($"Status: {curStatus}\n", false);
        }
        return false;
    }

    public static void CheckStatus(string arg) {
        if (connector.Status == MyShipConnectorStatus.Connectable && mergerBottom.IsConnected && !mergerTop.IsConnected) {
            connector.Connect();
        }
        
        // Moving upwards -> merger = Top && unmerger = Bottom
        IMyShipMergeBlock merger;
        IMyShipMergeBlock unmerger;
        if (arg == "Up") {
            merger = mergerTop;
            unmerger = mergerBottom;
        } else if (arg == "Down") {
            merger = mergerBottom;
            unmerger = mergerTop;
        } else {
            curStatus = STATUS.Stopped;
            return;
        }

        if (unmerger.IsConnected && merger.IsConnected) {
            if (piston.CurrentPosition < 5) {
                curStatus = STATUS.Retracting;
                return;
            } else {
                curStatus = STATUS.Extending;
                return;
            }
        }
        
        if (unmerger.IsConnected) {
            curStatus = STATUS.Extending;
            return;
        }
        
        if (merger.IsConnected) {
            curStatus = STATUS.Retracting;
            return;
        }
    }

    private static bool ExtendUp() {
        piston.Velocity = PISTON_SPEED;
        if (piston.CurrentPosition < 9.9) {
            return false;
        }

        mergerTop.ApplyAction("OnOff_On");
        connector.Disconnect();
        if (mergerTop.IsConnected) {
            mergerBottom.ApplyAction("OnOff_Off");
        }
        return true;
    }

    private static bool RetractUp() {
        piston.Velocity = -PISTON_SPEED;
        if (piston.CurrentPosition > 0.1) {
            return false;
        }

        mergerBottom.ApplyAction("OnOff_On");
        if (mergerBottom.IsConnected) {
            mergerTop.ApplyAction("OnOff_Off");
        }
        return true;
    }

    private static bool ExtendDown() {
        piston.Velocity = PISTON_SPEED;
        if (piston.CurrentPosition < 9.9) {
            return false;
        }

        mergerBottom.ApplyAction("OnOff_On");
        mergerTop.ApplyAction("OnOff_Off");
        return true;
    }

    private static bool RetractDown() {
        piston.Velocity = -PISTON_SPEED;
        if (piston.CurrentPosition > 0.1) {
            return false;
        }

        mergerTop.ApplyAction("OnOff_On");
        connector.Disconnect();
        if (mergerTop.IsConnected) {
            mergerBottom.ApplyAction("OnOff_Off");
        }
        return true;
    }

    private static void LCDWrite(string msg, bool clear = true) {
        lcd.ContentType = ContentType.TEXT_AND_IMAGE;
        lcd.FontSize = 1.1f;
        lcd.FontColor = Color.LightBlue;
        lcd.WriteText(msg, !clear);
    }
}
// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
