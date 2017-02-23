using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaindropEffect : MonoBehaviour {

    private Material material;


    // Use this for initialization
    void Start () {
        material = new Material(Shader.Find("Hidden/rainEffect"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
