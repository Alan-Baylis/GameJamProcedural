using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour {
    public TerrainLoader terrain;
    public class BoidObject {
        public Transform obj;
        public Vector3 velocity;
        public Vector3 terrainAvoidanceVelocity;
        public Chunk currentChunk = null;
        public int[] currentChunkBlockIndex;
        public Chunk.Block currentChunkBlock = null;
        public bool seesPlayer = true;
        public Vector3 lastKnownPlayerPosition;
        public DroidShootPlayer shootScript;
        public BoidObject(GameObject o, Transform parent, PlayerFirstPerson playerControlScript) {
            obj = Instantiate(o, parent.position, Quaternion.identity).transform;
            lastKnownPlayerPosition = parent.position;
            shootScript = obj.GetComponent<DroidShootPlayer>();
            shootScript.player = playerControlScript;
            velocity = Vector3.zero;
            terrainAvoidanceVelocity = Vector3.zero;
            currentChunkBlockIndex = new int[3];
        }
    }
    public BoidObject[] boidObjs;
    public Transform player;
    public PlayerFirstPerson playerControlScript;
    
    public GameObject boidPrefab;
    public int numBoids = 10;
    
    public int boidMoveToPlayerSlowness = 10;
    public int boidFollowCenterSlowness = 10;
    public float boidToBoidDistance = .6f;
    public float boidToPlayerDistance = 1.2f;
    public float boidToBoidRepulsivity = .8f;
    public float boidToPlayerRepulsivity = 1.2f;
    public float boidVelocityDampen = .8f;
    public float boidTerrainRepulsivity = 3;
    public int numBlockAvoidanceShells = 3;
    
    Vector3 spVelocity = Vector3.zero;
    public int spawnerMoveToPlayerSlowness = 5;
    public float spawnerToPlayerDistance = 100;
    public float spawnerToPlayerRepulsivity = 1.5f;
    public float spawnerMaxSpeed = 20;
    
    public float targetVelocity = 1;
    
    void Start () {
        boidObjs = new BoidObject[numBoids];
        for (int i=0; i<numBoids; i++)
            boidObjs[i] = new BoidObject(boidPrefab, transform, playerControlScript);
    }
    
    void Update () {
        foreach (BoidObject boid in boidObjs)
        {
            if (!boid.obj.gameObject.activeSelf)
                continue;
            if (terrain.isInNewBlock(boid))
            {
                boid.terrainAvoidanceVelocity = Vector3.zero;
                if (boid.currentChunkBlock.density > 0)
                {
                    boid.terrainAvoidanceVelocity = boid.currentChunkBlock.direction * boidTerrainRepulsivity;
                }
                for (int shellId=0; shellId<numBlockAvoidanceShells; shellId++)
                {
                    int numBlocksPerEdge = 3 + shellId*2;
                    List<Chunk.Block> blocks = new List<Chunk.Block>();
                    for (int a=0; a<numBlocksPerEdge; a++)
                    for (int b=0; b<numBlocksPerEdge; b++)
                    {
                        // face du bas
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[1]-(shellId+1),
                                boid.currentChunkBlockIndex[2]+b-(int)(numBlocksPerEdge/2)));
                        // face du haut
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[1]+(shellId+1),
                                boid.currentChunkBlockIndex[2]+b-(int)(numBlocksPerEdge/2)));
                        // face de derrière
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[1]+b-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[2]+(shellId+1)));
                        // face de devant
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[1]+b-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[2]-(shellId+1)));
                        // face de gauche
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]-(shellId+1),
                                boid.currentChunkBlockIndex[1]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[2]+b-(int)(numBlocksPerEdge/2)));
                        // face du haut
                        blocks.Add( terrain.getBlock(boid.currentChunk,
                                boid.currentChunkBlockIndex[0]+(shellId+1),
                                boid.currentChunkBlockIndex[1]+a-(int)(numBlocksPerEdge/2),
                                boid.currentChunkBlockIndex[2]+b-(int)(numBlocksPerEdge/2)));
                    }
                    foreach (Chunk.Block block in blocks)
                    {
                        if (block != null && block.density > 0)
                            boid.terrainAvoidanceVelocity += block.direction / (shellId+1) * boidTerrainRepulsivity;
                    }
                }
                
                RaycastHit hit;
                if (Physics.Raycast(boid.obj.position, (player.position - boid.obj.position), out hit))
                {
                    if (hit.collider.tag == "Player")
                    {
                        boid.shootScript.seesPlayer = true;
                        boid.seesPlayer = true;
                    }
                    else
                    {
                        boid.shootScript.seesPlayer = false;
                        boid.seesPlayer = false;
                    }
                }
                else
                {
                    boid.shootScript.seesPlayer = false;
                    boid.seesPlayer = false;
                }
            }
            
            boid.velocity += boid.terrainAvoidanceVelocity;
            
            boid.velocity *= boidVelocityDampen;
            
            // se recentre sur le joueur (si on peu le voir, ou bien si il est trop loin)
            if (boid.seesPlayer || boid.currentChunkBlockIndex == null || boid.currentChunk == null)
                boid.lastKnownPlayerPosition = player.position;
            boid.velocity += (boid.lastKnownPlayerPosition - boid.obj.position) / boidMoveToPlayerSlowness;
            
            // évite ses congénères
            foreach (BoidObject other in boidObjs)
                if (other != boid && other.obj.gameObject.activeSelf)
                    if ((other.obj.position - boid.obj.position).magnitude < boidToBoidDistance)
                        boid.velocity += (boid.obj.position - other.obj.position).normalized * boidToBoidRepulsivity * (boidToBoidDistance - (boid.obj.position - other.obj.position).magnitude);
            
            // évite le joueur
            if ((player.position - boid.obj.position).magnitude < boidToPlayerDistance)
                boid.velocity += (boid.obj.position - player.position).normalized * boidToPlayerRepulsivity * (boidToPlayerDistance - (boid.obj.position - player.position).magnitude);
            
            // se recentre sur le point central des boids
            Vector3 center = Vector3.zero;
            int totalAdd = 0;
            foreach (BoidObject other in boidObjs)
                if (other != boid && other.obj.gameObject.activeSelf)
                {
                    center += other.obj.position;
                    totalAdd++;
                }
            if (totalAdd > 0)
            {
                center /= totalAdd;
                boid.velocity += (center - boid.obj.position) / boidFollowCenterSlowness;
            }
            
            // assigne une vélocité minimale aux boids
            if (boid.velocity.magnitude < targetVelocity)
                boid.velocity *= targetVelocity / (boid.velocity.magnitude+.001f);
            
            boid.obj.position += boid.velocity * Time.deltaTime;
        }
        
        int numAlive = 0;
        foreach (BoidObject boid in boidObjs)
            if (boid.obj.gameObject.activeSelf)
                numAlive++;
        if (numAlive == 0)
        {
            // on fait repoper tout ce ptit monde
            foreach (BoidObject boid in boidObjs)
            {
                boid.obj.gameObject.SetActive(true);
                boid.velocity = Vector3.zero;
                boid.terrainAvoidanceVelocity = Vector3.zero;
                boid.currentChunk = null;
                boid.currentChunkBlockIndex = new int[3];
                boid.currentChunkBlock = null;
                boid.seesPlayer = false;
                boid.lastKnownPlayerPosition = transform.position;
                boid.obj.position = transform.position + Vector3.up * Random.value;
                boid.shootScript.seesPlayer = false;
            }
        }
        
        
        // On déplace le spawner pour qu'il suive à distance respectable le joueur... Le spawner est un boid lui-même
        
        Vector3 target = player.position;
        target.y += 50; // pour éviter le terrain (la flemme de réécrire le code d'évitement...)
        
        // se recentre sur le joueur
        spVelocity += (target - transform.position) / spawnerMoveToPlayerSlowness;
        
        // mais reste loin de lui
        if ((target - transform.position).magnitude < spawnerToPlayerDistance)
            spVelocity += (transform.position - target).normalized * spawnerToPlayerRepulsivity * (spawnerToPlayerDistance - (transform.position - player.position).magnitude);
        
        // on cappe la vitesse maximale de déplacement
        if (spVelocity.sqrMagnitude > spawnerMaxSpeed*spawnerMaxSpeed)
            spVelocity = spVelocity.normalized * spawnerMaxSpeed;
        
        transform.position += spVelocity * Time.deltaTime;
    }
}
