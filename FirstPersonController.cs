using UnityEngine;
using System.Collections;

public class FirstPersonController : MonoBehaviour {

	public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float mouseSensitivity = 5.0f;
    public float pitchRange = 20.0f;
    public Vector3 restPosition;
    public float transitionSpeed = 20f;
    public float bobWalk = 4.8f;
    public float bobRun = 8f;
    public float bobAmount = 0.05f;
    public AudioSource stepSound;

    float pitchRotation = 0f;
	float verticalVelocity = 0;
    float timer = Mathf.PI / 2;
    float stepTimer = Mathf.PI / 2;
    Vector3 camPos;
    CharacterController cc;
    private float movementSpeed;
    private float bobSpeed;
    bool stepLeft = true;

	// Use this for initialization
	void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cc = GetComponent<CharacterController> ();
	}

    void Awake ()
    {
        camPos = Camera.main.transform.localPosition;
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = runSpeed;
            bobSpeed = bobRun;
        }
        else {
            movementSpeed = walkSpeed;
            bobSpeed = bobWalk;
        }

		// Rotation
		float rotYaw = Input.GetAxis ("Mouse X") * mouseSensitivity;
		transform.Rotate (0, rotYaw, 0);

		pitchRotation -= Input.GetAxis ("Mouse Y") * mouseSensitivity;
		pitchRotation = Mathf.Clamp (pitchRotation, -pitchRange, pitchRange);
		Camera.main.transform.localRotation = Quaternion.Euler (pitchRotation, 0, 0);

		// Movement
		float forwardSpeed = Input.GetAxis ("Vertical") * movementSpeed;
		float sideSpeed = Input.GetAxis ("Horizontal") * movementSpeed;

		verticalVelocity += Physics.gravity.y * Time.deltaTime;	

		Vector3 speed = new Vector3 (sideSpeed, verticalVelocity, forwardSpeed);
		speed = transform.rotation * speed;

		cc.Move( speed * Time.deltaTime);

        // Add Headbob
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)    // If we're moving
        {
            timer += bobSpeed * Time.deltaTime;
            stepTimer -= bobSpeed * Time.deltaTime;

            //use the timer value to set the position
            Vector3 newPosition = new Vector3(Mathf.Cos(timer) * bobAmount, restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z); //abs val of y for a parabolic path
            camPos = newPosition;
        }
        else
        {
            timer = Mathf.PI / 2; //reinitialize
            stepTimer = Mathf.PI / 2;
            stepLeft = true;

            Vector3 newPosition = new Vector3(Mathf.Lerp(camPos.x, restPosition.x, transitionSpeed * Time.deltaTime), Mathf.Lerp(camPos.y, restPosition.y, transitionSpeed * Time.deltaTime), Mathf.Lerp(camPos.z, restPosition.z, transitionSpeed * Time.deltaTime)); //transition smoothly from walking to stopping.
            camPos = newPosition;
        }

        if (stepTimer <= 0)
        {
            if (stepLeft)
            {
                stepSound.panStereo = -.15f;
                stepLeft = false;
            }
            else
            {
                stepSound.panStereo = .15f;
                stepLeft = true;
            }
                      
            stepSound.Play();
            stepTimer = Mathf.PI;
        }

        if (timer > Mathf.PI * 2) //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
            timer = 0;

        Camera.main.transform.localPosition = camPos;

        // Now, just throw in the quit game key
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
	}
}