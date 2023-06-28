using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRSketch;
using Newtonsoft.Json;

//public enum ExportMode
//{
//    OBJ,
//    Curves,
//}

public class ExportController : MonoBehaviour
{
    [SerializeField]
    private DrawingCanvas canvas = null;


    //public ExportMode exportMode = ExportMode.Curves;

    [Header("Export formats")]

    [SerializeField]
    private bool ExportFinalStrokes = true;

    [SerializeField]
    private bool ExportInputStrokes = true;

    [SerializeField]
    private bool ExportSketchOBJ = true;

    [SerializeField]
    private bool ExportGraphData = true;


    [Header("Export options")]

    public float StrokeWidth;
    public int SubdivisionsPerUnit;

    public bool ClearAfterExport = false;

    [Header("Export folder name")]

    [SerializeField]
    private string ExportFolderName = "SketchData~"; // Tilde makes Unity ignore this folder, so it doesn' t try to "import" the content as Assets (which breaks with the custom .curves format)

    public void ExportSketch(string fileName = null)
    {
        if (ExportSketchOBJ)
        {
            Debug.Log("[EXPORT] Exporting sketch as OBJ.");
            ExportToOBJ(fileName); // default file name
        }

        if (ExportFinalStrokes)
        {
            Debug.Log("[EXPORT] Exporting sketch as .curves (final strokes).");
            ExportToCurves(fileName, finalStrokes: true); // default file name

        }

        if (ExportInputStrokes)
        {
            Debug.Log("[EXPORT] Exporting sketch as .curves (input strokes).");
            ExportToCurves(fileName, finalStrokes: false); // default file name
        }

        if (ExportGraphData)
        {
            Debug.Log("[EXPORT] Exporting graph data as JSON file.");
            ExportCurveNetwork(fileName);
        }

        if (ClearAfterExport)
        {
            Debug.Log("[EXPORT] Clearing the scene.");
            canvas.Clear();
        }
    }

    public void ExportToOBJ(string fileName=null)
    {
        string name = fileName ?? DefaultFileName();

        List<int> strokeIDs = new List<int>();
        List<int> patchIDs = new List<int>();

        // STROKES
        string path = Path.Combine(Application.dataPath, ExportFolderName);

        // Try to create the directory
        TryCreateDirectory(path);

        if (canvas.Strokes.Count > 0)
        {
            string fileNameStrokes = name + "_strokes" + ".obj";
            string fullfileNameStrokes = Path.Combine(path, fileNameStrokes);
            File.Create(fullfileNameStrokes).Dispose();

            string curves = "";
            ObjExporterScript.Start();

            foreach (FinalStroke s in canvas.Strokes)
            {
                // First compute the mesh for the stroke
                //Mesh mesh = StrokeBrush.Solidify(s.Curve);
                int tubularSegments = Mathf.CeilToInt(s.Curve.GetLength() * SubdivisionsPerUnit);

                Mesh mesh = Tubular.Tubular.Build(s.Curve, tubularSegments, StrokeWidth);

                // Add stroke ID as a group name

                curves += string.Format("g {0}\n", s.ID);

                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                curves += objString;

                // Store ID
                if (s as FinalStroke != null)
                    strokeIDs.Add(((FinalStroke)s).ID);
            }
            ObjExporterScript.End();
            File.WriteAllText(fullfileNameStrokes, curves);
        }


        // PATCHES
        SurfacePatch[] allPatches = canvas.GetComponentsInChildren<SurfacePatch>();

        if (allPatches.Length > 0)
        {
            string fileNamePatches = name + "_patches" + ".obj";
            string fullfileNamePatches = Path.Combine(path, fileNamePatches);
            File.Create(fullfileNamePatches).Dispose();

            string patches = "";
            ObjExporterScript.Start();



            foreach (SurfacePatch s in allPatches)
            {
                // Add cycle ID as a group name
                patches += string.Format("g {0}\n", s.GetID());

                string objString = ObjExporterScript.MeshToString(
                                        s.GetComponent<MeshFilter>().sharedMesh,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                patches += objString;

                patchIDs.Add(s.GetID());
            }
            ObjExporterScript.End();
            File.WriteAllText(fullfileNamePatches, patches);
        }

        // SKETCH END DATA
        SketchEndData sketchData = new SketchEndData(strokeIDs, patchIDs);

        var sketchDataName = name + "_sketch.json";
        var fullfileNameSketch = Path.Combine(path, sketchDataName);

        File.WriteAllText(fullfileNameSketch, JsonConvert.SerializeObject(sketchData, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));

    }

    public void ExportToCurves(string fileName = null, bool finalStrokes = true)
    {
        string name = fileName ?? DefaultFileName();

        string path = Path.Combine(Application.dataPath, ExportFolderName);

        // Try to create the directory
        TryCreateDirectory(path);

        string fileNameCurve = name;

        if (!finalStrokes)
            fileNameCurve += "-input";

        fileNameCurve += ".curves";
        string fullfileNameCurve = Path.Combine(path, fileNameCurve);

        File.Create(fullfileNameCurve).Dispose();

        StringBuilder curves = new StringBuilder();

        foreach (FinalStroke s in canvas.Strokes)
        {

            if (finalStrokes)
            {
                string curveString = CurvesExport.CurveToPolyline(s.Curve, SubdivisionsPerUnit);
                curves.Append(curveString);
            }
            else
            {
                string stroke = CurvesExport.SamplesToPolyline(s.inputSamples);
                curves.Append(stroke);
            }
        }

        File.WriteAllText(fullfileNameCurve, curves.ToString());
    }

    public void ExportCurveNetwork(string fileName=null)
    {
        string name = fileName ?? DefaultFileName();

        string path = Path.Combine(Application.dataPath, ExportFolderName);

        // Try to create the directory
        TryCreateDirectory(path);

        string fileNameNet = name + "_graph.json";

        List<SerializableStrokeInGraph> strokes = new List<SerializableStrokeInGraph>();
        List<SerializableSegment> segments = new List<SerializableSegment>();
        List<SerializableNode> nodes = canvas.Graph.GetNodesData();

        foreach (FinalStroke s in canvas.Strokes)
        {
            (SerializableStrokeInGraph strokeData, List<SerializableSegment> strokeSegments) = s.GetGraphData();
            strokes.Add(strokeData);
            segments.AddRange(strokeSegments);
        }


        CurveNetworkData graphData = new CurveNetworkData(strokes, segments, nodes);

        File.WriteAllText(Path.Combine(path, fileNameNet), JsonConvert.SerializeObject(graphData, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));
    }

    private string DefaultFileName()
    {
        return (DateTime.Now).ToString("yyyyMMddHHmmss");
    }

    private void TryCreateDirectory(string path)
    {
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
    }
}
