using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolation {
    public float interpolate(float x, float y, float z, float[] extradata)
    {
        return  extradata[0] * (1 - x) * (1 - y) * (1 - z) +
                extradata[1] * x * (1 - y) * (1 - z) +
                extradata[2] * (1 - x) * y * (1 - z) +
                extradata[3] * (1 - x) * (1 - y) * z +
                extradata[4] * x * (1 - y) * z +
                extradata[5] * (1 - x) * y * z +
                extradata[6] * x * y * (1 - z) +
                extradata[7] * x * y * z;
    }
}
