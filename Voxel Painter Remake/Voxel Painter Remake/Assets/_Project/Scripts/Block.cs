using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Face[] faces;
    public HashSet<Face> faceLookUp = new HashSet<Face>();
    private void Awake()
    {
        foreach (Face face in faces)
            faceLookUp.Add(face);
    }
    public void SetColor(Color c) {
        foreach (Face face in faces)
        {
            face.SetColor(c);
        }
    }
}
