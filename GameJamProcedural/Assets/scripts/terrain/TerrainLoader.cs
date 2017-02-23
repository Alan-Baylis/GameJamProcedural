using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoader : MonoBehaviour {
    public GameObject chunkPrefab;
    public float chunkSize=20;
    public int chunkResolution=20;
    public float maxChunkDistance = 1000; // 1km
    public float maxChunkHeight = 100; // 100m
    
    Chunk[] chunks; // chunks générés ou en cours de génération
    Chunk[] chunksToBuild; // chunks à générer
    Chunk[] chunksToRepurpose; // chunks trop loin à supprimer
    Density density;
    Vector3 camPos;
    int lastChunkIdX;
    int lastChunkIdY;
    int lastChunkIdZ;
    int maxThreads;
    
	void Start () {
        density = new Density(2);
        int numChunksToGenerateXZ = (int) (maxChunkDistance / chunkSize);
        int numChunksToGenerateY = (int) (maxChunkHeight / chunkSize);
        int numChunksToGenerate = numChunksToGenerateXZ*numChunksToGenerateXZ*numChunksToGenerateY;
        Debug.Log("Terrain - préparation de " + numChunksToGenerate + " chunk");
        int maxAllowedChunks = numChunksToGenerateXZ*numChunksToGenerateXZ*numChunksToGenerateY; // faut toujours en mettre BEAUCOUP ici.
        maxAllowedChunks *= 3;
        chunks = new Chunk[maxAllowedChunks];
        chunksToBuild = new Chunk[numChunksToGenerateXZ*numChunksToGenerateXZ*numChunksToGenerateY]; // devrait éviter tout ralentissement
        chunksToRepurpose = new Chunk[numChunksToGenerateXZ*numChunksToGenerateXZ*numChunksToGenerateY];
        
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
        maxThreads = System.Environment.ProcessorCount;
	}
    
    Chunk getChunkFromId(int x, int y, int z)
    {
        foreach (Chunk chunk in chunks)
            if (
                    chunk != null &&
                    chunk.idX == x &&
                    chunk.idY == y &&
                    chunk.idZ == z
                )
                return chunk;
        foreach (Chunk chunk in chunksToBuild)
            if (
                    chunk != null &&
                    chunk.idX == x &&
                    chunk.idY == y &&
                    chunk.idZ == z
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
            if (numChunksToGenerateXZ < 1)
                numChunksToGenerateXZ = 1;
            if (numChunksToGenerateY < 1)
                numChunksToGenerateY = 1;
            
            for (int x=0; x<numChunksToGenerateXZ; x++)
            for (int y=0; y<numChunksToGenerateY; y++)
            for (int z=0; z<numChunksToGenerateXZ; z++)
            {
                // on cherche les chunks à proximité à générer
                int thisChunkIdX = (int) (chunkIdX + x - numChunksToGenerateXZ/2);
                int thisChunkIdY = (int) (chunkIdY + y - numChunksToGenerateY/2);
                int thisChunkIdZ = (int) (chunkIdZ + z - numChunksToGenerateXZ/2);
                if (getChunkFromId(thisChunkIdX, thisChunkIdY, thisChunkIdZ) == null)
                {
                    // chunk non généré -> on génère
                    Vector3 chunkPos = new Vector3(thisChunkIdX * chunkSize, thisChunkIdY * chunkSize, thisChunkIdZ * chunkSize);
                    int id = -1;
                    for (int i=0; i<chunksToBuild.Length; i++)
                        if (chunksToBuild[i] == null)
                        {
                            id = i;
                            break;
                        }
                    if (id != -1)
                    {
                        // emplacement vide dans la liste -> on y ajoute le nouveau chunk
                        int repurposeId = -1;
                        for (int j=0; j<chunksToRepurpose.Length; j++)
                            if (chunksToRepurpose[j] != null)
                            {
                                repurposeId = j;
                                break;
                            }
                        if (repurposeId != -1)
                        {
                            // on a un chunk qu'on peut recycler - on l'utilise
                            // Debug.Log("Recyclage d'un chunk...");
                            Chunk chunk = chunksToRepurpose[repurposeId];
                            chunk.transform.localPosition = chunkPos;
                            chunk.idX = thisChunkIdX; chunk.idY = thisChunkIdY; chunk.idZ = thisChunkIdZ;
                            chunk.PrepareRecycle();
                            chunksToBuild[id] = chunk;
                            chunksToRepurpose[repurposeId] = null;
                        }
                        else
                        {
                            // pas de chunk à recycler - on en instancie un nouveau
                            // Debug.Log("Instantiation d'un nouveau chunk");
                            Chunk chunk = Instantiate(chunkPrefab, transform).GetComponent<Chunk>();
                            chunk.transform.localPosition = chunkPos;
                            chunk.idX = thisChunkIdX; chunk.idY = thisChunkIdY; chunk.idZ = thisChunkIdZ;
                            chunksToBuild[id] = chunk;
                        }
                    }
                    else
                    {
                        // liste déjà pleine -> on trouve le chunk à générer le plus lointain.
                        // Si il est suffisamment loin, on le repositionne au nouvel emplacement
                        id = getFarthestChunkId(chunksToBuild);
                        if (getChunkSqrDistance(chunksToBuild[id]) < (chunkPos+new Vector3(chunkSize*.5f, chunkSize*.5f, chunkSize*.5f)-camPos).sqrMagnitude)
                        {
                            // Debug.Log("Trop de chunks dans la liste de rendu, suppression de chunk distant...");
                            Chunk chunk = chunksToBuild[id];
                            chunk.transform.localPosition = chunkPos;
                            chunk.idX = thisChunkIdX; chunk.idY = thisChunkIdY; chunk.idZ = thisChunkIdZ;
                        }
                        else
                            // le nouveau chunk est plus loin que tous les chunks actuels -> normalement ne devrait pas arriver, dans le doute on l'ignore
                            Debug.Log("Trop de chunks dans la liste de rendu, on ignore les nouveaux chunks trop loin");
                    }
                }
            }
        }
        
        CheckChunksToBuild();
	}
    
    void CheckChunksToBuild()
    {
        int chunksBeingBuilt = 0;
        foreach (Chunk chunk in chunks)
            if (chunk != null && chunk.building && !chunk.generationDone)
                chunksBeingBuilt++;
        
        while (chunksBeingBuilt < maxThreads)
        {
            int closestChunkId = getClosestChunkId(chunksToBuild);
            if (closestChunkId == -1)
            {
                // rien à devoir builder
                break;
            }
            int newId = -1;
            for (int i=0; i<chunks.Length; i++)
                if (chunks[i] == null)
                {
                    newId = i;
                    break;
                }
            if (newId != -1)
            {
                Chunk chunk = chunksToBuild[closestChunkId];
                chunks[newId] = chunk;
                chunksToBuild[closestChunkId] = null;
                chunk.CreateUnityGeometry(
                    chunkSize, chunkSize, chunkSize,
                    chunkResolution, chunkResolution, chunkResolution,
                    chunk.transform.localPosition.x, chunk.transform.localPosition.y, chunk.transform.localPosition.z,
                    density);
                chunksBeingBuilt++;
            }
            else
            {
                // ENCORE pas assez de chunks disponibles dans la liste.
                // on supprime un chunk lointain...
                int id = getFarthestChunkId(chunks);
                Chunk chunk = chunksToBuild[closestChunkId];
                Destroy(chunks[id]);
                chunks[id] = chunk;
                chunksToBuild[closestChunkId] = null;
                chunk.CreateUnityGeometry(
                    chunkSize, chunkSize, chunkSize,
                    chunkResolution, chunkResolution, chunkResolution,
                    chunk.transform.localPosition.x, chunk.transform.localPosition.y, chunk.transform.localPosition.z,
                    density);
                chunksBeingBuilt++;
            }
        }
        
        // on vérifie si il y a des chunks à recycler
        for (int i=0; i<chunks.Length; i++)
        {
            Chunk chunk = chunks[i];
            if (chunk != null && chunk.generationDone && getChunkSqrDistance(chunk) > (maxChunkDistance*maxChunkDistance+maxChunkDistance))
            {
                // le chunk est trop loin - on l'enlève de la liste principale et on le met dans la liste "à recycler"
                int repurposeId = -1;
                for (int j=0; j<chunksToRepurpose.Length; j++)
                    if (chunksToRepurpose[j] == null)
                    {
                        repurposeId = j;
                        break;
                    }
                if (repurposeId != -1)
                {
                    chunksToRepurpose[repurposeId] = chunk;
                    chunks[i] = null;
                }
                else
                {
                    // pas assez de place dans la liste des chunks à recycler ??
                    // Ne devrait jamais arriver. Dans le doute, on le supprime purement et simplement
                    Destroy(chunk);
                }
            }
        }
    }
    
    int getClosestChunkId(Chunk[] chunks)
    {
        int closestChunkId = -1;
        float minDist = -1;
        for (int i=1; i<chunks.Length; i++)
        {
            if (chunks[i] == null)
                continue;
            float dist = getChunkSqrDistance(chunks[i]);
            if (dist < minDist || closestChunkId == -1)
            {
                minDist = dist;
                closestChunkId = i;
            }
        }
        return closestChunkId;
    }
    
    int getFarthestChunkId(Chunk[] chunks)
    {
        int farthestChunkId = -1;
        float maxDist = -1;
        for (int i=1; i<chunks.Length; i++)
        {
            if (chunks[i] == null)
                continue;
            float dist = getChunkSqrDistance(chunks[i]);
            if (dist > maxDist || farthestChunkId == -1)
            {
                maxDist = dist;
                farthestChunkId = i;
            }
        }
        return farthestChunkId;
    }
    
    float getChunkSqrDistance(Chunk chunk)
    {
        return (chunk.transform.localPosition + new Vector3(chunkSize*.5f, chunkSize*.5f, chunkSize*.5f) - camPos).sqrMagnitude;
    }
    
    void AddChunk(Chunk chunk)
    {
        
    }
}
