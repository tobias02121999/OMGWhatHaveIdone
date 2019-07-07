using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorLight : MonoBehaviour
{
    // Initialize the public variables
    public int state;
    public GameObject[] lightStateObject;

    // Run this code every single frame
    void Update()
    {
        SetState(); // Set the current state
    }

    // Set the current state
    void SetState()
    {
        for (int i = 0; i < lightStateObject.Length; i++)
        {
            bool isActive = (i == state);
            lightStateObject[i].SetActive(isActive);
        }
    }
}
