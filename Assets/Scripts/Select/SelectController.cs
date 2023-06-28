using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SelectController : MonoBehaviour
{

    public DrawingCanvas canvas;

    public void OnDeleteCollision(Collider collided)
    {
        if (collided.GetComponent<FinalStroke>() != null)
            canvas.UpdateToDelete(collided.GetComponent<FinalStroke>());

        if (collided.GetComponent<SurfacePatch>() != null)
            canvas.UpdateToDelete(collided.GetComponent<SurfacePatch>());
    }

    public void OnDeleteCollisionExit(Collider collided)
    {
        if (collided.GetComponent<Stroke>() != null)
            canvas.ClearToDelete(collided.GetComponent<Stroke>());

        if (collided.GetComponent<SurfacePatch>() != null)
            canvas.ClearToDelete(collided.GetComponent<SurfacePatch>());
    }
    
}
