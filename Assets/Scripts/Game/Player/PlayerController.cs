using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float sensitivity;
    public float movement_speed;
    public CharacterController characterController;
    public float gravity;
    public Transform orientation;
    public Camera player_camera;
    public float distanceToGround;
    public float jumpVelocity;



    private Vector3 movedirection;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraXRotation;
    private float bodyYRotation;



    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    void Update()
    {
        if (!IsOwner)
        {
            if (GetComponentInChildren<Camera>().enabled)
            {
                GetComponentInChildren<Camera>().enabled = false;
            }
            return;
        };
            checkIfGrounded(player_camera.transform.position);


            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                jump();
            }


            //rotating character
            change_player_rotation(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"),
                player_camera.transform.localRotation.eulerAngles, orientation.eulerAngles);
            player_camera.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0.0f);
            //player_camera.transform.GetComponent<NetworkTransform>().SetState
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, bodyYRotation, transform.eulerAngles.z);


            //визначаю в яку сторону потрібно рухатися
            float moveForvard = 0;
            float moveRight = 0;
            if (Input.GetKey(KeyCode.W))
            {
                moveForvard = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveForvard = -1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                moveRight = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                moveRight = -1;
            }


            //setting up movement vecots
            Vector3 vec = orientation.forward * moveForvard + orientation.right * moveRight;
            calculateMovementDirection(vec);
            calculateVelocity();



            //acually moving
            characterController.Move(new Vector3(movedirection.x, 0, movedirection.z) + velocity * Time.deltaTime);
        
    }


    
    public void calculateMovementDirection(Vector3 _vec)
    {
        movedirection = new Vector3(_vec.x, 0, _vec.z).normalized * movement_speed * Time.deltaTime;
    }


    
    public void checkIfGrounded(Vector3 CameraPosition)
    {
        isGrounded = Physics.CheckSphere(CameraPosition - new Vector3(0, distanceToGround, 0), 0.1f);
    }


    
    public void calculateVelocity()
    {
        if (isGrounded && velocity.y < -2)
        {
            velocity = new Vector3(velocity.x, -2f, velocity.z);
        }


        velocity = velocity - new Vector3(0, gravity * Time.deltaTime, 0);
    }


    public void jump()
    {
        velocity = new Vector3(velocity.x, jumpVelocity, velocity.z);
    }


    
    private void change_player_rotation(float x, float y, Vector3 playerCamera, Vector3 bodyTransform)
    {
        float rotate_x;
        float rotate_y;


        rotate_x = playerCamera.x - x * sensitivity;
        rotate_y = bodyTransform.y + y * sensitivity;


        if (rotate_x <= 280 && rotate_x >= 140)
        {
            rotate_x = 281;
        }
        else if (rotate_x >= 80 && rotate_x <= 260)
        {
            rotate_x = 79;
        }

        
        cameraXRotation = rotate_x;
        bodyYRotation = rotate_y;
    }
}
