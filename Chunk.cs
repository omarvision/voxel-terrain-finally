using UnityEngine;
using UnityEditor; //for assetdatabase

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    #region --- helpers ---
    public delegate void ChunkRendered(Vector3Int position);
    public static event ChunkRendered OnChunkRendered;
    #endregion

    public Vector3Int Size = new Vector3Int(10, 10, 10);
    public Voxel.MeshData data = new Voxel.MeshData();
    [Range(0.25f, 0.5f)]
    public float voxelSize = 0.5f;
    public bool sideOptimize = true;
    public Voxel[,,] Voxels = null;
    private Mesh mesh = null;
    private MeshCollider mc = null;
    [HideInInspector]
    public bool isReady = false;
    
    private void Start()
    {
        mc = this.GetComponent<MeshCollider>();
        initChunk();
        renderChunk();

        //SaveChunkAsMeshAsset("chunkmesh.asset");
    }
    private void OnEnable()
    {
        // sanity check - position is integer (no decimal coord), for voxel methods to work
        Vector3 roundp = quantizePoint(this.transform.position);
        if (roundp.Equals(this.transform.position) == false)
        {
            Debug.Log("ERROR: a chunk must be placed on an integer position (no decimal)");
        }

        // sanity check - is the tag "Chunk", for add remove voxels to work
        if (this.CompareTag("Chunk") == false)
        {
            Debug.Log("ERROR: object must be tagged 'Chunk' in order for add remove voxel methods to work");
        }
    }
    public void initChunk()
    {
        //Note: Unity mesh has 64K vertices, 64K uv, 64K triangles limit (not including submeshes) as of 2019
        if (Voxels != null)
        {
            return;
        }

        //allocate for mesh
        int numvertices = Size.x * Size.y * Size.z * 24;
        int numuv = Size.x * Size.y * Size.z * 24;
        int numtriangles = Size.x * Size.y * Size.z * 36;
        data.vertices = new Vector3[numvertices];
        data.uv = new Vector2[numuv];
        data.triangles = new int[numtriangles];

        //allocate for voxel
        Voxels = new Voxel[Size.x, Size.y, Size.z];

        //new voxels
        int startvertices = 0;
        int startuv = 0;
        int starttriangles = 0;
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                for (int z = 0; z < Size.z; z++)
                {
                    Voxel.MeshData d = new Voxel.MeshData();

                    d.startvertices = startvertices;
                    d.startuv = startuv;
                    d.starttriangles = starttriangles;
                    d.offset = new Vector3(x, y, z);                 
                    d.vertices = data.vertices;
                    d.uv = data.uv;
                    d.triangles = data.triangles;

                    Voxel voxelobject = new Voxel(ref d, Voxel.enumVoxelType.grass, voxelSize);
                    Voxels[x, y, z] = voxelobject;

                    startvertices += 24;    // 6 sides * 4 points = 24 indices per voxel
                    startuv += 24;          // uv array count must match vertices array count = 24 indices per voxel
                    starttriangles += 36;   // 6 sides * 2 triangles = 12 triangles * 3 points = 36 indices per voxel
                }
            }
        }
    }
    public void renderChunk()
    {
        //mesh reference
        if (mesh == null)
        {
            getMesh();
        }
        
        if (sideOptimize == true)
        {
            sidesOffOptimization();
        }
        
        //data to mesh
        mesh.vertices = data.vertices;
        mesh.uv = data.uv;
        mesh.triangles = data.triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //the shape of the meshcollider will be rebuilt
        mc.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
        mc.sharedMesh = mesh;

        //fire event (check to see if there are subscribers to the event first, to prevent runtime error)       
        if (isReady == false)
        {
            isReady = true;

            Vector3Int position = new Vector3Int(
                Mathf.RoundToInt(this.transform.position.x)
                , Mathf.RoundToInt(this.transform.position.y)
                , Mathf.RoundToInt(this.transform.position.z));

            OnChunkRendered?.Invoke(position);
        }        
    }
    private void getMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "chunk";
            this.GetComponent<MeshFilter>().mesh = mesh;
            this.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }    
    public void removeVoxel(RaycastHit hit)
    {
        Vector3 forwardfrompoint = hit.point + (hit.normal * -0.1f);
        Vector3 p = quantizePoint(forwardfrompoint);
        p = p - this.transform.position;
        try
        {
            Voxels[(int)p.x, (int)p.y, (int)p.z].turnOff();
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("Chunk.removeVoxel [coord]={0} EXCEPTION:{1}", p, ex.Message));
        }

        renderChunk();
    }    
    public void addVoxel(RaycastHit hit, Voxel.enumVoxelType voxeltype)
    {
        Vector3 backfrompoint = hit.point + (hit.normal * 0.9f);
        Vector3 p = backfrompoint - this.transform.position;
        p.x = (float)System.Math.Round(p.x, System.MidpointRounding.AwayFromZero);
        p.y = (float)System.Math.Round(p.y, System.MidpointRounding.AwayFromZero);
        p.z = (float)System.Math.Round(p.z, System.MidpointRounding.AwayFromZero);
        try
        {
            Voxels[(int)p.x, (int)p.y, (int)p.z].turnOn(voxeltype);
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("Chunk.addVoxel [coord]={0} [voxelType]={1} EXCEPTION:{2}", p, voxeltype, ex.Message));
        }        
        
        renderChunk();
    }
    public void setVoxelType(int x, int y, int z, Voxel.enumVoxelType voxeltype)
    {
        Voxels[x, y, z].turnOn(voxeltype);
    }
    private void sidesOffOptimization()
    {
        for (int x = 0; x < Voxels.GetLength(0); x++)
        {
            for (int y = 0; y < Voxels.GetLength(1); y++)
            {
                for (int z = 0; z < Voxels.GetLength(2); z++)
                {
                    Voxel v = Voxels[x, y, z];
                    if (v.on == 0)
                    {
                        continue;
                    }

                    v.setTriangleSide(Voxel.enumSide.EFAB_back, currentSideOn(Voxel.enumSide.EFAB_back, x, y, z));
                    v.setTriangleSide(Voxel.enumSide.HGDC_forward, currentSideOn(Voxel.enumSide.HGDC_forward, x, y, z));
                    v.setTriangleSide(Voxel.enumSide.FHBD_right, currentSideOn(Voxel.enumSide.FHBD_right, x, y, z));
                    v.setTriangleSide(Voxel.enumSide.GECA_left, currentSideOn(Voxel.enumSide.GECA_left, x, y, z));
                    v.setTriangleSide(Voxel.enumSide.EFGH_up, currentSideOn(Voxel.enumSide.EFGH_up, x, y, z));
                    v.setTriangleSide(Voxel.enumSide.ABCD_down, currentSideOn(Voxel.enumSide.ABCD_down, x, y, z));
                }
            }
        }
    }
    private int currentSideOn(Voxel.enumSide neighborside, int x, int y, int z)
    {
        int neighborOn = 1;

        //voxel neighbor voxel side
        switch (neighborside)
        {
            case Voxel.enumSide.EFAB_back:
                neighborOn = (z > 0) ? Voxels[x, y, z - 1].on : 0;                
                break;
            case Voxel.enumSide.FHBD_right:
                neighborOn = (x < Voxels.GetLength(0) - 1) ? Voxels[x + 1, y, z].on : 0;
                break;
            case Voxel.enumSide.HGDC_forward:
                neighborOn = (z < Voxels.GetLength(2) - 1) ? Voxels[x, y, z + 1].on : 0;                
                break;
            case Voxel.enumSide.GECA_left:
                neighborOn = (x > 0) ? Voxels[x - 1, y, z].on : 0;                
                break;
            case Voxel.enumSide.EFGH_up:
                neighborOn = (y < Voxels.GetLength(1) - 1) ? Voxels[x, y + 1, z].on : 0;
                break;
            case Voxel.enumSide.ABCD_down:
                neighborOn = (y > 0) ? Voxels[x, y - 1, z].on : 0;                
                break;
        }

        //switch (neighborside)
        //{
        //    case Voxel.enumSide.EFAB_back:
        //        if (z == 0) neighborOn = 1;
        //        break;
        //    case Voxel.enumSide.FHBD_right:
        //        if (x == Voxels.GetUpperBound(0)) neighborOn = 1;
        //        break;
        //    case Voxel.enumSide.HGDC_forward:
        //        if (z == Voxels.GetUpperBound(2)) neighborOn = 1;
        //        break;
        //    case Voxel.enumSide.GECA_left:
        //        if (x == 0) neighborOn = 1;
        //        break;
        //    case Voxel.enumSide.EFGH_up:
        //        break;
        //    case Voxel.enumSide.ABCD_down:
        //        if (y == 0) neighborOn = 1;
        //        break;
        //}

        if (neighborOn == 0)
            return 1;
        else
            return 0;
    }
    public Vector3 quantizePoint(Vector3 p)
    {
        p.x = (float)System.Math.Round(p.x, System.MidpointRounding.AwayFromZero);
        p.y = (float)System.Math.Round(p.y, System.MidpointRounding.AwayFromZero);
        p.z = (float)System.Math.Round(p.z, System.MidpointRounding.AwayFromZero);
        return p;
    }
    public void SaveChunkAsMeshAsset(string name)
    {
        //note: https://answers.unity.com/questions/39311/editor-scripting-how-to-save-a-script-generated-me.html

        //save the chunk as an asset. the mesh asset can be used on a MeshFilter mesh in the Unity editor.
        AssetDatabase.CreateAsset(this.GetComponent<MeshFilter>().mesh, "Assets/" + name);


    }
}
