using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVerts : MonoBehaviour {
    void OnDrawGizmos () {
        // Gizmos.color = color;
        Transform transform = this.transform;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertices != null)
            foreach (Vector3 vert in mf.sharedMesh.vertices)
                Gizmos.DrawWireSphere(transform.TransformPoint(vert), .5f);
    }
}
