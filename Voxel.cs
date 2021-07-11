using UnityEngine;

public class Voxel 
{
    #region --- helper ---
    private enum enumCorner
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
    }
    public enum enumSide
    {
        EFAB_back,
        FHBD_right,
        HGDC_forward,
        GECA_left,
        EFGH_up,
        ABCD_down,
    }    
    private enum enumTexture
    {
        dirt,
        grassdirt,
        grass,
        water,

        leaf,
        stone,
        coal,
        bark,

        treecut,
        wood,
        brick,
        lava,

        sand,
        mud,
        bark2,
        wood2,
    }
    public enum enumVoxelType
    {
        dirt,   //just dirt
        grass,  //grass on top, dirt on sides and bottom
        water,  //just water
        leaf,   //just leaf
        tree1,  //cut on top bottom, bark on sides
        tree2,  //cut on top bottom, bark2 on sides
        stone,  //just stone
        coal,   //just coal
        wood1,  //just wood1
        wood2,  //just wood2
        brick,  //just brick
        lava,   //just lava
        sand,   //just sand
        mud,    //just mud
    }
    public enum enumSideBit
    {
        back = 1,       // 100000000
        right = 2,      // 010000000
        forward = 4,    // 
        left = 8,
        up = 16,
        down = 32,
        ALL = 63,
    }
    public struct MeshData
    {
        public int startvertices;   //used by voxel
        public int startuv;
        public int starttriangles;
        public Vector3 offset;
        public int sidesAlwaysOff;  //sides to never draw because of location in chunk at edges

        public Vector3[] vertices;  //used by chunk and voxel
        public Vector2[] uv;
        public int[] triangles;
    }
    public class TextureMap
    {
        public Vector2 layout = new Vector2(4, 4);
        public Vector2 pixels = new Vector2(128, 128);
        public float width = 4 * 128;
        public float height = 4 * 128;
        [HideInInspector]
        public Vector2 coord = new Vector2(0, 0);
        [HideInInspector]
        public Vector2 LO = new Vector2(0, 0);
        [HideInInspector]
        public Vector2 HI = new Vector2(1, 1);
    }
    #endregion
    
    public MeshData data = new MeshData();  // reference to chunk data, w/offsets for voxel
    public int on = 1;
    public enumVoxelType voxeltype = enumVoxelType.dirt;
    public float voxelSize = 0.5f;
    public TextureMap map = new TextureMap();
    private int A, B, C, D, E, F, G, H;
    private float LO;
    private float HI;
    
    public Voxel(ref MeshData _data, enumVoxelType _voxeltype, float _voxelSize = 0.5f)
    {
        data = _data;
        voxeltype = _voxeltype;
        voxelSize = _voxelSize;
        LO = -voxelSize;
        HI = voxelSize;
        
        setVerticesSide(enumSide.EFAB_back);
        setVerticesSide(enumSide.FHBD_right);
        setVerticesSide(enumSide.HGDC_forward);
        setVerticesSide(enumSide.GECA_left);
        setVerticesSide(enumSide.EFGH_up);
        setVerticesSide(enumSide.ABCD_down);
                
        setUVVoxelType(voxeltype);

        setTriangleSide(enumSide.EFAB_back);
        setTriangleSide(enumSide.FHBD_right);
        setTriangleSide(enumSide.HGDC_forward);
        setTriangleSide(enumSide.GECA_left);
        setTriangleSide(enumSide.EFGH_up);
        setTriangleSide(enumSide.ABCD_down);
    }
    private Vector3 Corner(enumCorner code)
    {
        /*        
                   G           H 
               
               E           F 
                                            
                                           
                   C           D            
                                            
               A           B                

       */
       
        switch (code)
        {
            case enumCorner.A:
                return data.offset + new Vector3(LO, LO, LO);     //  A   -  -  -
            case enumCorner.B:
                return data.offset + new Vector3(HI, LO, LO);     //  B   +  -  -
            case enumCorner.C:
                return data.offset + new Vector3(LO, LO, HI);     //  C   -  -  +
            case enumCorner.D:
                return data.offset + new Vector3(HI, LO, HI);     //  D   +  -  +
            case enumCorner.E:
                return data.offset + new Vector3(LO, HI, LO);     //  E   -  +  -
            case enumCorner.F:
                return data.offset + new Vector3(HI, HI, LO);     //  F   +  +  -
            case enumCorner.G:
                return data.offset + new Vector3(LO, HI, HI);     //  G   -  +  +
            case enumCorner.H:
                return data.offset + new Vector3(HI, HI, HI);     //  H   +  +  +
            default:
                Debug.Log("Corner: Error [enumCorner]=" + code.ToString());
                return Vector3.zero;
        }
    }
    private void setVerticesSide(enumSide side)
    {
        int i;

        try
        {
            i = data.startvertices + (int)side * 4;

            switch (side)
            {
                case enumSide.EFAB_back:
                    data.vertices[i++] = Corner(enumCorner.E);
                    data.vertices[i++] = Corner(enumCorner.F);
                    data.vertices[i++] = Corner(enumCorner.A);
                    data.vertices[i++] = Corner(enumCorner.B);
                    break;
                case enumSide.FHBD_right:
                    data.vertices[i++] = Corner(enumCorner.F);
                    data.vertices[i++] = Corner(enumCorner.H);
                    data.vertices[i++] = Corner(enumCorner.B);
                    data.vertices[i++] = Corner(enumCorner.D);
                    break;
                case enumSide.HGDC_forward:
                    data.vertices[i++] = Corner(enumCorner.H);
                    data.vertices[i++] = Corner(enumCorner.G);
                    data.vertices[i++] = Corner(enumCorner.D);
                    data.vertices[i++] = Corner(enumCorner.C);
                    break;
                case enumSide.GECA_left:
                    data.vertices[i++] = Corner(enumCorner.G);
                    data.vertices[i++] = Corner(enumCorner.E);
                    data.vertices[i++] = Corner(enumCorner.C);
                    data.vertices[i++] = Corner(enumCorner.A);
                    break;
                case enumSide.EFGH_up:
                    data.vertices[i++] = Corner(enumCorner.E);
                    data.vertices[i++] = Corner(enumCorner.F);
                    data.vertices[i++] = Corner(enumCorner.G);
                    data.vertices[i++] = Corner(enumCorner.H);
                    break;
                case enumSide.ABCD_down:
                    data.vertices[i++] = Corner(enumCorner.A);
                    data.vertices[i++] = Corner(enumCorner.B);
                    data.vertices[i++] = Corner(enumCorner.C);
                    data.vertices[i++] = Corner(enumCorner.D);
                    break;
                default:
                    Debug.Log("setVerticesSide: Error [enumSide]=" + side.ToString());
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }        
    }
    private void setUVSide(enumSide side, enumTexture texture)
    {
        // 1. get the LO, HI coords of the texture within the material
        switch (texture)
        {
            // row 3
            case enumTexture.dirt:
                map.LO.x = map.pixels.x * 0;
                map.LO.y = map.pixels.y * 3;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.grassdirt:
                map.LO.x = map.pixels.x * 1;
                map.LO.y = map.pixels.y * 3;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.grass:
                map.LO.x = map.pixels.x * 2;
                map.LO.y = map.pixels.y * 3;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.water:
                map.LO.x = map.pixels.x * 3;
                map.LO.y = map.pixels.y * 3;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            
            // row 2
            case enumTexture.leaf:
                map.LO.x = map.pixels.x * 0;
                map.LO.y = map.pixels.y * 2;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.stone:
                map.LO.x = map.pixels.x * 1;
                map.LO.y = map.pixels.y * 2;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.coal:
                map.LO.x = map.pixels.x * 2;
                map.LO.y = map.pixels.y * 2;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.bark:
                map.LO.x = map.pixels.x * 3;
                map.LO.y = map.pixels.y * 2;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            
            // row 1
            case enumTexture.treecut:
                map.LO.x = map.pixels.x * 0;
                map.LO.y = map.pixels.y * 1;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.wood:
                map.LO.x = map.pixels.x * 1;
                map.LO.y = map.pixels.y * 1;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.brick:
                map.LO.x = map.pixels.x * 2;
                map.LO.y = map.pixels.y * 1;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.lava:
                map.LO.x = map.pixels.x * 3;
                map.LO.y = map.pixels.y * 1;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;

            // row 0
            case enumTexture.sand:
                map.LO.x = map.pixels.x * 0;
                map.LO.y = map.pixels.y * 0;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.mud:
                map.LO.x = map.pixels.x * 1;
                map.LO.y = map.pixels.y * 0;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.bark2:
                map.LO.x = map.pixels.x * 2;
                map.LO.y = map.pixels.y * 0;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
            case enumTexture.wood2:
                map.LO.x = map.pixels.x * 3;
                map.LO.y = map.pixels.y * 0;
                map.HI.x = map.LO.x + map.pixels.x;
                map.HI.y = map.LO.y + map.pixels.y;
                break;
        }

        map.LO = new Vector2(map.LO.x / map.width, map.LO.y / map.height);
        map.HI = new Vector2(map.HI.x / map.width, map.HI.y / map.height);
        
        // 2. what start index in the mesh UV array
        int i = data.startuv + (int)side * 4;

        // 3. plot the UV
        switch (side)
        {
            case enumSide.EFAB_back:
            case enumSide.FHBD_right:
            case enumSide.HGDC_forward:
            case enumSide.GECA_left:
                data.uv[i++] = new Vector2(map.LO.x, map.HI.y);  //  -   +
                data.uv[i++] = new Vector2(map.HI.x, map.HI.y);  //  +   +
                data.uv[i++] = new Vector2(map.LO.x, map.LO.y);  //  -   -
                data.uv[i++] = new Vector2(map.HI.x, map.LO.y);  //  +   -
                break;
            case enumSide.EFGH_up:
                data.uv[i++] = new Vector2(map.LO.x, map.LO.y);  //  -   -
                data.uv[i++] = new Vector2(map.HI.x, map.LO.y);  //  +   -
                data.uv[i++] = new Vector2(map.LO.x, map.HI.y);  //  -   +
                data.uv[i++] = new Vector2(map.HI.x, map.HI.y);  //  +   +
                break;
            case enumSide.ABCD_down:
                data.uv[i++] = new Vector2(map.LO.x, map.HI.y);  //  -   +
                data.uv[i++] = new Vector2(map.HI.x, map.HI.y);  //  +   +
                data.uv[i++] = new Vector2(map.LO.x, map.LO.y);  //  -   -
                data.uv[i++] = new Vector2(map.HI.x, map.LO.y);  //  +   -
                break;
            default:
                Debug.Log("setUVSide: Error [enumSide]=" + side.ToString() + ", [enumTexture]=" + texture.ToString());
                break;
        }
    }
    private void setUVVoxelType(enumVoxelType voxeltype)
    {
        switch (voxeltype)
        {
            case enumVoxelType.dirt:
                setUVSide(enumSide.EFAB_back, enumTexture.dirt);
                setUVSide(enumSide.FHBD_right, enumTexture.dirt);
                setUVSide(enumSide.HGDC_forward, enumTexture.dirt);
                setUVSide(enumSide.GECA_left, enumTexture.dirt);
                setUVSide(enumSide.EFGH_up, enumTexture.dirt);
                setUVSide(enumSide.ABCD_down, enumTexture.dirt);
                break;
            case enumVoxelType.grass:
                setUVSide(enumSide.EFAB_back, enumTexture.grassdirt);
                setUVSide(enumSide.FHBD_right, enumTexture.grassdirt);
                setUVSide(enumSide.HGDC_forward, enumTexture.grassdirt);
                setUVSide(enumSide.GECA_left, enumTexture.grassdirt);
                setUVSide(enumSide.EFGH_up, enumTexture.grass);
                setUVSide(enumSide.ABCD_down, enumTexture.dirt);
                break;
            case enumVoxelType.water:
                setUVSide(enumSide.EFAB_back, enumTexture.water);
                setUVSide(enumSide.FHBD_right, enumTexture.water);
                setUVSide(enumSide.HGDC_forward, enumTexture.water);
                setUVSide(enumSide.GECA_left, enumTexture.water);
                setUVSide(enumSide.EFGH_up, enumTexture.water);
                setUVSide(enumSide.ABCD_down, enumTexture.water);
                break;
            case enumVoxelType.leaf:
                setUVSide(enumSide.EFAB_back, enumTexture.leaf);
                setUVSide(enumSide.FHBD_right, enumTexture.leaf);
                setUVSide(enumSide.HGDC_forward, enumTexture.leaf);
                setUVSide(enumSide.GECA_left, enumTexture.leaf);
                setUVSide(enumSide.EFGH_up, enumTexture.leaf);
                setUVSide(enumSide.ABCD_down, enumTexture.leaf);
                break;
            case enumVoxelType.tree1:
                setUVSide(enumSide.EFAB_back, enumTexture.bark);
                setUVSide(enumSide.FHBD_right, enumTexture.bark);
                setUVSide(enumSide.HGDC_forward, enumTexture.bark);
                setUVSide(enumSide.GECA_left, enumTexture.bark);
                setUVSide(enumSide.EFGH_up, enumTexture.treecut);
                setUVSide(enumSide.ABCD_down, enumTexture.treecut);
                break;
            case enumVoxelType.tree2:
                setUVSide(enumSide.EFAB_back, enumTexture.bark2);
                setUVSide(enumSide.FHBD_right, enumTexture.bark2);
                setUVSide(enumSide.HGDC_forward, enumTexture.bark2);
                setUVSide(enumSide.GECA_left, enumTexture.bark2);
                setUVSide(enumSide.EFGH_up, enumTexture.treecut);
                setUVSide(enumSide.ABCD_down, enumTexture.treecut);
                break;
            case enumVoxelType.stone:
                setUVSide(enumSide.EFAB_back, enumTexture.stone);
                setUVSide(enumSide.FHBD_right, enumTexture.stone);
                setUVSide(enumSide.HGDC_forward, enumTexture.stone);
                setUVSide(enumSide.GECA_left, enumTexture.stone);
                setUVSide(enumSide.EFGH_up, enumTexture.stone);
                setUVSide(enumSide.ABCD_down, enumTexture.stone);
                break;
            case enumVoxelType.coal:
                setUVSide(enumSide.EFAB_back, enumTexture.coal);
                setUVSide(enumSide.FHBD_right, enumTexture.coal);
                setUVSide(enumSide.HGDC_forward, enumTexture.coal);
                setUVSide(enumSide.GECA_left, enumTexture.coal);
                setUVSide(enumSide.EFGH_up, enumTexture.coal);
                setUVSide(enumSide.ABCD_down, enumTexture.coal);
                break;
            case enumVoxelType.wood1:
                setUVSide(enumSide.EFAB_back, enumTexture.wood);
                setUVSide(enumSide.FHBD_right, enumTexture.wood);
                setUVSide(enumSide.HGDC_forward, enumTexture.wood);
                setUVSide(enumSide.GECA_left, enumTexture.wood);
                setUVSide(enumSide.EFGH_up, enumTexture.wood);
                setUVSide(enumSide.ABCD_down, enumTexture.wood);
                break;
            case enumVoxelType.wood2:
                setUVSide(enumSide.EFAB_back, enumTexture.wood2);
                setUVSide(enumSide.FHBD_right, enumTexture.wood2);
                setUVSide(enumSide.HGDC_forward, enumTexture.wood2);
                setUVSide(enumSide.GECA_left, enumTexture.wood2);
                setUVSide(enumSide.EFGH_up, enumTexture.wood2);
                setUVSide(enumSide.ABCD_down, enumTexture.wood2);
                break;
            case enumVoxelType.brick:
                setUVSide(enumSide.EFAB_back, enumTexture.brick);
                setUVSide(enumSide.FHBD_right, enumTexture.brick);
                setUVSide(enumSide.HGDC_forward, enumTexture.brick);
                setUVSide(enumSide.GECA_left, enumTexture.brick);
                setUVSide(enumSide.EFGH_up, enumTexture.brick);
                setUVSide(enumSide.ABCD_down, enumTexture.brick);
                break;
            case enumVoxelType.lava:
                setUVSide(enumSide.EFAB_back, enumTexture.lava);
                setUVSide(enumSide.FHBD_right, enumTexture.lava);
                setUVSide(enumSide.HGDC_forward, enumTexture.lava);
                setUVSide(enumSide.GECA_left, enumTexture.lava);
                setUVSide(enumSide.EFGH_up, enumTexture.lava);
                setUVSide(enumSide.ABCD_down, enumTexture.lava);
                break;
            case enumVoxelType.sand:
                setUVSide(enumSide.EFAB_back, enumTexture.sand);
                setUVSide(enumSide.FHBD_right, enumTexture.sand);
                setUVSide(enumSide.HGDC_forward, enumTexture.sand);
                setUVSide(enumSide.GECA_left, enumTexture.sand);
                setUVSide(enumSide.EFGH_up, enumTexture.sand);
                setUVSide(enumSide.ABCD_down, enumTexture.sand);
                break;
            case enumVoxelType.mud:
                setUVSide(enumSide.EFAB_back, enumTexture.mud);
                setUVSide(enumSide.FHBD_right, enumTexture.mud);
                setUVSide(enumSide.HGDC_forward, enumTexture.mud);
                setUVSide(enumSide.GECA_left, enumTexture.mud);
                setUVSide(enumSide.EFGH_up, enumTexture.mud);
                setUVSide(enumSide.ABCD_down, enumTexture.mud);
                break;
        }
    }
    public void setTriangleSide(enumSide side, int on = 1)
    {
        int i = data.starttriangles + (int)side * 6;
        
        switch (side)
        {
            case enumSide.EFAB_back:
                if ((data.sidesAlwaysOff & (int)enumSideBit.back) != 0)
                {
                    on = 0;
                }
                E = (on == 1)?(data.startvertices + 0):0; //we will use the vertex index if on, else 0
                F = (on == 1)?(data.startvertices + 1):0;
                A = (on == 1)?(data.startvertices + 2):0;
                B = (on == 1)?(data.startvertices + 3):0;
                data.triangles[i++] = A;
                data.triangles[i++] = E;
                data.triangles[i++] = F;
                data.triangles[i++] = B;
                data.triangles[i++] = A;
                data.triangles[i++] = F;
                break;
            case enumSide.FHBD_right:
                if ((data.sidesAlwaysOff & (int)enumSideBit.right) != 0)
                {
                    on = 0;
                }
                F = (on == 1) ?(data.startvertices + 4):0;
                H = (on == 1) ?(data.startvertices + 5):0;
                B = (on == 1) ?(data.startvertices + 6):0;
                D = (on == 1) ?(data.startvertices + 7):0;
                data.triangles[i++] = B;
                data.triangles[i++] = F;
                data.triangles[i++] = H;
                data.triangles[i++] = D;
                data.triangles[i++] = B;
                data.triangles[i++] = H;
                break;
            case enumSide.HGDC_forward:
                if ((data.sidesAlwaysOff & (int)enumSideBit.forward) != 0)
                {
                    on = 0;
                }
                H = (on == 1) ?(data.startvertices + 8):0;
                G = (on == 1) ?(data.startvertices + 9):0;
                D = (on == 1) ?(data.startvertices + 10):0;
                C = (on == 1) ?(data.startvertices + 11):0;
                data.triangles[i++] = D;
                data.triangles[i++] = H;
                data.triangles[i++] = G;
                data.triangles[i++] = C;
                data.triangles[i++] = D;
                data.triangles[i++] = G;
                break;
            case enumSide.GECA_left:
                if ((data.sidesAlwaysOff & (int)enumSideBit.left) != 0)
                {
                    on = 0;
                }
                G = (on == 1) ?(data.startvertices + 12):0;
                E = (on == 1) ?(data.startvertices + 13):0;
                C = (on == 1) ?(data.startvertices + 14):0;
                A = (on == 1) ?(data.startvertices + 15):0;
                data.triangles[i++] = C;
                data.triangles[i++] = G;
                data.triangles[i++] = E;
                data.triangles[i++] = A;
                data.triangles[i++] = C;
                data.triangles[i++] = E;
                break;
            case enumSide.EFGH_up:
                if ((data.sidesAlwaysOff & (int)enumSideBit.up) != 0)
                {
                    on = 0;
                }
                E = (on == 1) ?(data.startvertices + 16):0;
                F = (on == 1) ?(data.startvertices + 17):0;
                G = (on == 1) ?(data.startvertices + 18):0;
                H = (on == 1) ?(data.startvertices + 19):0;
                data.triangles[i++] = E;
                data.triangles[i++] = G;
                data.triangles[i++] = H;
                data.triangles[i++] = F;
                data.triangles[i++] = E;
                data.triangles[i++] = H;
                break;
            case enumSide.ABCD_down:
                if ((data.sidesAlwaysOff & (int)enumSideBit.down) != 0)
                {
                    on = 0;
                }
                A = (on == 1) ?(data.startvertices + 20):0;
                B = (on == 1) ?(data.startvertices + 21):0;
                C = (on == 1) ?(data.startvertices + 22):0;
                D = (on == 1) ?(data.startvertices + 23):0;
                data.triangles[i++] = C;
                data.triangles[i++] = A;
                data.triangles[i++] = B;
                data.triangles[i++] = D;
                data.triangles[i++] = C;
                data.triangles[i++] = B;
                break;
            default:
                Debug.Log("setTrianglesSide: Error [enumSide]=" + side.ToString());
                break;
        }

    }     
    public void setTriangleSides(int code)
    {
        if ((code & (int)enumSideBit.back) != 0)
        {
            setTriangleSide(enumSide.EFAB_back, 0);
        }
        if ((code & (int)enumSideBit.right) != 0)
        {
            setTriangleSide(enumSide.FHBD_right, 0);
        }
        if ((code & (int)enumSideBit.forward) != 0)
        {
            setTriangleSide(enumSide.HGDC_forward, 0);
        }
        if ((code & (int)enumSideBit.left) != 0)
        {
            setTriangleSide(enumSide.GECA_left, 0);
        }
        if ((code & (int)enumSideBit.up) != 0)
        {
            setTriangleSide(enumSide.EFGH_up, 0);
        }
        if ((code & (int)enumSideBit.down) != 0)
        {
            setTriangleSide(enumSide.ABCD_down, 0);
        }
    }
    public void turnOn(enumVoxelType voxeltype)
    {
        on = 1;        
        setTriangleSide(enumSide.EFAB_back, on);
        setTriangleSide(enumSide.FHBD_right, on);
        setTriangleSide(enumSide.HGDC_forward, on);
        setTriangleSide(enumSide.GECA_left, on);
        setTriangleSide(enumSide.EFGH_up, on);
        setTriangleSide(enumSide.ABCD_down, on);

        setUVVoxelType(voxeltype);
    }
    public void turnOff()
    {
        on = 0;
        setTriangleSide(enumSide.EFAB_back, on);
        setTriangleSide(enumSide.FHBD_right, on);
        setTriangleSide(enumSide.HGDC_forward, on);
        setTriangleSide(enumSide.GECA_left, on);
        setTriangleSide(enumSide.EFGH_up, on);
        setTriangleSide(enumSide.ABCD_down, on);
    } 
}
