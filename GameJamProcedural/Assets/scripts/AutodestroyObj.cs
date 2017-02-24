using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutodestroyObj : MonoBehaviour {
    public float timeout = 1;

	void Start () {
		StartCoroutine(Die());
	}
    
    IEnumerator Die()
    {
        yield return new WaitForSeconds(timeout);
        Destroy(this.gameObject);
    }
}
