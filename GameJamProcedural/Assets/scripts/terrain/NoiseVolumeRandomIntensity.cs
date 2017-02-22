using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVolumeRandomIntensity : NoiseVolume {

    int size;
    int resolution;
    float[] values;
    Interpolation inter;
    
    public NoiseVolumeRandomIntensity(int size, int seed)
    {
        this.size = size;
        resolution = size * size * size;
        values = new float[resolution];
        
        System.Random generator;
        if (seed != -1)
            generator = new System.Random(seed);
        else
            generator = new System.Random();
        
        // créé la grille des valeurs
        for (int i=0; i<resolution; i++)
            values[i] = (float)generator.NextDouble();
        
        inter = new Interpolation();
    }
    
    float getValue(int x, int y, int z)
    {
        if (x == size) x -= size;
        if (y == size) y -= size;
        if (z == size) z -= size;
        return values[x + y*size + z*size*size];
    }
    
    
    public override NoiseSample interpolate(float x, float y, float z)
    {
        float decx = x - (int)x;
        float decy = y - (int)y;
        float decz = z - (int)z;
        
        int ix = (decx < 0) ? modulo((int) x-1, size) : modulo((int) x, size);
        int iy = (decy < 0) ? modulo((int) y-1, size) : modulo((int) y, size);
        int iz = (decz < 0) ? modulo((int) z-1, size) : modulo((int) z, size);
        
        if (x<0 && decx != 0)
            decx = 1+decx;
        if (y<0 && decy != 0)
            decy = 1+decy;
        if (z<0 && decz != 0)
            decz = 1+decz;
        
        float bound0 = getValue(ix,   iy,   iz);
        float bound1 = getValue(ix+1, iy,   iz);
        float bound2 = getValue(ix,   iy+1, iz);
        float bound3 = getValue(ix,   iy,   iz+1);
        float bound4 = getValue(ix+1, iy,   iz+1);
        float bound5 = getValue(ix,   iy+1, iz+1);
        float bound6 = getValue(ix+1, iy+1, iz);
        float bound7 = getValue(ix+1, iy+1, iz+1);
        
        NoiseSample ns = new NoiseSample();
        ns.value = inter.interpolate(decx, decy, decz,
                             new float[] {bound0,bound1,bound2,bound3,
                             bound4,bound5,bound6,bound7} );
        return ns;
    }
    
    /**
     * Modulo rapide fonctionnant avec les nombres négatifs
     */
    static int modulo(int x, int y)
    {
        if (x < 0)
            x = y-modulo(-x, y);
        return x & (y-1);
    }
}
