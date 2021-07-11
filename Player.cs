using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    #region --- helper ---
    private enum enumMouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2,
    }
    #endregion

    public float MoveSpeed = 3.5f;
    public float TurnSpeed = 120f;
    public float JumpForce = 6f;
    public Camera cam = null;
    private Rigidbody rb = null;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        if (cam != null)
        {
            //make camera child of this gameobject (to be this object's eyes)
            Bounds B = this.GetComponent<Renderer>().bounds;
            cam.transform.position = B.center;
            cam.transform.parent = this.transform;

            //lock mouse to gamescreen
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            //regular mouse mode
            Cursor.lockState = CursorLockMode.None;
        }

        //movement (WASD, Arrows)
        float forward = Input.GetAxis("Vertical");
        float strafe = Input.GetAxis("Horizontal");
        this.transform.Translate(Vector3.forward * forward * MoveSpeed * Time.deltaTime);
        this.transform.Translate(Vector3.right * strafe * MoveSpeed * Time.deltaTime);

        //looking around (MOUSE)
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        Vector3 mouselook = new Vector2(mx, my);
        mouselook = Vector2.Scale(mouselook, new Vector2(TurnSpeed, TurnSpeed));
        this.transform.localRotation *= Quaternion.AngleAxis(mouselook.x * Time.deltaTime, Vector3.up);
        if (cam != null)
        {
            cam.transform.localRotation *= Quaternion.AngleAxis(-mouselook.y * Time.deltaTime, Vector3.right);
        }

        //jump (SPACE)
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
        }

        //add voxel (LEFT MOUSE)
        if (Input.GetMouseButtonDown((int)enumMouseButton.Left) == true)
        {
            AddVoxel();
        }
        //remove voxel (RIGHT MOUSE)
        if (Input.GetMouseButtonDown((int)enumMouseButton.Right) == true)
        {
            RemoveVoxel();
        }
    }
    private void AddVoxel()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f) == true)
        {
            if (hit.transform.CompareTag("Chunk") == true)
            {
                Chunk script = hit.transform.gameObject.GetComponent<Chunk>();
                script.addVoxel(hit, Voxel.enumVoxelType.lava);
            }
        }
    }
    private void RemoveVoxel()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f) == true)
        {
            if (hit.transform.CompareTag("Chunk") == true)
            {
                Chunk script = hit.transform.gameObject.GetComponent<Chunk>();
                script.removeVoxel(hit);
            }
        }
    }
}
