using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGlobalVariables : MonoBehaviour {

    [SerializeField]
    private Texture2D noiseOffsetTexture;

    void Update()
    {
        Shader.SetGlobalTexture("_NoiseOffsets", noiseOffsetTexture);
        Shader.SetGlobalFloat("_AspectRatio", (float)Screen.width / (float)Screen.height);
        Shader.SetGlobalFloat("_FieldOfView", Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * 2f);
    }
}
