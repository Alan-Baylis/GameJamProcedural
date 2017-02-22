using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVolumeSimplex : NoiseVolume {
    
    public override NoiseSample interpolate(float x, float y, float z)
    {
        NoiseMethod method = Noise.methods[(int)NoiseMethodType.Simplex][2];
        NoiseSample sample = Noise.Sum(method, new Vector3(x,y,z), 1, 2, 2, .5f);
		return sample;
    }
}
