using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class NewLabel : MonoBehaviour
{
    private Payload payload;
    private GameObject building;

    [SerializeField] private Orchestrator orchestrator;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button cancelButton;

    // Start is called before the first frame update
    void Start()
    {
        cancelButton.onClick.AddListener(Cancel);
        inputField.onSubmit.AddListener(CommitLabel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Cancel() {
        orchestrator.CancelLabelCreation();
    }

    public void InitiateCreation(Payload payload, GameObject building) {
        this.payload = payload;
        this.building = building;

        this.inputField.Select();
    }

    void CommitLabel(string inputText) {
        if(inputText.Trim().Length == 0) return;
        
        payload.name = inputText;
        orchestrator.CommitLabel(payload, building);

        this.inputField.text = "";
    }
}
