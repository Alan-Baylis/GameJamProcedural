using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoader : MonoBehaviour {
    public GameObject chunkPrefab;
    public float chunkSize=20;
    public int chunkResolution=20;
    public float maxChunkDistance = 1000; // 1km
    public float maxChunkHeight = 100; // 100m
    
    List<Chunk> chunks;
    List<Chunk> chunksToBuild;
    Density density;
    Vector3 camPos;
    int lastChunkIdX;
    int lastChunkIdY;
    int lastChunkIdZ;
    
	void Start () {
        density = new Density(2);
        chunks = new List<Chunk>();
        chunksToBuild = new List<Chunk>();
        
		camPos = Camera.main.transform.position;
        lastChunkIdX = (int) (camPos.x / chunkSize);
        lastChunkIdY = (int) (camPos.y / chunkSize);
        lastChunkIdZ = (int) (camPos.z / chunkSize);
        if (camPos.x < 0)
            lastChunkIdX -= 1;
        if (camPos.y < 0)
            lastChunkIdY -= 1;
        if (camPos.z < 0)
            lastChunkIdZ -= 1;
        lastChunkIdZ++; // a pour effet de forcer la génération de terrain à se lancer
	}
    
    Chunk getChunkFromId(int x, int y, int z)
    {
        foreach (Chunk chunk in chunks)
            if (
                    (int)(chunk.originX/chunkSize) == x &&
                    (int)(chunk.originY/chunkSize) == y &&
                    (int)(chunk.originZ/chunkSize) == z
                )
                return chunk;
        foreach (Chunk chunk in chunksToBuild)
            if (
                    (int)(chunk.originX/chunkSize) == x &&
                    (int)(chunk.originY/chunkSize) == y &&
                    (int)(chunk.originZ/chunkSize) == z
                )
                return chunk;
        return null;
    }
	
	void Update () {
		camPos = Camera.main.transform.position;
        int chunkIdX = (int) (camPos.x / chunkSize);
        int chunkIdY = (int) (camPos.y / chunkSize);
        int chunkIdZ = (int) (camPos.z / chunkSize);
        if (camPos.x < 0)
            chunkIdX -= 1;
        if (camPos.y < 0)
            chunkIdY -= 1;
        if (camPos.z < 0)
            chunkIdZ -= 1;
        
        if (chunkIdX != lastChunkIdX ||
            chunkIdY != lastChunkIdY ||
            chunkIdZ != lastChunkIdZ)
        {
            Debug.Log("Changement de chunk");
            lastChunkIdX = chunkIdX; lastChunkIdY = chunkIdY; lastChunkIdZ = chunkIdZ;
            int numChunksToGenerateXZ = (int) (maxChunkDistance / chunkSize);
            int numChunksToGenerateY = (int) (maxChunkHeight / chunkSize);
            
            for (int x=0; x<numChunksToGenerateXZ; x++)
            for (int y=0; y<numChunksToGenerateY; y++)
            for (int z=0; z<numChunksToGenerateXZ; z++)
            {
                int thisChunkIdX = (int) (chunkIdX + x - numChunksToGenerateXZ/2);
                int thisChunkIdY = (int) (chunkIdY + y - numChunksToGenerateY/2);
                int thisChunkIdZ = (int) (chunkIdZ + z - numChunksToGenerateXZ/2);
                if (getChunkFromId(thisChunkIdX, thisChunkIdY, thisChunkIdZ) == null)
                {
                    Vector3 chunkPos = new Vector3(thisChunkIdX * chunkSize, thisChunkIdY * chunkSize, thisChunkIdZ * chunkSize);
                    Chunk chunk = Instantiate(chunkPrefab, transform).GetComponent<Chunk>();
                    chunk.transform.localPosition = chunkPos;
                    chunksToBuild.Add(chunk);
                }
            }
        }
        
        CheckChunksToBuild();
	}
    
    void CheckChunksToBuild()
    {
        int chunksBeingBuilt = 0;
        int maxThreads = 8;
        foreach (Chunk chunk in chunks)
            if (chunk.building && !chunk.generationDone)
                chunksBeingBuilt++;
        
        while (chunksBeingBuilt < maxThreads && chunksToBuild.Count > 0)
        {
            chunksToBuild.Sort((a, b) => {
                Vector3 posa = a.transform.localPosition + new Vector3(chunkSize*.5f, chunkSize*.5f, chunkSize*.5f) - camPos;
                Vector3 posb = b.transform.localPosition + new Vector3(chunkSize*.5f, chunkSize*.5f, chunkSize*.5f) - camPos;
                if (posa.sqrMagnitude < posb.sqrMagnitude) // ordre descendant
                    return 1;
                return -1;
            });
            Chunk chunk = chunksToBuild[chunksToBuild.Count-1];
            chunks.Add(chunk);
            chunksToBuild.RemoveAt(chunksToBuild.Count-1);
            chunk.CreateUnityGeometry(
                chunkSize, chunkSize, chunkSize,
                chunkResolution, chunkResolution, chunkResolution,
                chunk.transform.localPosition.x, chunk.transform.localPosition.y, chunk.transform.localPosition.z,
                density);
            chunksBeingBuilt++;
        }
    }
}
