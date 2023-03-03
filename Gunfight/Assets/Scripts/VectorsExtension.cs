using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorsExtension
{
    // Start is called before the first frame update
    public static Vector3 WithAxis(this Vector3 vector, Axis axis, float value)
    {
        return new Vector3(
            axis == Axis.X ? value : vector.x,
            axis == Axis.Y ? value : vector.y,
            axis == Axis.Z ? value : vector.z
            );
    }
}

public enum Axis
{
    X, Y, Z
}

