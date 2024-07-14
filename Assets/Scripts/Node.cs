using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public bool isBlocked = false;
    public Vector3 coorddinates;

    public Node(Vector3 coordinates, bool isBlocked)
    {
        this.coorddinates = coordinates;
        this.isBlocked = isBlocked;
    }
    
}
