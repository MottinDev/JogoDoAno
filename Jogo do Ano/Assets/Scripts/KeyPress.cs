using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideCursor : MonoBehaviour
{
    public float inactiveTime = 5f; // Time in seconds before hiding the cursor
    private float timer;
    private bool isCursorHidden;

    void Start()
    {
        // Initialize the timer and hide the cursor at the start
        timer = inactiveTime;
        Cursor.lockState = CursorLockMode.locked;
        isCursorHidden = false;
        Cursor.visible = true;
    }

    void Update()
    {
        // Reset the timer if there is user interaction
        if (Input.anyKey || Input.mousePosition != Vector3.zero)
        {
            timer = inactiveTime;
            if (isCursorHidden)
            {
                // Show cursor if it was hidden and there is interaction
                Cursor.visible = true;
                isCursorHidden = false;
            }
        }
        else
        {
            // Decrease the timer based on time passed
            timer -= Time.deltaTime;
            if (timer <= 0 && !isCursorHidden)
            {
                // Hide the cursor when the timer reaches zero
                Cursor.lockState = CursorLockMode.locked;
                Cursor.visible = false;
                isCursorHidden = true;
            }
        }

        // Make the cursor visible when the Alt key is pressed
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            Cursor.visible = true;
            isCursorHidden = false;
            timer = inactiveTime; // Reset timer if Alt is pressed
        }
    }
}
