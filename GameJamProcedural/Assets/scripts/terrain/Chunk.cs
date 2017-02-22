using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Chunk : MonoBehaviour {
    
    public class Block
    {
        public float density;
        public int vtxIndexX=-1;
        public int vtxIndexY=-1;
        public int vtxIndexZ=-1;
        public Vector3 direction;
    }
    public class SimpleMesh
    {
        public List<float> vtx;
        public List<float> norms;
        public List<int> faces;
        public SimpleMesh()
        { vtx = new List<float>(); norms = new List<float>(); faces = new List<int>(); }
    }

    float sizeX;
    float sizeY;
    float sizeZ;
    int resolutionX;
    int resolutionY;
    int resolutionZ;
    public float originX;
    public float originY;
    public float originZ;
    Block[] blocks;
    float blockSizeX;
    float blockSizeY;
    float blockSizeZ;
    Density density;
    
    SimpleMesh mesh;
    Mesh unityMesh;
    MeshFilter mf;
    MeshCollider mc;
    
    public bool building = false;
    public bool generationDone = false;
    bool meshCreated = false;
    
    Vector3[] finalVertices;
    // Vector2[] finalUvs;
    Vector3[] finalNormals;
    int[] finalFaces;
    
    Thread generateThread;
    
    void Start()
    {
		mf = GetComponent<MeshFilter>();
		mc = GetComponent<MeshCollider>();
        unityMesh = new Mesh();
    }
    
    void Update()
    {
        if (generationDone && !meshCreated)
        {
            unityMesh.name = "proter-" + originX + "-" + originZ + "-" + originY;
            unityMesh.vertices = finalVertices;
            unityMesh.normals = finalNormals;
            unityMesh.triangles = finalFaces;
            // mesh.uvs = uvs;
            
            mf.mesh = unityMesh;
            mc.sharedMesh = unityMesh;
            meshCreated = true;
            Debug.Log("Chunk " + originX + " " + originY + " " + originZ + " généré (" + finalFaces.Length + " triangles)");
        }
    }
    
    void OnDestroy()
    {
        mf.mesh = null;
        mc.sharedMesh = null;
        DestroyImmediate(unityMesh, true);
    }
    
    void OnApplicationQuit()
    {
        mf.mesh = null;
        mc.sharedMesh = null;
        DestroyImmediate(unityMesh, true);
    }
    
    public void Cleanup()
    {
        mf.mesh = null;
        mc.sharedMesh = null;
        DestroyImmediate(unityMesh, true);
        Destroy(this);
    }

    public void CreateUnityGeometry(float sx, float sy, float sz, int rx, int ry, int rz, float origX, float origY, float origZ, Density d)
    {
        if (building)
            return;
        sizeX = sx;
        sizeY = sy;
        sizeZ = sz;
        resolutionX = rx;
        resolutionY = ry;
        resolutionZ = rz;
        originX = origX;
        originY = origY;
        originZ = origZ;
        density = d;
        blockSizeX = sizeX/resolutionX;
        blockSizeY = sizeY/resolutionY;
        blockSizeZ = sizeZ/resolutionZ;
        blocks = new Block[(resolutionX+1) * (resolutionY+1) * (resolutionZ+1)];
        mesh = new SimpleMesh();
        
        generateThread = new Thread(new ThreadStart(generateAll));
        generateThread.Start();
        building = true;
    }
    
    void generateAll()
    {
        generationDone = false;
        meshCreated = false;
        generateBlocks(density);
        generateGeometry();
        // generateGeomNormals();
        
        finalVertices = new Vector3[mesh.vtx.Count/3];
        finalNormals = new Vector3[mesh.norms.Count/3];
        finalFaces = new int[mesh.faces.Count];
        for (int i=0; i<mesh.vtx.Count/3; i++)
            finalVertices[i] = new Vector3(mesh.vtx[i*3], mesh.vtx[i*3+1], mesh.vtx[i*3+2]);
        for (int i=0; i<mesh.norms.Count/3; i++)
            finalNormals[i] = new Vector3(mesh.norms[i*3], mesh.norms[i*3+1], mesh.norms[i*3+2]);
        for (int i=0; i<mesh.faces.Count; i++)
            finalFaces[i] = mesh.faces[i];
        
        generationDone = true; // on attend la resynchronisation avec le thread principal de Unity pour charger les données en CG
    }
    
    void generateBlocks(Density D)
    {
        for (int x=0; x<resolutionX+1; x++)
            for (int y=0; y<resolutionY+1; y++)
                for (int z=0; z<resolutionZ+1; z++)
                {
                    NoiseSample ns = D.getDensity(originX + x*blockSizeX, originZ + z*blockSizeZ, originY + y*blockSizeY);
                    Block block = getOrCreate(x,y,z);
                    block.density = ns.value;
                    block.direction = new Vector3(-ns.derivative.x, -ns.derivative.z, -ns.derivative.y);
                }
    }
    
    Block getOrCreate(int x, int y, int z)
    {
        Block block = blocks[x + y*(resolutionX+1) + z*(resolutionX+1)*(resolutionY+1)];
        if (block == null)
            block = blocks[x + y*(resolutionX+1) + z*(resolutionX+1)*(resolutionY+1)] = new Block();
        return block;
    }
    
    Block get(int x, int y, int z)
    {
        return blocks[x + y*(resolutionX+1) + z*(resolutionX+1)*(resolutionY+1)];
    }
    
    void generateGeometry()
    {
        // créé les différents vertices associés à chaque bloc
        // (on boucle en sens inverse pour plus de simplicité)
        for (int x=resolutionX;            x>=0; x--) { // on prend un bloc de plus pour garder une transition entre deux chunks
            for (int y=resolutionY;        y>=0; y--) {
                for (int z=resolutionZ;    z>=0; z--) {
                    // blocs de coordonnées xyz
                    Block b000, b001, b010, b011, b100, b101, b110, b111;
                    
                    /* DEBUT DE LA PARTIE CHIANTE */
                    
                    // Vérification: on s'assure qu'on est pas sur une ligne de transition entre deux chunks, auquel cas on ne génère qu'une partie de la géométrie
                    if (x == resolutionX)
                    {
                        if (z == resolutionZ)
                        {
                            // arête
                            
                            if (y == resolutionY)
                                continue; // coin
                            
                            b000 = get(x,y,z);
                            b010 = get(x,y+1,z);
                            if (b000.density * b010.density <= 0)
                            {
                                float vertexY = Mathf.Abs(b000.density / (b010.density-b000.density));
                                mesh.vtx.Add(x*blockSizeX);
                                mesh.vtx.Add((y+vertexY)*blockSizeY);
                                mesh.vtx.Add(z*blockSizeZ);
                                mesh.norms.Add(b000.direction.x);
                                mesh.norms.Add(b000.direction.y);
                                mesh.norms.Add(b000.direction.z);
                                b000.vtxIndexY = mesh.vtx.Count / 3-1;
                            }
                            continue;
                        }
                        
                        if (y == resolutionY)
                        {
                            b000 = get(x,y,z);
                            b001 = get(x,y,z+1);
                            if (b000.density * b001.density <= 0)
                            {
                                float vertexZ = Mathf.Abs(b000.density / (b001.density-b000.density));
                                mesh.vtx.Add(x*blockSizeX);
                                mesh.vtx.Add(y*blockSizeY);
                                mesh.vtx.Add((z+vertexZ)*blockSizeZ);
                                mesh.norms.Add(b000.direction.x);
                                mesh.norms.Add(b000.direction.y);
                                mesh.norms.Add(b000.direction.z);
                                b000.vtxIndexZ = mesh.vtx.Count / 3-1;
                            }
                            continue;
                        }
                        
                        b000 = get(x,y,z);
                        b001 = get(x,y,z+1);
                        b010 = get(x,y+1,z);
                        if (b000.density * b010.density <= 0)
                        {
                            float vertexY = Mathf.Abs(b000.density / (b010.density-b000.density));
                            mesh.vtx.Add(x*blockSizeX);
                            mesh.vtx.Add((y+vertexY)*blockSizeY);
                            mesh.vtx.Add(z*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexY = mesh.vtx.Count / 3-1;
                        }
                        if (b000.density * b001.density <= 0)
                        {
                            float vertexZ = Mathf.Abs(b000.density / (b001.density-b000.density));
                            mesh.vtx.Add(x*blockSizeX);
                            mesh.vtx.Add(y*blockSizeY);
                            mesh.vtx.Add((z+vertexZ)*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexZ = mesh.vtx.Count / 3-1;
                        }
                        continue;
                    }
                    if (y == resolutionY)
                    {
                        if (z == resolutionZ)
                        {
                            b000 = get(x,y,z);
                            b100 = get(x+1,y,z);
                            if (b000.density * b100.density <= 0)
                            {
                                float vertexX = Mathf.Abs(b000.density / (b100.density-b000.density));
                                mesh.vtx.Add((x+vertexX)*blockSizeX);
                                mesh.vtx.Add(y*blockSizeY);
                                mesh.vtx.Add(z*blockSizeZ);
                                mesh.norms.Add(b000.direction.x);
                                mesh.norms.Add(b000.direction.y);
                                mesh.norms.Add(b000.direction.z);
                                b000.vtxIndexX = mesh.vtx.Count / 3-1;
                            }
                            continue; // ces if imbriqués sont chiants
                        }
                        
                        b000 = get(x,y,z);
                        b001 = get(x,y,z+1);
                        b100 = get(x+1,y,z);
                        if (b000.density * b100.density <= 0)
                        {
                            float vertexX = Mathf.Abs(b000.density / (b100.density-b000.density));
                            mesh.vtx.Add((x+vertexX)*blockSizeX);
                            mesh.vtx.Add(y*blockSizeY);
                            mesh.vtx.Add(z*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexX = mesh.vtx.Count / 3-1;
                        }
                        if (b000.density * b001.density <= 0)
                        {
                            float vertexZ = Mathf.Abs(b000.density / (b001.density-b000.density));
                            mesh.vtx.Add(x*blockSizeX);
                            mesh.vtx.Add(y*blockSizeY);
                            mesh.vtx.Add((z+vertexZ)*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexZ = mesh.vtx.Count / 3-1;
                        }
                        continue;
                    }
                    if (z == resolutionZ)
                    {
                        b000 = get(x,y,z);
                        b010 = get(x,y+1,z);
                        b100 = get(x+1,y,z);
                        if (b000.density * b100.density <= 0)
                        {
                            float vertexX = Mathf.Abs(b000.density / (b100.density-b000.density));
                            mesh.vtx.Add((x+vertexX)*blockSizeX);
                            mesh.vtx.Add(y*blockSizeY);
                            mesh.vtx.Add(z*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexX = mesh.vtx.Count / 3-1;
                        }
                        if (b000.density * b010.density <= 0)
                        {
                            float vertexY = Mathf.Abs(b000.density / (b010.density-b000.density));
                            mesh.vtx.Add(x*blockSizeX);
                            mesh.vtx.Add((y+vertexY)*blockSizeY);
                            mesh.vtx.Add(z*blockSizeZ);
                            mesh.norms.Add(b000.direction.x);
                            mesh.norms.Add(b000.direction.y);
                            mesh.norms.Add(b000.direction.z);
                            b000.vtxIndexY = mesh.vtx.Count / 3-1;
                        }
                        continue;
                    }
                    
                    /* FIN TRUCS CHIANTS */
                    
                    
                    // code principal pour le reste du chunk
                    
                    b000 = get(x,     y,      z);
                    b001 = get(x,     y,      z+1);
                    b010 = get(x,     y+1,    z);
                    b011 = get(x,     y+1,    z+1);
                    b100 = get(x+1,   y,      z);
                    b101 = get(x+1,   y,      z+1);
                    b110 = get(x+1,   y+1,    z);
                    b111 = get(x+1,   y+1,    z+1);
                    
                    int code = 0;
                    code |= (b000.density >= 0) ? 0x1   :0;
                    code |= (b001.density >= 0) ? 0x2   :0;
                    code |= (b101.density >= 0) ? 0x4   :0;
                    code |= (b100.density >= 0) ? 0x8   :0;
                    code |= (b010.density >= 0) ? 0x10  :0;
                    code |= (b011.density >= 0) ? 0x20  :0;
                    code |= (b111.density >= 0) ? 0x40  :0;
                    code |= (b110.density >= 0) ? 0x80  :0;
                    
                    
                    if (b000.density * b100.density <= 0)
                    {
                        float vertexX = Mathf.Abs(b000.density / (b100.density-b000.density));
                        mesh.vtx.Add((x+vertexX)*blockSizeX);
                        mesh.vtx.Add(y*blockSizeY);
                        mesh.vtx.Add(z*blockSizeZ);
                        mesh.norms.Add(b000.direction.x);
                        mesh.norms.Add(b000.direction.y);
                        mesh.norms.Add(b000.direction.z);
                        b000.vtxIndexX = mesh.vtx.Count / 3-1;
                    }
                    if (b000.density * b010.density <= 0)
                    {
                        float vertexY = Mathf.Abs(b000.density / (b010.density-b000.density));
                        mesh.vtx.Add(x*blockSizeX);
                        mesh.vtx.Add((y+vertexY)*blockSizeY);
                        mesh.vtx.Add(z*blockSizeZ);
                        mesh.norms.Add(b000.direction.x);
                        mesh.norms.Add(b000.direction.y);
                        mesh.norms.Add(b000.direction.z);
                        b000.vtxIndexY = mesh.vtx.Count / 3-1;
                    }
                    if (b000.density * b001.density <= 0)
                    {
                        float vertexZ = Mathf.Abs(b000.density / (b001.density-b000.density));
                        mesh.vtx.Add(x*blockSizeX);
                        mesh.vtx.Add(y*blockSizeY);
                        mesh.vtx.Add((z+vertexZ)*blockSizeZ);
                        mesh.norms.Add(b000.direction.x);
                        mesh.norms.Add(b000.direction.y);
                        mesh.norms.Add(b000.direction.z);
                        b000.vtxIndexZ = mesh.vtx.Count / 3-1;
                    }
                    // if (b000.density > 0)
                    // {
                        // mesh.vtx.Add(x*blockSizeX);
                        // mesh.vtx.Add(y*blockSizeY);
                        // mesh.vtx.Add(z*blockSizeZ);
                    // }
                        
                    
                    // récupère les arêtes (l'ordre est donné par les tables de GeometryLookupTables)
                    int[] thisBlocksEdges = {
                                                b000.vtxIndexZ,
                                                b001.vtxIndexX,
                                                b100.vtxIndexZ,
                                                b000.vtxIndexX,
                                                
                                                b010.vtxIndexZ,
                                                b011.vtxIndexX,
                                                b110.vtxIndexZ,
                                                b010.vtxIndexX,
                                                
                                                b000.vtxIndexY,
                                                b001.vtxIndexY,
                                                b101.vtxIndexY,
                                                b100.vtxIndexY,
                                              };
                    
                    // on utilise la table de valeurs pour savoir combient de faces on génère
                    int totalfaces = GeometryLookupTables.numFacesPerCode[code];
                    
                    // puis on génère les triangles, si il y en a
                    for (int faceid=0; faceid < totalfaces; faceid++)
                    {
                        // vertex index: [code][face number][vertex id]
                        // code: 256 possibilités, faces: jusqu'à 5 possible (-1 par défaut).
                        mesh.faces.Add(thisBlocksEdges[GeometryLookupTables.edgeIndexPerCode[code *4*5 + faceid*4]] );
                        mesh.faces.Add(thisBlocksEdges[GeometryLookupTables.edgeIndexPerCode[code *4*5 + faceid*4 +2]] ); // ceci permet d'avoir des normales correctes
                        mesh.faces.Add(thisBlocksEdges[GeometryLookupTables.edgeIndexPerCode[code *4*5 + faceid*4 +1]] );
                    }
        }}}
    }

    bool isOfAnyInterest()
    {
        return mesh.faces.Count != 0;
    }
    
    float fisqrt(float number)
    {
        // Fast Inverse Square Root de Quake
        float xhalf = 0.5f*number;
        int i = System.BitConverter.ToInt32(System.BitConverter.GetBytes(number), 0);
        i = 0x5f3759df - (i>>1); // what the fck ?
        number = System.BitConverter.ToSingle(System.BitConverter.GetBytes(i), 0);
        number = number*(1.5f - xhalf*number*number);
        return number;
    }
    
    void generateGeomNormalsDONTUSE()
    {
        foreach (float vtx in mesh.vtx)
            mesh.norms.Add(0f);
        
        for (int i=0; i<mesh.faces.Count / 3; i++)
        {
            int vtxindex1 = mesh.faces[i*3];
            int vtxindex2 = mesh.faces[i*3+1];
            int vtxindex3 = mesh.faces[i*3+2];
            // calcul du vecteur de v1 à v2 et de v2 à v3 - puis produit vectoriel pour trouver la normale
            float[] edge1 = {
                                mesh.vtx[ 3*vtxindex2 ]   -   mesh.vtx[ 3*vtxindex1 ],
                                mesh.vtx[ 3*vtxindex2+1 ] -   mesh.vtx[ 3*vtxindex1+1 ],
                                mesh.vtx[ 3*vtxindex2+2 ] -   mesh.vtx[ 3*vtxindex1+2 ],
                            };
            float[] edge2 = {
                                mesh.vtx[ 3*vtxindex3 ]   -   mesh.vtx[ 3*vtxindex2 ],
                                mesh.vtx[ 3*vtxindex3+1 ] -   mesh.vtx[ 3*vtxindex2+1 ],
                                mesh.vtx[ 3*vtxindex3+2 ] -   mesh.vtx[ 3*vtxindex2+2 ],
                            };
            
            float[] facenorm = {
                edge1[1]*edge2[2] - edge1[2]*edge2[1],
                edge1[2]*edge2[0] - edge1[0]*edge2[2],
                edge1[0]*edge2[1] - edge1[1]*edge2[0],
            };
            float length = 1f/fisqrt(facenorm[0]*facenorm[0] + facenorm[1]*facenorm[1] + facenorm[2]*facenorm[2]);
            facenorm[0] /= length;
            facenorm[1] /= length;
            facenorm[2] /= length;
            
            mesh.norms[3*vtxindex1] = mesh.norms[3*vtxindex1] + facenorm[0];
            mesh.norms[3*vtxindex1+1] = mesh.norms[3*vtxindex1+1] + facenorm[1];
            mesh.norms[3*vtxindex1+2] = mesh.norms[3*vtxindex1+2] + facenorm[2];
            
            mesh.norms[3*vtxindex2] = mesh.norms[3*vtxindex2] + facenorm[0];
            mesh.norms[3*vtxindex2+1] = mesh.norms[3*vtxindex2+1] + facenorm[1];
            mesh.norms[3*vtxindex2+2] = mesh.norms[3*vtxindex2+2] + facenorm[2];
            
            mesh.norms[3*vtxindex3] = mesh.norms[3*vtxindex3] + facenorm[0];
            mesh.norms[3*vtxindex3+1] = mesh.norms[3*vtxindex3+1] + facenorm[1];
            mesh.norms[3*vtxindex3+2] = mesh.norms[3*vtxindex3+2] + facenorm[2];
        }
        
        
        // La normale d'un vertex est juste la moyenne des normales des faces qui lui sont connectées.
        for (int i=0; i<mesh.norms.Count/3; i++)
        {
            float nx = mesh.norms[i*3]; float ny = mesh.norms[i*3+1]; float nz = mesh.norms[i*3+2];
            float length = 1/fisqrt(nx*nx + ny*ny + nz*nz);
            mesh.norms[i*3] = nx/length; mesh.norms[i*3+1] = ny/length; mesh.norms[i*3+2] = nz/length;
        }
    }
    
    void OnDrawGizmosSelected () {
        Transform transform = this.transform;
        for (int x=0; x<resolutionX+1; x++)
            for (int y=0; y<resolutionY+1; y++)
                for (int z=0; z<resolutionZ+1; z++)
                {
                    Block b = get(x,y,z);
                    Gizmos.DrawLine(transform.TransformPoint(new Vector3(x,y,z)*sizeX/resolutionX), transform.TransformPoint(new Vector3(x,y,z)*sizeX/resolutionX + b.direction*.2f));
                    Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(x,y,z)*sizeX/resolutionX), .1f);
                }
    }
}
