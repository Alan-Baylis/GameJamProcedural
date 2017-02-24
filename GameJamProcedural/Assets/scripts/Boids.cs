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
        public BoidObject(GameObject o, Transform parent) {
            obj = Instantiate(o, parent.position, Quaternion.identity).transform;
            velocity = Vector3.zero;
            terrainAvoidanceVelocity = Vector3.zero;
            currentChunkBlockIndex = new int[3];
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
    public float boidTerrainRepulsivity = 3;
    public int numBlockAvoidanceShells = 3;
    
    public float targetVelocity = 1;
    
    void Start () {
        boidObjs = new BoidObject[numBoids];
        for (int i=0; i<numBoids; i++)
            boidObjs[i] = new BoidObject(boidPrefab, player);
    }
    
    void Update () {
        foreach (BoidObject boid in boidObjs)
        {
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
            }
            
            boid.velocity += boid.terrainAvoidanceVelocity;
            
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
