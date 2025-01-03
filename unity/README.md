# LabelAR Unity Application
This folder contains the code for the LabelAR unity application.

## Project Setup
LabelAR uses **Unity 2022.3.50f1** and the **Magic Leap Unity SDK v2.5.0**, which you can download from the Package manager of [**Magic Leap Hub**](https://ml2-developer.magicleap.com/downloads).

Please modify `Packages/manifest.json` with the location of your Magic Leap Unity SDK. This will most likely be:
```json
"com.magicleap.unitysdk": "file:<your_path>/MagicLeap/tools/unity/v2.5.0/com.magicleap.unitysdk.tgz",
```

After modifying the manifest, **Unity Hub** should take care of downloading the correct Unity version and you will be able to build for the Magic Leap 2 device by simply selecting the device in the Build Settings dropdown within the editor.

## Hosting your own server
If you are running your own server, the `Request.baseUrl` static attribute within the `Assets/Scripts/Utility/Request.cs` script needs to be modified with the URL of your web server.

## Obtaining the assets
For simplicity, we provide a set of pre-selected assets for the city of Zürich which you can find [here](https://polybox.ethz.ch/index.php/s/JkaCCkGQjmU8Mh3). After downloading and unzipping the two files, replace the empty `Buildings` and `Terrain` folders under `Assets/Resources` with the downloaded folders.

It's also possible to manually setup any location of your choice by following these steps:

### 1. Building meshes
Download the relevant section of [**SwissBuildings3D 2.0**](https://www.swisstopo.admin.ch/it/modello-del-territorio-swissbuildings3d-2-0#swissBUILDINGS3D-2.0---Download). This should be a file named `swissbuildings3d_<numbers>.dxf.zip`.

Unzip and move this file to the `Assets/StreamingAssets` folder.

### 2. Terrain mesh
Download the relevant section of the [**SwissALTI3D**](https://www.swisstopo.admin.ch/en/height-model-swissalti3d#swissALTI3D---Download) map. Make sure the format is `XYZ(ZIP)` and the resolution is **2m**. This should be a file named `swissalti3d_<numbers>.xyz.zip`.

Since the maximum resolution allowed is **2m**, which is too space-consuming, we will first downsample this file.
Unzip this file and rename it `map.xyz`, then, if not done previously, build the python environment:

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

Then you can simply run the downsampling script:
```bash
python terrain_downsampler.py
```

Move the `ds_map.xyz` file to the `Assets/StreamingAssets` folder.

### 3. Convert to Unity assets
Once all the data is in the `StreamingAssets` folder (you can mix and match multiple `.dxf` files) we provide a tool within the Unity Editor to convert these to assets in the `Assets/Resources` folder. Simply open the `Tools` dropdown and select the ***Building Importer*** and ***Terrain Importer*** tools.