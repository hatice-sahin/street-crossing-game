using UnityEngine;

public class WASDMovementProvider : MonoBehaviour
{
    public float speed = 3.5f;
    public float gravityMultiplier = 1.0f;
    public GameObject traffic;

    private CharacterController characterController = null;
    private GameObject head = null;

    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0F;

    private Vector3 mouseDelta;
    private Vector3 mouseLast;
    private bool _mouseButtonOnHold;

    protected void Awake()
    {
        characterController = GetComponent<CharacterController>();
        head = GetComponentInChildren<Camera>().gameObject;
    }

    private void Start()
    {
        mouseLast = Input.mousePosition;
    }

    private void FixedUpdate()
    {
        CheckForInput();
        ApplyGravity();
    }


    private void CheckForInput()
    {
        Vector2 move = new Vector2();

#if UNITY_EDITOR
        mouseDelta = Input.mousePosition - mouseLast;
        mouseLast = Input.mousePosition;

        if (axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + mouseDelta.x * sensitivityX;

            rotationY += mouseDelta.y * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, mouseDelta.x * sensitivityX, 0);
        }
        else
        {
            rotationY += mouseDelta.y * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }

        if (Input.GetKey(KeyCode.W))
        {
            move.y = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            move.y = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            move.x = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            move.x = 1;
        }
#else
		var horizontal = Input.GetAxis("Horizontal");//
		var vertical = Input.GetAxis("Vertical");
		if (horizontal < -0.2f) {
			move.y = -horizontal;
		}
		if (horizontal > 0.2f) {
			move.y = -horizontal;
		}
		if (vertical < -0.2f) {
			move.x = vertical;
		}
		if (vertical > 0.2f) {
			move.x = vertical;
		}
#endif

/*
if (!ScenarioControl.Instance.autonomous)
{
    if (Input.anyKey && !(
                          Input.GetKey(KeyCode.W) ||
                          Input.GetKey(KeyCode.A) ||
                          Input.GetKey(KeyCode.S) ||
                          Input.GetKey(KeyCode.D) ||
                          OVRInput.Axis2D.Any < 0 ||
                          OVRInput.Axis2D.Any > 0 
                          ) ||
        Input.anyKeyDown && !(
                              Input.GetKeyDown(KeyCode.W) ||
                              Input.GetKeyDown(KeyCode.A) ||
                              Input.GetKeyDown(KeyCode.S) ||
                              Input.GetKeyDown(KeyCode.D) ||
                              OVRInput.Axis2D.Any < 0 ||
                              OVRInput.Axis2D.Any > 0
                              ) ||
        _mouseButtonOnHold)
    {
        _mouseButtonOnHold = true;
        var script = traffic.GetComponent<TrafficControl>();
        script.ManualHold(gameObject, true);
    }

    if ((!Input.anyKey && _mouseButtonOnHold))
    {
        _mouseButtonOnHold = false;
        var script = traffic.GetComponent<TrafficControl>();
        script.ManualHold(gameObject, false);
    }
    */

//TOD: IMPLEMENT HAND-TRACKED MANUAL HOLD HERE
if (!ScenarioControl.Instance.autonomous)
{
    if (Input.GetKey(KeyCode.Mouse0) || 
                OVRInput.Get(OVRInput.Button.One) || 
                OVRInput.Get(OVRInput.Button.Two) || 
                OVRInput.Get(OVRInput.Button.Three) || 
                OVRInput.Get(OVRInput.Button.Four) || 
                _mouseButtonOnHold)
    {
        _mouseButtonOnHold = true;
        ScenarioControl.Instance.setButtonPressed();
        var script = traffic.GetComponent<TrafficControl>();
        script.ManualHold(gameObject, true);
    }

    if ((!(Input.GetKey(KeyCode.Mouse0) ||
                OVRInput.Get(OVRInput.Button.One) ||
                OVRInput.Get(OVRInput.Button.Two) ||
                OVRInput.Get(OVRInput.Button.Three) ||
                OVRInput.Get(OVRInput.Button.Four)) && _mouseButtonOnHold))
    {
        _mouseButtonOnHold = false;
        ScenarioControl.Instance.setButtonReleased();
        var script = traffic.GetComponent<TrafficControl>();
        script.ManualHold(gameObject, false);
    }

    /*if (ScenarioControl.Instance.leftHandRaised)
    {
        _mouseButtonOnHold = true;
        var script = traffic.GetComponent<TrafficControl>();
        script.ManualHold(gameObject, true);
                ScenarioControl.Instance.setButtonPressed();


                if (!ScenarioControl.Instance.leftHandRaised)
        {
            _mouseButtonOnHold = false;
            script = traffic.GetComponent<TrafficControl>();
            script.ManualHold(gameObject, false);
        }
    }*/
}

    StartMove(move.normalized);
}

private void StartMove(Vector2 position)
{
Vector3 direction = new Vector3(position.x, 0, position.y);
Vector3 headRotation = new Vector3(0, head.transform.eulerAngles.y, 0);

direction = Quaternion.Euler(headRotation) * direction;

Vector3 movement = direction * speed;
characterController.Move(movement * Time.deltaTime);
}

private void ApplyGravity()
{
Vector3 gravity = new Vector3(0, Physics.gravity.y * gravityMultiplier, 0);
if (!characterController.isGrounded)
    characterController.Move(gravity * Time.deltaTime);
}
}
 