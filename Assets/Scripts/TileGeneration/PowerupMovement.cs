using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupMovement : MonoBehaviour
{
    private static float movementAmount = 0.4f;
    private static float movementSpeed = 0.2f;

    private float minHeight;
    private float maxHeight;

    private float startY;
    private float targetY;
    private float t;

    void Start()
    {
        minHeight = transform.position.y;
        maxHeight = transform.position.y + movementAmount;
        startY = minHeight;
        targetY = maxHeight;
        t = 0;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime * movementSpeed;
        Vector3 startPos = new Vector3(transform.position.x, startY, transform.position.z);
        Vector3 endPos = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.MoveTowards(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));

        if (Vector3.Distance(transform.position, endPos) < 0.01f)
        {
            if (targetY > (maxHeight + minHeight) / 2)
            {
                targetY = minHeight;
                startY = maxHeight;
                t = 0;
            }
            else
            {
                targetY = maxHeight;
                startY = minHeight;
                t = 0;
            }
        }
    }
}
