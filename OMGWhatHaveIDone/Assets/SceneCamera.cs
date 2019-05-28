using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneCamera : MonoBehaviour
{
    // Initialize the public variables
    public Button buttonLeft;
    public Button buttonRight;

    public Button buttonBack0;
    public Button buttonBack1;
    public Button buttonBack2;
    public Button buttonBack3;

    public Button[] consoleButton;
    public Button coffeeButton;

    public GameObject[] roomCamObject;
    public GameObject[] roomCanvObject;

    public Animator nukeAnimator;

    public IndicatorLight indicatorLightScript;

    public ParticleSystem coffeeParticleSystem;

    public GameObject gameOverTextObject;
    public GameObject victoryTextObject;

    public int roomAmount;

    // Run this code once at the start
    void Start()
    {
        // Add listeners to the buttons
        buttonLeft.onClick.AddListener(delegate { SetRoom(1); });
        buttonRight.onClick.AddListener(delegate { SetRoom(2); });

        buttonBack0.onClick.AddListener(delegate { SetRoom(0); });
        buttonBack1.onClick.AddListener(delegate { SetRoom(0); });
        buttonBack2.onClick.AddListener(delegate { SetRoom(3); });
        buttonBack3.onClick.AddListener(delegate { SetRoom(0); });

        coffeeButton.onClick.AddListener(delegate { Proceed(); });

        for (int i = 0; i < consoleButton.Length; i++)
        {
            int id = i;
            consoleButton[i].onClick.AddListener(delegate { ConsoleButtonClick(id); });
        }
    }

    // Set the current room
    void SetRoom(int roomID)
    {
        // Enable and disable the cameras and canvasses
        for (int i = 0; i < roomAmount; i++)
        {
            bool isActive = (i == roomID);

            roomCamObject[i].SetActive(isActive);
            roomCanvObject[i].SetActive(isActive);
        }
    }

    // Check the console button interactions
    void ConsoleButtonClick(int buttonID)
    {
        // Switch through the different button ID's
        switch (buttonID)
        {
            // Check the outcome of the action button
            case 0:
                CheckOutcome();
                break;

            // Disable the indicator light off
            case 1:
                indicatorLightScript.state = 0;
                break;

            // Disable the indicator light on
            case 2:
                indicatorLightScript.state = 1;
                break;
        }
    }

    // Check the outcome of the action button
    void CheckOutcome()
    {
        if (indicatorLightScript.state == 0)
            LaunchNukes();
        else
            MakeCoffee();
    }

    // Launch the nukes
    void LaunchNukes()
    {
        nukeAnimator.SetBool("Launch", true);
        gameOverTextObject.SetActive(true);
    }

    // Make coffee
    void MakeCoffee()
    {
        coffeeParticleSystem.Play();
        victoryTextObject.SetActive(true);
    }

    // Proceed to the next level if possible
    void Proceed()
    {
        if (victoryTextObject.activeSelf)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
