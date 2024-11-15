using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicLeap.OpenXR.Features.LocalizationMaps;
using MagicLeap.OpenXR.Features;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

public class ViewSettings : MonoBehaviour
{
    [SerializeField] private Orchestrator orchestrator;
    [SerializeField] private Shader transparentShader;

    [SerializeField] private Button editButton;
    [SerializeField] private Button solidButton;
    [SerializeField] private Button transparentButton;
    [SerializeField] private Button semiTransparentButton;
    [SerializeField] private Toggle visibilityToggle;

    void Start()
    {
        //editButton.onClick.AddListener(SetEdit);
        solidButton.onClick.AddListener(SetSolid);
        transparentButton.onClick.AddListener(SetFullyTransparent);
        semiTransparentButton.onClick.AddListener(SetTransparent);
        visibilityToggle.onValueChanged.AddListener(ToggleVisibility);
        editButton.onClick.AddListener(SetEdit);
    }

    public void SetSolid()
    {
        MaterialHelper.SetSolid(orchestrator.material);
    }

    public void SetFullyTransparent()
    {
        MaterialHelper.SetFullyTransparent(orchestrator.material, transparentShader);
    }

    public void SetTransparent()
    {
        MaterialHelper.SetTransparent(orchestrator.material);
    }

    public void SetEdit()
    {
        orchestrator.SetAdjustmentMode(true);
    }

    public void ToggleVisibility(bool value)
    {
        orchestrator.SetFarClippingPlane(value ? Request.response.visibility : -1);
    }

    void Update()
    {

    }
}
