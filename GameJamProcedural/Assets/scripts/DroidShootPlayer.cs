using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroidShootPlayer : MonoBehaviour {
    public PlayerFirstPerson player;
    public float shootRange = 10;
    public float shootCooldown = 2;
    public float shootCooldownRandomAdd = 1.2f;
    public float accuracyPercent = 20;
    public GameObject beamPrefab;
    
    float shootCooldownTimer = 0;
    
	void Start () {
		
	}
	void Update () {
        shootCooldownTimer -= Time.deltaTime;
        if (shootCooldownTimer < 0) shootCooldownTimer = 0;
        
        Vector3 shootDirection = player.transform.position - transform.position;
		if (shootDirection.sqrMagnitude < shootRange*shootRange)
        {
            if (shootCooldownTimer <= 0)
                Shoot(shootDirection);
        }
	}
    
    void Shoot(Vector3 shootDirection)
    {
        Debug.Log("Shoot");
        shootCooldownTimer = shootCooldown * Random.value*shootCooldownRandomAdd;
        RaycastHit hit;
        if (Random.value * 100 > accuracyPercent)
            shootDirection = Quaternion.Lerp(Quaternion.identity, Random.rotation, .05f) * shootDirection;
        Quaternion shootDirectionQuat = new Quaternion();
        shootDirectionQuat.SetLookRotation(shootDirection);
        if (Physics.Raycast(transform.position, shootDirection, out hit))
        {
            GameObject laser = Instantiate(beamPrefab, transform.position, shootDirectionQuat);
            laser.transform.localScale = new Vector3(1,1,hit.distance);
            Debug.Log("Hit " + hit.collider.gameObject.name);
            if (hit.collider.tag == "Player")
            {
                Debug.Log("Hit player !");
                player.TakeHit();
            }
        }
        else
        {
            GameObject laser = Instantiate(beamPrefab, transform.position, shootDirectionQuat);
            laser.transform.localScale = new Vector3(1,1,10000);
        }
    }
}
