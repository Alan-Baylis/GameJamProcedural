using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour {
    public class BoidObject {
        public Transform obj;
        public Vector3 velocity;
        public BoidObject(GameObject o, Transform parent) {
            obj = Instantiate(o, parent.position, Quaternion.identity).transform;
            velocity = Vector3.zero;
        }
    }
    public BoidObject[] boidObjs;
    public Transform player;
    
    public GameObject boidPrefab;
    public int numBoids = 10;
    
    public int boidMoveToPlayerSlowness = 10;
    public int boidFollowCenterSlowness = 10;
    public float boidToBoidDistance = .6f;
    public float boidToPlayerDistance = 1.2f;
    public float boidToBoidRepulsivity = .8f;
    public float boidToPlayerRepulsivity = 1.2f;
    public float boidVelocityDampen = .8f;
    
    public float targetVelocity = 1;
    
    void Start () {
        boidObjs = new BoidObject[numBoids];
        for (int i=0; i<numBoids; i++)
            boidObjs[i] = new BoidObject(boidPrefab, player);
    }
    
    void Update () {
        foreach (BoidObject boid in boidObjs)
        {
            boid.velocity *= boidVelocityDampen;
            
            // se recentre sur le joueur
            boid.velocity += (player.position - boid.obj.position) / boidMoveToPlayerSlowness;
            
            // évite ses congénères
            foreach (BoidObject other in boidObjs)
                if (other != boid)
                    if ((other.obj.position - boid.obj.position).magnitude < boidToBoidDistance)
                        boid.velocity += (boid.obj.position - other.obj.position).normalized * boidToBoidRepulsivity * (boidToBoidDistance - (boid.obj.position - other.obj.position).magnitude);
            
            // évite le joueur
            if ((player.position - boid.obj.position).magnitude < boidToPlayerDistance)
                boid.velocity += (boid.obj.position - player.position).normalized * boidToPlayerRepulsivity * (boidToPlayerDistance - (boid.obj.position - player.position).magnitude);
            
            // se recentre sur le point central des boids
            Vector3 center = Vector3.zero;
            foreach (BoidObject other in boidObjs)
                if (other != boid)
                    center += other.obj.position;
            center /= boidObjs.Length-1;
            boid.velocity += (center - boid.obj.position) / boidFollowCenterSlowness;
            
            // assigne une vélocité minimale aux boids
            if (boid.velocity.magnitude < targetVelocity)
                boid.velocity *= targetVelocity / (boid.velocity.magnitude+.001f);
            
            boid.obj.position += boid.velocity * Time.deltaTime;
        }
    }
}
