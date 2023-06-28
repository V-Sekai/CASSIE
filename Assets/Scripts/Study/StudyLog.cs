using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

namespace VRSketch
{

    // WARNING: I think there is something wrong with serialization of Vector3 and Quaternions: ToString methods are not called, the serialization is simply something like {"x": value, "y": value, "z": value}.
    // Won't fix since the whole study was done with that error in place, and analysis code was geared to that serialization format.
    [Serializable]
    public struct SerializableVector3
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return Utils.ChangeHandedness(new Vector3(rValue.x, rValue.y, rValue.z));
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            rValue = Utils.ChangeHandedness(rValue);
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }


    }

    [Serializable]
    public struct SerializableQuaternion
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// w component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        /// <param name="rW"></param>
        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        /// <summary>
        /// Automatic conversion from SerializableQuaternion to Quaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Quaternion(SerializableQuaternion rValue)
        {
            return Utils.ChangeHandedness(new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w));
        }

        /// <summary>
        /// Automatic conversion from Quaternion to SerializableQuaternion
        /// </summary>
        /// <param name = "rValue" ></ param >
        /// <returns></returns>
        public static implicit operator SerializableQuaternion(Quaternion rValue)
        {
            rValue = Utils.ChangeHandedness(rValue);
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }

    [Serializable]
    public struct SystemState
    {
        public InteractionType interactionType;
        public float time;
        public int elementID;
        public bool mirroring;
        public SerializableVector3 headPos;
        public SerializableQuaternion headRot;
        public SerializableVector3 primaryHandPos;
        public SerializableVector3 canvasPos;
        public SerializableQuaternion canvasRot;
        public float canvasScale;

        public SystemState(
            InteractionType type,
            int elementID,
            bool mirroring,
            SerializableVector3 headPos,
            SerializableQuaternion headRot,
            SerializableVector3 primaryHandPos,
            SerializableVector3 canvasPos,
            SerializableQuaternion canvasRot,
            float canvasScale)
        {
            this.interactionType = type;
            this.time = Time.time;
            this.elementID = elementID;
            this.mirroring = mirroring;
            this.headPos = headPos;
            this.headRot = headRot;
            this.primaryHandPos = primaryHandPos;
            this.canvasPos = canvasPos;
            this.canvasRot = canvasRot;
            this.canvasScale = canvasScale;
        }
    }

    [Serializable]
    public struct SerializableStroke
    {
        public int id;
        public List<SerializableVector3> ctrlPts;
        public List<SerializableVector3> inputSamples;
        public List<SerializableConstraint> appliedPositionConstraints;
        public List<SerializableConstraint> rejectedPositionConstraints;
        public bool onSurface;
        public bool planar;
        public bool closedLoop;

        public SerializableStroke(int id, List<Vector3> ctrlPts, List<Vector3> inputSamples, List<SerializableConstraint> appliedPositionConstraints, List<SerializableConstraint> rejectedPositionConstraints, bool onSurface, bool planar, bool closedLoop)
        {
            this.id = id;
            this.ctrlPts = ctrlPts.ConvertAll(x => (SerializableVector3)x);
            this.inputSamples = inputSamples.ConvertAll(x => (SerializableVector3)x);
            this.appliedPositionConstraints = appliedPositionConstraints;
            this.rejectedPositionConstraints = rejectedPositionConstraints;
            this.onSurface = onSurface;
            this.planar = planar;
            this.closedLoop = closedLoop;
        }

        public SerializableStroke(int id)
        {
            this.id = -1;
            this.ctrlPts = new List<SerializableVector3>(0);
            this.inputSamples = new List<SerializableVector3>(0);
            this.appliedPositionConstraints = new List<SerializableConstraint>(0);
            this.rejectedPositionConstraints = new List<SerializableConstraint>(0);
            this.onSurface = false;
            this.planar = false;
            this.closedLoop = false;
        }
    }

    [Serializable]
    public struct SerializableConstraint
    {
        public SerializableVector3 position;
        public bool isIntersection;
        public bool isAtExistingNode;
        public bool isAtNewEndpoint;
        public bool alignTangents;

        public SerializableConstraint(Vector3 position, bool isIntersection, bool isAtExistingNode, bool isAtNewEndpoint, bool alignTangents)
        {
            this.position = position;
            this.isIntersection = isIntersection;
            this.isAtExistingNode = isAtExistingNode;
            this.isAtNewEndpoint = isAtNewEndpoint;
            this.alignTangents = alignTangents;
        }
    }

    [Serializable]
    public struct SerializablePatch
    {
        public int id;
        public bool foundByAlgo;
        //public int segmentsCount;
        public List<int> strokesID;

        public SerializablePatch(int cycleID, bool foundByAlgo, List<int> strokesID)
        {
            this.id = cycleID;
            this.foundByAlgo = foundByAlgo;
            this.strokesID = strokesID;
        }
    }

    [Serializable]
    public struct SessionData
    {
        public SketchSystem sketchSystem;
        public SketchModel sketchModel;
        public InteractionMode interactionMode;
        public List<SystemState> systemStates;
        public List<SerializableStroke> allSketchedStrokes;
        public List<SerializablePatch> allCreatedPatches;

        public SessionData(SketchSystem sketchSystem, SketchModel sketchModel, InteractionMode mode, List<SystemState> systemStates, List<SerializableStroke> strokes, List<SerializablePatch> patches)
        {
            this.sketchSystem = sketchSystem;
            this.sketchModel = sketchModel;
            this.interactionMode = mode;
            this.systemStates = systemStates;
            this.allSketchedStrokes = strokes;
            this.allCreatedPatches = patches;
        }
    }

    [Serializable]
    public struct SketchEndData
    {
        public List<int> finalStrokes;
        public List<int> finalPatches;

        public SketchEndData(List<int> finalStrokes, List<int> finalPatches)
        {
            this.finalPatches = finalPatches;
            this.finalStrokes = finalStrokes;
        }
    }

    public static class StudyLog
    {
        public static void SaveData(SessionData sessionData, string fileName = "")
        {
            string path = Path.Combine(Application.dataPath, "SketchData~");
            // Try to create the directory
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }



            int systemID = (int)sessionData.sketchSystem;
            int modelID = (int)sessionData.sketchModel;
            int interactionModeID = (int)sessionData.interactionMode;
            string timestamp = (DateTime.Now).ToString("yyyyMMddHHmmss");
            var name = fileName + ".json";
            if (fileName.Length == 0)
                name = timestamp + "_" + interactionModeID + "_" + systemID + "_" + modelID + ".json";
            var fullfileName = Path.Combine(path, name);

            File.WriteAllText(fullfileName, JsonConvert.SerializeObject(sessionData, new JsonSerializerSettings
            {
                Culture = new System.Globalization.CultureInfo("en-US")
            }));
        }
    }

}