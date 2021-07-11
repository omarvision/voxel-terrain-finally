using UnityEngine;

public class VoxelTerrain : MonoBehaviour
{
    public Vector3Int TerrainSizeInChunks = new Vector3Int(10, 3, 10);
    public Vector3Int ChunkSizeInVoxels = new Vector3Int(10, 10, 10);
    public GameObject prefabChunk = null;
    [Range(0.25f, 0.5f)]
    public float voxelSize = 0.5f;
    public bool sideOptimize = true;
    public bool perlinOn = true;
    [Range(0.1f, 30.0f)]
    public float hillAmount = 1.0f;
    public GameObject Player = null;    
    private Chunk[,,] terrainChunks = null;
    private float maxTerrainX = 0f;
    private float maxTerrainZ = 0f;
    private float maxTerrainY = 0f;

    private void Start()
    {
        maxTerrainX = TerrainSizeInChunks.x * ChunkSizeInVoxels.x;
        maxTerrainZ = TerrainSizeInChunks.z * ChunkSizeInVoxels.z;
        maxTerrainY = TerrainSizeInChunks.y * ChunkSizeInVoxels.y;

        initVoxelTerrain();

        //a base floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = new Vector3((maxTerrainZ / 2) - 0.5f, -2f, (maxTerrainZ / 2) - 0.5f);
        floor.transform.localScale = new Vector3(maxTerrainX, 1f, maxTerrainZ);
    }
    private void OnEnable()
    {
        Chunk.OnChunkRendered += OnChunkRendered;
    }
    private void OnDisable()
    {
        Chunk.OnChunkRendered -= OnChunkRendered;
    }
    private void OnChunkRendered(Vector3Int chunkposition)
    {
        //let's track to see if all chunks are ready
        for (int x = 0; x < TerrainSizeInChunks.x; x++)
        {
            for (int y = 0; y < TerrainSizeInChunks.y; y++)
            {
                for (int z = 0; z < TerrainSizeInChunks.z; z++)
                {
                    if (terrainChunks[x,y,z].isReady == false)
                    {
                        return;
                    }
                }
            }
        }

        afterAllChunksReady();
    }
    private void afterAllChunksReady()
    {
        perlinChunkColumns();
        reRenderChunks();

        //center the player
        if (Player != null)
        {
            Player.transform.position = new Vector3(maxTerrainX / 2, maxTerrainY + 2, maxTerrainZ / 2);
        }
    }
    private void initVoxelTerrain()
    {
        //allocate
        terrainChunks = new Chunk[TerrainSizeInChunks.x, TerrainSizeInChunks.y, TerrainSizeInChunks.z];

        for (int x = 0; x < TerrainSizeInChunks.x; x++)
        {
            for (int z = 0; z < TerrainSizeInChunks.z; z++)
            {
                for (int y = 0; y < TerrainSizeInChunks.y; y++)
                {
                    //new chunk
                    Vector3 chunkPos = new Vector3(x * ChunkSizeInVoxels.x, y * ChunkSizeInVoxels.y, z * ChunkSizeInVoxels.z);
                    GameObject chunkObj = Instantiate(prefabChunk, chunkPos, Quaternion.identity, this.transform);

                    Chunk chunkScript = chunkObj.GetComponent<Chunk>();
                    chunkScript.voxelSize = voxelSize;
                    chunkScript.sideOptimize = sideOptimize;
                                        
                    //add script to collection
                    terrainChunks[x, y, z] = chunkScript;
                }
            }
        }
    }
    private void perlinChunkColumns()
    {
        if (perlinOn == false)
        {
            return;
        }

        Vector3 percent = Vector3.zero;
        int terraincolumnheight = 0;
        Vector3Int currentChunk = Vector3Int.zero;

        Vector3Int totalVoxels = TerrainSizeInChunks * ChunkSizeInVoxels;
        for (int terrainvoxelx = 0; terrainvoxelx < totalVoxels.x; terrainvoxelx++)
        {
            int chunkx = Mathf.RoundToInt(terrainvoxelx / ChunkSizeInVoxels.x);
            int chunkvoxelx = terrainvoxelx - (chunkx * ChunkSizeInVoxels.x);

            for (int terrainvoxelz = 0; terrainvoxelz < totalVoxels.z; terrainvoxelz++)
            {
                int chunkz = Mathf.RoundToInt(terrainvoxelz / ChunkSizeInVoxels.z);
                int chunkvoxelz = terrainvoxelz - (chunkz * ChunkSizeInVoxels.z);

                //terrain column height
                percent.x = (terrainvoxelx / (float)totalVoxels.x) * hillAmount;
                percent.z = (terrainvoxelz / (float)totalVoxels.z) * hillAmount;
                terraincolumnheight = Mathf.RoundToInt(Mathf.PerlinNoise(percent.x, percent.z) * totalVoxels.y);
                //up the chunks
                for (int chunky = 0; chunky < TerrainSizeInChunks.y; chunky++)  
                {
                    Chunk script = terrainChunks[chunkx, chunky, chunkz];

                    //up the voxels in a chunk
                    for (int chunkvoxely = 0; chunkvoxely < ChunkSizeInVoxels.y; chunkvoxely++)  
                    {
                        int terrainvoxely = (chunky * ChunkSizeInVoxels.y) + chunkvoxely;

                        if (terrainvoxely > terraincolumnheight)
                        {
                            // A. above the terrain
                            script.Voxels[chunkvoxelx, chunkvoxely, chunkvoxelz].turnOff();
                        }
                        else if (terrainvoxely == terraincolumnheight)
                        {
                            // B. the terrain line
                            script.Voxels[chunkvoxelx, chunkvoxely, chunkvoxelz].turnOn(Voxel.enumVoxelType.grass);
                        }
                        else
                        {
                            // C. below the terrain
                            script.Voxels[chunkvoxelx, chunkvoxely, chunkvoxelz].turnOn(Voxel.enumVoxelType.dirt);
                        }

                        //if (terrainvoxely == terraincolumnheight)
                        //{
                        //    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //    obj.transform.position = new Vector3(terrainvoxelx, terrainvoxely, terrainvoxelz);
                        //}
                    }
                }
            }
        }        
    }
    private void reRenderChunks()
    {
        for (int x = 0; x < TerrainSizeInChunks.x; x++)
        {
            for (int z = 0; z < TerrainSizeInChunks.z; z++)
            {
                for (int y = 0; y < TerrainSizeInChunks.y; y++)
                {
                    terrainChunks[x, y, z].renderChunk();
                }
            }
        }
    }
}
