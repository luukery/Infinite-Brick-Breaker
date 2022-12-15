using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speedLimit;
    [SerializeField] float deAccelerationSpeed;
    [SerializeField] float AccelerationSpeed;
    [SerializeField] float maxRange = 7.3f;      //max range on the X axis that the paddle can go

    float acceleration;
    Vector2 move;

    [Header("Rotation")]
    [SerializeField] float maxRotation;
    [SerializeField] float maxRotatingSpeed;


    // Update is called once per frame
    void Update()
    {
        PaddleRotation();

        //the movement for the paddle
        #region movement

        move = new Vector2(acceleration, 0);

        if (Input.GetKey(KeyCode.D) && transform.position.x < maxRange && !Input.GetKey(KeyCode.A))        //accelerates to the left, until the speedlimit has been reached.
        {
            acceleration += AccelerationSpeed;

            if (acceleration > speedLimit)
            {
                acceleration = speedLimit;
            }
        }
        else if (Input.GetKey(KeyCode.A) && transform.position.x > -maxRange && !Input.GetKey(KeyCode.D))   //accelerates to the right, until the speedlimit has been reached.
        {
            acceleration -= AccelerationSpeed;


            if (acceleration < -speedLimit)
            {
                acceleration = -speedLimit;
            }
        }
        else                                        //I used else because now I can make it deaccelerate
        {

            if (acceleration < 0)
            {
                acceleration += deAccelerationSpeed; //slows down until it has reached the deacceleration speed limit, from there it snaps to 0

                if (acceleration > deAccelerationSpeed)
                {
                    acceleration = 0;
                }
            }


            if (acceleration > 0)                    //slows down until it has reached the deacceleration speed limit, from there it snaps to 0
            {
                acceleration -= deAccelerationSpeed;

                if (acceleration < deAccelerationSpeed)
                {
                    acceleration = 0;
                }
            }


            if (transform.position.x < -maxRange)
            {
                transform.position = new Vector2(-maxRange, transform.position.y);
            }


            if (transform.position.x > maxRange)
            {
                transform.position = new Vector2(maxRange, transform.position.y);
            }

        }


        transform.Translate(move * Time.deltaTime, Space.World);     //registers the movement

        #endregion
    }

    //the rotation of the paddle
    void PaddleRotation()
    {
        Vector3 maxTurn = new Vector3(0, 0, 15);


        if (Input.GetKey(KeyCode.A) && transform.position.x > -maxRange)
        {
            if (transform.rotation.z < 0.13f )
            {
                transform.Rotate(maxTurn * maxRotatingSpeed * Time.deltaTime, Space.Self);
            }
        }
        else if (Input.GetKey(KeyCode.D) && transform.position.x < maxRange)
        {
            if (transform.rotation.z > -0.13f )
            {
                transform.Rotate(-maxTurn * maxRotatingSpeed * Time.deltaTime, Space.Self); 
            }
        }
        else
        {

            if(transform.rotation.z < 0)
            {
                transform.Rotate(maxTurn * maxRotatingSpeed * Time.deltaTime, Space.Self);

                if(transform.rotation.z > -0.0005f)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                }
            }
            else if(transform.rotation.z > 0)
            {
                transform.Rotate(-maxTurn * maxRotatingSpeed * Time.deltaTime, Space.Self);

                if (transform.rotation.z < 0.0005f)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                }
            }

        }
    }


}
