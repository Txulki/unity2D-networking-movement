using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //=================
    //PLAYER VARIABLES:
    //=================
    [SerializeField] GameObject Object; //Player GameObject.
    [SerializeField] UDP_Client Client; //UDP_Client
    [SerializeField] float MovementSpeed; //Player movement speed


    [Space(10)]
    //=================
    //CONTROL KEYCODES:
    //=================

    [SerializeField] private KeyCode Up, Down, Left, Right; //Keys for WASD movement.


    private void FixedUpdate()
    {
        //Check for any movement control inputs.
        if(Input.anyKey)
        {
            Movement();
        }

    }

    private void Movement()
    {
        int MovementX = 0;
        int MovementY = 0;

        if(Input.GetKey(Up))
        {
            MovementY++;
        }

        if(Input.GetKey(Down))
        {
            MovementY--;
        }

        if(Input.GetKey(Left))
        {
            MovementX--;
        }

        if(Input.GetKey(Right))
        {
            MovementX++;
        }

        bool send = false;

        if (MovementY != 0 || MovementX != 0)
        {
            send = true;
        }

        if(MovementX != 0 && MovementY != 0)
        {
            send = true;
            Object.transform.Translate(Vector2.right * Time.fixedDeltaTime * MovementX * (MovementSpeed * Mathf.Sqrt(2)) / 2);
            Object.transform.Translate(Vector2.up * Time.fixedDeltaTime * MovementY * (MovementSpeed * Mathf.Sqrt(2)) / 2);
        }
        else if(MovementY != 0)
        {
            Object.transform.Translate(Vector2.up * Time.fixedDeltaTime * MovementY * MovementSpeed);
        }
        else if(MovementX != 0)
        {
            Object.transform.Translate(Vector2.right * Time.fixedDeltaTime * MovementX * MovementSpeed);
        }

        if(send)
        {
            Client.SendMoveInput(MovementX, MovementY);
        }
    }

    public void SetMovement(float[,] Coords)
    {
        Object.transform.position = new Vector2(Coords[0, 0], Coords[0, 1]);
    }
}
