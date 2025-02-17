using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Slider zeppelinVolumeSlider;
    [SerializeField] private Slider containerMassSlider;
    [SerializeField] private GameObject zeppelin;
    [SerializeField] private Zeppelin zeppelinScript;
    [SerializeField] private TextMeshProUGUI volumeSliderValue;
    [SerializeField] private TextMeshProUGUI containerMassValue;
    [SerializeField] private ThirdPersonOrbitCamBasic cameraControllerScript;

    private bool isGamePaused = false;

    private void Start()
    {
        zeppelinVolumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        containerMassSlider.onValueChanged.AddListener(OnContainerMassSliderValueChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void SetVolumeSlider(float value)
    {
        zeppelinVolumeSlider.value = value;
    }

    void OnVolumeSliderValueChanged(float value)
    {
        volumeSliderValue.text = zeppelinVolumeSlider.value.ToString("n") + " m<sup>3</sup>";
        zeppelinScript.CalculateZeppelinScale(value);
    }

    void OnContainerMassSliderValueChanged(float value)
    {
        containerMassValue.text = containerMassSlider.value.ToString("n") + " kg";
        zeppelinScript.ChangeContainerMass(value);
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        isGamePaused = false;
        cameraControllerScript.enabled = true;
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        isGamePaused = true;
        cameraControllerScript.enabled = false;
        Time.timeScale = 0f;
    }
}
