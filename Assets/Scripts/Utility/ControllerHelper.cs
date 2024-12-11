using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum ControllerMode {
    Adjustment,
    Labeling,
    Menu,
    Selecting,
    Hidden,
}

public class ControllerHelper: MonoBehaviour
{
    [SerializeField] private GameObject bumper;
    [SerializeField] private GameObject trigger;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject pad;


    void Start()
    {
        SetMode(ControllerMode.Selecting);
    }

    public void SetMode(ControllerMode mode)
    {
        SetAllActive(true);
        TMP_Text bumperText = bumper.GetComponent<TMP_Text>();
        TMP_Text triggerText = trigger.GetComponent<TMP_Text>();
        TMP_Text menuText = menu.GetComponent<TMP_Text>();
        // TMP_Text padText = pad.GetComponent<TMP_Text>();

        Debug.Log("Setting controller mode to " + mode);

        switch (mode)
        {
            case ControllerMode.Adjustment:
                bumperText.text = "Rotate";
                triggerText.text = "Grab";
                menuText.text = "Confirm";
                break;
            case ControllerMode.Labeling:
                bumper.SetActive(false);
                triggerText.text = "Add Label";
                menuText.text = "Settings";
                break;
            case ControllerMode.Menu:
                bumper.SetActive(false);
                triggerText.text = "Select";
                menuText.text = "Close Menu";
                break;
            case ControllerMode.Selecting:
                bumper.SetActive(false);
                triggerText.text = "Select";
                menu.SetActive(false);
                break;
            case ControllerMode.Hidden:
                SetAllActive(false);
                break;
        }
    }

    private void SetAllActive(bool value)
    {
        bumper.SetActive(value);
        trigger.SetActive(value);
        menu.SetActive(value);
        // pad.SetActive(value);
    }
}