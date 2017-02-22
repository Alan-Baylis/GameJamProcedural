using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour {

    void Update()
    {
        Shader.SetGlobalVector("_CamPos", this.transform.position);
        Shader.SetGlobalVector("_CamRight", this.transform.right);
        Shader.SetGlobalVector("_CamUp", this.transform.up);
        Shader.SetGlobalVector("_CamForward", this.transform.forward);
    }

}
