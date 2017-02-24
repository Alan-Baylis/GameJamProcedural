using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu ("Image Effects/Blink")]
public class BlinkDistortion : UnityStandardAssets.ImageEffects.PostEffectsBase
{
    public float intensity = 0.2f;
    public float desaturation = 0.2f;
    public Shader blinkShader;
    
    private Material m_BlinkMaterial;


    public override bool CheckResources ()
    {
        CheckSupport (false);

        m_BlinkMaterial = CheckShaderAndCreateMaterial (blinkShader, m_BlinkMaterial);

        if (!isSupported)
            ReportAutoDisable ();
        return isSupported;
    }


    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if ( CheckResources () == false)
        {
            Graphics.Blit (source, destination);
            return;
        }

        m_BlinkMaterial.SetFloat ("_Intensity", intensity);
        m_BlinkMaterial.SetFloat ("_Desaturation", desaturation);

        source.wrapMode = TextureWrapMode.Clamp;
        Graphics.Blit (source, destination, m_BlinkMaterial, 1);
    }
}
