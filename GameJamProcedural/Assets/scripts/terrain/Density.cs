using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Density {
    NoiseVolume noiseVol1;
    NoiseVolume noiseVol2;
    NoiseVolume noiseVol3;
    
    public Density()
    {
        noiseVol1 = new NoiseVolumeSimplex();
        noiseVol2 = new NoiseVolumeSimplex();
        noiseVol3 = new NoiseVolumeSimplex();
    }
    
    public Density(int seed)
    {
        noiseVol1 = new NoiseVolumeSimplex();
        noiseVol2 = new NoiseVolumeSimplex();
        noiseVol3 = new NoiseVolumeSimplex();
    }

    public NoiseSample getDensity(float x, float y, float z)
    {
        return getDensityOld(x,y,z);
    }
    
    public NoiseSample getDensityOld(float x, float y, float z)
    {
        NoiseSample D = new NoiseSample();
        
        // mise à l'échelle
        x *= .012f; y *= .012f; z *= .012f;
        
        // plus grande probabilité d'avoir un terrain solide lorsqu'on descent
        D += new NoiseSample(-z*.2f-1.3f, new Vector3(0,0,-.2f));
        
        // force le sol a partir d'une certaine altitude
        float limitlow=.20f;
        if (z < limitlow)
            D += new NoiseSample((limitlow-z)*5, new Vector3(0,0,-5));
        
        // pics escarpés à haute altitude
        float limitHigh=0.8f;
        if (z > limitHigh)
            z -= (z-limitHigh)*.62f;
        
        // pas super beau
//        float col[] = noiseColVol1.interpolate(x*.07f,y*.07f,z*.07f);
//        x+=col[0]*8; y+=col[1]*8; z+=col[2]*8;
        
        D += noiseVol1.interpolate(x*0.723f, y*0.723f, z*0.723f)*3.5f;
        D += noiseVol2.interpolate(x*2.96f, y*2.96f, z*2.96f) * 1.7f;
        D += noiseVol3.interpolate(x*4.03f, y*4.03f, z*4.03f) * .7f;
        
        // D += noiseVol1.interpolate(x*8.21f, y*8.21f, z*8.21f) * .4f;
        // D += noiseVol2.interpolate(x*15.96f, y*15.96f, z*15.96f) * .2f;
        // D += noiseVol3.interpolate(x*31.03f, y*31.03f, z*31.03f) * .1f;
        
        return D;
    }
}
