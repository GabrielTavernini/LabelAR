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

    [SerializeField] private Button adjustButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button solidButton;
    [SerializeField] private Button transparentButton;
    [SerializeField] private Button semiTransparentButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Toggle visibilityToggle;
    [SerializeField] private Toggle occlusionToggle;
    [SerializeField] private GameObject helpViewSettings;

    void Start()
    {
        //editButton.onClick.AddListener(SetEdit);
        solidButton.onClick.AddListener(SetSolid);
        transparentButton.onClick.AddListener(SetFullyTransparent);
        semiTransparentButton.onClick.AddListener(SetTransparent);
        visibilityToggle.onValueChanged.AddListener(ToggleVisibility);
        occlusionToggle.onValueChanged.AddListener(ToggleOcclusion);
        editButton.onClick.AddListener(SetEdit);
        adjustButton.onClick.AddListener(SetAdjust);
        helpButton.onClick.AddListener(OpenHelp);
        closeButton.onClick.AddListener(CloseHelp);
    }

    public void SetSolid()
    {
        MaterialHelper.SetSolid(orchestrator.material);
    }

    public void SetFullyTransparent()
    {
        MaterialHelper.SetShader(orchestrator.material, orchestrator.transparentShader);
    }

    public void SetTransparent()
    {
        MaterialHelper.SetTransparent(orchestrator.material);
    }

    public void SetEdit()
    {
        orchestrator.SetEditMode(true);
    }

    public void SetAdjust()
    {
        orchestrator.SetAdjustmentMode(true);
    }

    public void ToggleVisibility(bool value)
    {
        orchestrator.SetFarClippingPlane(value ? Request.response.visibility : -1);
    }

    public void ToggleOcclusion(bool value)
    {
        orchestrator.SetOcclusion(value);
    }

    public void OpenHelp()
    {
        helpViewSettings.SetActive(!helpViewSettings.activeSelf);
    }

    public void CloseHelp()
    {
        helpViewSettings.SetActive(false);
    }

    void Update()
    {

    }
}
