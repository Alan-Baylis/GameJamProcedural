using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootBoids : MonoBehaviour {
    public int scoreJoueur;
    public GameObject arme;
	// Use this for initialization
	void Start () {
        scoreJoueur = 0;
    }

    public LineRenderer lineRenderer;
    public Object fireExplosion; 
	
	// Update is called once per frame
	void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Input.GetMouseButtonDown(1))
        {
            lineRenderer.SetPosition(0, arme.transform.position);// + Camera.main.transform.right*.3f - Camera.main.transform.up * .3f);
            lineRenderer.SetPosition(1, arme.transform.position+arme.transform.right+Camera.main.transform.forward*100);

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                if(hit.collider.gameObject != null && hit.collider.gameObject.tag == "Ennemy")
                {
                    Instantiate(fireExplosion, hit.collider.gameObject.transform.position, Quaternion.identity);
                    hit.collider.gameObject.SetActive(false);
                    scoreJoueur++;
                }
            }
        }
    }

    void OnGUI()
    {
        Rect rect = new Rect(500, 10, 120, 20);
        GUI.Label(rect, "Score: " + scoreJoueur); 
    }
}
