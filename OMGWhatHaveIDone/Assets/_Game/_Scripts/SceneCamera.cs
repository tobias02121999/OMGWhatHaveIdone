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
    public Button buttonBack4;

    public Button[] consoleButton;
    public Button coffeeButton;
    public Button posterButton;
    public Button nextLevelButton;

    public GameObject[] roomCamObject;
    public GameObject[] roomCanvObject;

    public Animator nukeAnimator;

    public IndicatorLight indicatorLightScript;
    public IndicatorLight modeLightScript;
    public IndicatorLight overrideSwitchScript;

    public ParticleSystem coffeeParticleSystem;

    public int roomAmount;
    public int levelID;

    // Initialize the private variables
    bool victory;

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
        buttonBack4.onClick.AddListener(delegate { SetRoom(3); });

        posterButton.onClick.AddListener(delegate { SetRoom(4); });
        coffeeButton.onClick.AddListener(delegate { Proceed(); });

        if (levelID == 0)
            nextLevelButton.onClick.AddListener(NextLevel);

        for (int i = 0; i < consoleButton.Length; i++)
        {
            int id = i;
            consoleButton[i].onClick.AddListener(delegate { ConsoleButtonClick(id); });
        }
    }

    // Run this code every single update
    void Update()
    {
        // Control main console tasks
        ControlConsole();
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

            // Disable/enable coffee mode
            case 3:
                if (modeLightScript.state == 0)
                    modeLightScript.state = 1;
                else
                    modeLightScript.state = 0;
                break;

            // Disable/enable the override switch
            case 4:
                if (overrideSwitchScript.state == 0)
                    overrideSwitchScript.state = 1;
                else
                    overrideSwitchScript.state = 0;
                break;
        }
    }

    // Control main console tasks
    void ControlConsole()
    {
        if (modeLightScript.state == 1 && indicatorLightScript.state == 1 && overrideSwitchScript.state == 0)
            modeLightScript.state = 0;
    }

    // Check the outcome of the action button
    void CheckOutcome()
    {
        switch (levelID)
        {
            case 0:
                if (indicatorLightScript.state == 0)
                    LaunchNukes();
                else
                    MakeCoffee();
                break;

            case 1:
                if (indicatorLightScript.state == 0 || modeLightScript.state == 0)
                    LaunchNukes();
                else
                    MakeCoffee();
                break;
        }
    }

    // Launch the nukes
    void LaunchNukes()
    {
        nukeAnimator.SetBool("Launch", true);
        SetRoom(6);
        //gameOverTextObject.SetActive(true);
    }

    // Make coffee
    void MakeCoffee()
    {
        coffeeParticleSystem.Play();
        victory = true;
        //victoryTextObject.SetActive(true);
    }

    // Proceed to the victory screen if possible
    void Proceed()
    {
        if (victory)
            SetRoom(5);
    }

    // Load up the next level
    void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
