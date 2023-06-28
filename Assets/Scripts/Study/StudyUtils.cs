using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace VRSketch
{


    [Serializable]
    public struct StudySequenceData
    {
        public List<int> InteractionModeSequence;
        public List<int> SystemSequence;
        public List<int> ModelSequence;
        public List<bool> BreakTime;
        public List<bool> ShowExample;
        public List<float> TimeLimit;
    }

    public enum SketchSystem
    {
        Baseline = 0,
        Snap = 1,
        SnapSurface = 2
    }

    public enum SketchModel
    {
        Mouse = 0,
        Lamp = 1,
        Plane = 2,
        Shoe = 3,
    }

    public enum InteractionMode
    {
        Tutorial = 0,
        Normal = 1,
        FreeCreation = 2,
    }

    public enum InteractionType
    {
        Idle,
        StrokeAdd,
        StrokeDelete,
        SurfaceAdd,
        SurfaceDelete,
        CanvasTransform
    }

    public enum ControllerType
    {
        Vive = 0,
        Oculus = 1,
        Knuckles = 2
    }



    public static class StudyUtils
    {

        public static Dictionary<SketchModel, Plane> MirrorModelMapping = new Dictionary<SketchModel, Plane>
        {
            { SketchModel.Mouse, new Plane(Vector3.right, new Vector3(-0.125f, 0.125f, 0.125f)) },
            { SketchModel.Plane, new Plane(Vector3.forward, new Vector3(0.125f, 0.125f, 0f)) },
            { SketchModel.Lamp, new Plane(Vector3.forward, new Vector3(0.125f, 0.125f, 0f)) },
            { SketchModel.Shoe, new Plane(Vector3.right, new Vector3(0f, 0.125f, 0.125f)) },
        };

        public static Dictionary<InteractionMode, string> InteractionModeInstructions = new Dictionary<InteractionMode, string>
        {
            { InteractionMode.Tutorial, "Tutorial Mode: take as long as you like to explore the different features. \n Press MENU on Vive or B/Y on Oculus when you're done." },
            { InteractionMode.Normal, "Study task: sketch the model before the countdown ends. \n If you finish before, press MENU on Vive or B/Y on Oculus to end the task." },
            { InteractionMode.FreeCreation, "Free creation: sketch whatever you wish." +
                                            "\n When you're done, press MENU on Vive or B/Y on Oculus" +
                                            "\nto export your sketch and clear the canvas." +
                                            "\n\nPress ESC to exit the app." },
        };

        public static Dictionary<SketchSystem, string> SystemInstructions = new Dictionary<SketchSystem, string>
        {
            { SketchSystem.Baseline, "Freehand" },
            { SketchSystem.Snap, "Armature" },
            { SketchSystem.SnapSurface, "Patch" }
        };

        public static Dictionary<SketchModel, string> ModelNames = new Dictionary<SketchModel, string>
        {
            { SketchModel.Mouse, "Computer mouse" },
            { SketchModel.Lamp, "Desk lamp" },
            { SketchModel.Plane, "Airplane" },
            { SketchModel.Shoe, "Running shoe" },
        };



        public static bool TryLoadStudyData(out StudySequenceData ssd)
        {
            ssd = new StudySequenceData();
            try
            {
                string filename = Path.Combine(Application.streamingAssetsPath, "study_sequence.json");
                string jsonStr = File.ReadAllText(filename);
                ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo("en-US")
                });
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }

            return true;
        }

        public static bool IsRightHandedConfig()
        {

            try
            {
                var filename = Path.Combine(Application.streamingAssetsPath, "dominant_hand.txt");
                string data = File.ReadAllText(filename);

                if (!Int32.TryParse(data, out int handInt))
                    throw new Exception("Cannot open hand config file!");

                if (handInt == 0)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        public static ControllerType GetControllerType()
        {
            return (ControllerType)2;
        }

    }
}