using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicLeap.OpenXR.Features.LocalizationMaps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

public class SceneSelection : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button openButton;
    [SerializeField] private Orchestrator orchestrator;

    private LocalizationMap[] maps;
    private MagicLeapLocalizationMapFeature localizationMapFeature;


    void Start()
    {
        localizationMapFeature = OpenXRSettings.Instance.GetFeature<MagicLeapLocalizationMapFeature>();
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        XrResult result = localizationMapFeature.GetLocalizationMapsList(out maps);
        if (result == XrResult.Success)
            foreach(LocalizationMap map in maps)
                dropdown.options.Add(new TMP_Dropdown.OptionData(map.Name));

        openButton.onClick.AddListener(openClick);
    }

    private void openClick() {
        Debug.Log($"Request localization in: {maps[dropdown.value].MapUUID}");
        XrResult result = localizationMapFeature.RequestMapLocalization(maps[dropdown.value].MapUUID);
        Debug.Log($"Localize request result: {result}");

        orchestrator.StartCoroutine(orchestrator.Open(maps[dropdown.value].Name));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
