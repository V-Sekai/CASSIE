using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabController : MonoBehaviour
{

    public DrawingCanvas canvas;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 positionOffset;
    private Quaternion q0;
    private Quaternion qObj;


    public void GrabStart(Vector3 handPos, Quaternion handRot)
    {
        startPosition = handPos;
        startRotation = handRot;

        positionOffset = canvas.transform.position - startPosition;
        q0 = Quaternion.Inverse(startRotation);
        qObj = canvas.transform.rotation;
    }

    public void GrabUpdate(Vector3 handPos, Quaternion handRot)
    {
        if (Vector3.Distance(handPos, startPosition) > 0.01f || Quaternion.Angle(startRotation, handRot) > 0.1f)
        {
            Vector3 endPosition = handPos;
            Quaternion q1 = handRot;

            canvas.transform.position = endPosition;
            canvas.transform.rotation = q1 * q0 * qObj;
            Vector3 correctedOffset = q1 * q0 * positionOffset;
            canvas.transform.position += correctedOffset;
        }
    }


}
