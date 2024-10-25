/*InputManager.cs
* Description: Handles user input.
*/
using UnityEngine;
using System.Collections;


public class InputManager : MonoBehaviour
{
    [SerializeField] JoyStick joyStick;

    [Tooltip("Forward force applied to player")]
    public float force = 1250.0f;
    public float startForce = 600f;

    [Tooltip("Rotation speed of the player")]
    public float rotationSpeed = 75.0f;
    public float rotLimitAngle = 90f;

    [Tooltip("Amount of torque applied to 'lift' the player as they turn")]
    public float turnLift = 750.0f;

    [Tooltip("Amount to artificially assist the player in staying upright")]
    public float uprightAssist = 1.0f;

    public bool canRotate = false;
    private bool isTouchingCrab = false;
    private bool isTouchingGround = false;
    private Gyroscope gyro;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gyro = Input.gyro;

        if (PlayerPrefs.GetInt("gyro") == 1)
            gyro.enabled = true;
        else
            gyro.enabled = false;
    }


    float getTilt()
    {
        if (!canRotate) return 0;

        float direction = 0.0f;

        // if (gyro != null && gyro.enabled)
        // {
        //     direction = (gyro.attitude * Vector3.left).z;
        // }
        // else
        // {
        //     //if (Input.GetKey(KeyCode.D))
        //     //    direction += 1.0f;

        //     //if (Input.GetKey(KeyCode.A))
        //     //    direction -= 1.0f;

        //     // for mobile device
        // }
        direction += joyStick.Horizontal();
        float angleY = rb.rotation.eulerAngles.y > 180f ? rb.rotation.eulerAngles.y - 360 : rb.rotation.eulerAngles.y;         
        if(angleY < -rotLimitAngle && direction < 0) direction = 0;
        if(angleY > rotLimitAngle && direction > 0) direction = 0;
        return direction;
    }

    void FixedUpdate()
    {
        if (isTouchingGround)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0.0f, getTilt() * rotationSpeed * Time.deltaTime, 0.0f));
            rb.AddRelativeTorque(getTilt() * Vector3.back * turnLift * Time.deltaTime);
            rb.AddForce(rb.rotation * Vector3.forward * force * Time.deltaTime);

            rb.MoveRotation(Quaternion.Euler(Mathf.MoveTowardsAngle(FixAngle(rb.rotation.eulerAngles.x), 0.0f, uprightAssist * Time.deltaTime), rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
        }
        else
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0.0f, getTilt() * rotationSpeed * 2 * Time.deltaTime, 0.0f));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Terrain")
            isTouchingGround = true;
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Terrain")
            isTouchingGround = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Terrain")
            isTouchingGround = false;
    }

    private float FixAngle(float angle)
    {
        if (angle < 0.0f)
            angle += 360.0f;
        else
            while (angle > 360.0f)
                angle -= 360.0f;

        return angle;
    }

    public void OnTouchingCrab()
    {
        if (force <= 150f) force = 150;
        else force *= 0.7f;
        
        Debug.Log(force);
    }

    public void StopMove() 
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
    }

    // not use, just test for stun, can erase this function
    IEnumerator Stun(float stunTime)
    {
        float curr = 0;


        Debug.Log("stun start");
        while (curr <= stunTime)
        {
            curr += Time.deltaTime;

            yield return null;
        }

        isTouchingCrab = false;
        Debug.Log("stun finish");
    }
}

