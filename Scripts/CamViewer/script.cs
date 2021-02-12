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
using Sandbox.Engine.Utils;
using System.Runtime.CompilerServices;
using VRageRender;
using VRage.Network;

namespace SpaceEngineers.UWBlockPrograms.CamViewer {
    public sealed class Program : MyGridProgram {
#endregion
// YOUR CODE BEGIN
// USER SETTINGS
const int RESOLUTION = 12;
const int RANGE = 120;
// USER SETTINGS END

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlockGroupWithName("CamScreens").GetBlocksOfType<IMyTextPanel>(panels);
    foreach (var panel in panels) {
        panel.ContentType = ContentType.SCRIPT;
        panel.Script = "";
        panel.ScriptBackgroundColor = Color.Black;
        panel.ScriptForegroundColor = Color.White;
    }
    camData = new Vector3D?[RESOLUTION*RESOLUTION];
}

IMyCameraBlock cam;
int colCounter = 0;
Vector3D?[] camData;
public void Main(string argument, UpdateType updateSource) {
    writeDisplay("");
    if (argument != "" || cam == null) {
        try {
            cam = (IMyCameraBlock)GridTerminalSystem.GetBlockWithName(argument);
            cam.EnableRaycast = true;
        } catch (Exception) {
            writeDisplay($"Cam with Name '{argument}' does not exist!\nAdd right name as argument!\n");
            return;
        }
    }

    if (!GetCamData()) {
        writeDisplay("Waiting for next column...\n", true);
        return;
    }

    UpdateDisplay();
}

public void UpdateDisplay() {
    List<IMyTextPanel> panels = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlockGroupWithName("CamScreens").GetBlocksOfType<IMyTextPanel>(panels);

    foreach (var panel in panels) {
        var frame = panel.DrawFrame();
        float center = panel.SurfaceSize.Y/2;
        float planeDist = center/(float)Math.Sin(cam.RaycastConeLimit);
        Matrix matrix = cam.WorldMatrix;
        Vector3D planeCenter = matrix.Forward;
        PlaneD plane = new PlaneD(planeCenter, planeCenter + matrix.Down, planeCenter + matrix.Left);
        
        for (int i = 0; i < camData.Length; i++) {
            if (camData[i] == null) {
                continue;
            }

            Vector3D dp0 = (Vector3D)camData[i];
            Vector3D dp1 = plane.Intersection(ref Vector3D.Zero, ref dp0);

            Vector3D planePointVector = planeCenter - dp1;
            double ppvDistance = planePointVector.Length();
            planePointVector.Normalize();

            double dotProduct = planePointVector.Dot(matrix.Left);
            double angle = Math.Acos(dotProduct);

            float y = (float)(ppvDistance * Math.Sin(angle)) * 2.1f;
            float x = (float)(ppvDistance * Math.Cos(angle)) * 2.1f;
            if (i > camData.Length/2) {
                y = -y;
            }

            float relativeDistance = (float)Math.Pow((dp0.Length() / RANGE), 2);
            float fontSize = (0.1f / relativeDistance) / (RESOLUTION/2f);
            fontSize = fontSize > 2f/((float)RESOLUTION/12) ? 2f/((float)RESOLUTION/12) : fontSize;
            frame.Add(new MySprite() {
                Type = SpriteType.TEXT,
                Data = "x",
                Position = new Vector2(center + x*center, center + y*center - fontSize*24),
                RotationOrScale = fontSize,
                Color = new Color(1/relativeDistance),
                FontId = "MonoSpace"
            });
        }
        frame.Dispose();
    }
}

public bool GetCamData() {
    float angleStep = cam.RaycastConeLimit / RESOLUTION;
    int curStep = 0;
    Vector3D camPos = cam.GetPosition();
    writeDisplay($"Range:{cam.AvailableScanRange:F2}m\nNeeded: {RESOLUTION*RANGE}m\n", true);
    
    if (cam.AvailableScanRange >= RESOLUTION * RANGE) {
        while (curStep < RESOLUTION) {
            int dataPos = colCounter * RESOLUTION + curStep;
            Vector3D? dp = cam.Raycast(RANGE, angleStep*(colCounter-RESOLUTION/2), angleStep*(curStep-RESOLUTION/2)).HitPosition;
            if (dp != null) {
                dp -= (Vector3D?)camPos;
            }

            camData[dataPos] = dp;
            curStep += 1;
        }
        colCounter++;
        if (colCounter >= RESOLUTION) {
            colCounter = 0;
        }
        return true;
    }
    return false;
}

public void writeDisplay(string msg, bool append = false) {
    writeDisplay(msg, Color.Red, append);
}

public void writeDisplay(string msg, Color color, bool append = false) {
    IMyTextSurface display = Me.GetSurface(0);
    display.ContentType = ContentType.TEXT_AND_IMAGE;
    display.FontColor = color;
    display.Alignment = TextAlignment.CENTER;
    display.FontSize = 1.2f;
    display.WriteText(msg, append);
}

// YOUR CODE END
#region PreludeFooter
    }
}
#endregion
