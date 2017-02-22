using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseVolume
{
    public abstract NoiseSample interpolate(float x, float y, float z);
}
