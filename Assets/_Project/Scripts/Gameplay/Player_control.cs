using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player_control : NetworkBehaviour 
{
    public float speed = 5f;

    [Header("Map Boundaries")]
    private float minX = -8.3f;
    private float maxX = 8.3f;
    private float minY = -4.5f;
    private float maxY = 4.5f;

    void Update()
    {
        if (!IsServer) return;
        
        if (!GameManager.isGameStarted) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);

        Vector3 newPosition = transform.position + movement * speed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }
}
