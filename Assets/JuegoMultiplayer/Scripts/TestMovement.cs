using System;
using Unity.Netcode;
using UnityEngine;

public class TestMovement : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Hola soy un character");

    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if(Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(0, 0, 1 * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += new Vector3(0, 0, -1* Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += new Vector3(-1 * Time.deltaTime, 0, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += new Vector3(1 * Time.deltaTime, 0, 0);
            }
        }
    }
}
