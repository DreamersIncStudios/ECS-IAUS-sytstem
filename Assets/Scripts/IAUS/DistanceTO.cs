using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS;

public class DistanceTO : ConsiderationBase
{
    public float MaxRange = 50.0f;
    public float MinRange = 3.0f; 
    //public IConsideration Clone()
    //{
    //    throw new System.NotImplementedException();
    //}
    public GameObject Point;
    public GameObject Agent;
    public override void Consider()
    {
        Score = new float();
        float dist = Vector3.Distance(Point.transform.position, Agent.transform.position);
        if (Inverse)
        {
            Score = 1.0f - Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
        }
        else
        {
            Score = Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
        }


    }



}
