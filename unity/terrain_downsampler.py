import os
import requests
import zipfile
from io import BytesIO

import pandas as pd
import numpy as np

df = pd.read_csv("map.xyz", header=None)

folder_path = "ds_map.xyz"
if os.path.exists(folder_path):
    import shutil
    shutil.rmtree(folder_path)
os.makedirs(folder_path)

for idx, link in enumerate(df.iloc[:, 0]):
    try:
        response = requests.get(link)
        response.raise_for_status()  
        with zipfile.ZipFile(BytesIO(response.content)) as zip_file:
            zip_file.extractall(folder_path)
        print(f"Downloaded and extracted ZIP file from {link} to {folder_path}")
    except requests.RequestException as e:
        print(f"Failed to download ZIP file from {link}: {e}")
    except zipfile.BadZipFile:
        print(f"Failed to extract ZIP file from {link}: Not a valid ZIP file")
dfs = []
for filename in os.listdir(folder_path):
    file_path = os.path.join(folder_path, filename)
    if os.path.isfile(file_path)  and filename.endswith('.xyz'):  
        print(f"Processing file: {file_path}")
        dfs.append(pd.read_csv(file_path, sep='\s+', header=0, names=["X", "Y", "Z"]))
df_points = pd.concat(dfs, ignore_index=True)

df_points['X'] = pd.to_numeric(df_points['X'], errors='coerce', downcast='float')
df_points['Y'] = pd.to_numeric(df_points['Y'], errors='coerce', downcast='float')
df_points['Z'] = pd.to_numeric(df_points['Z'], errors='coerce', downcast='float')
resolution = 10
df_points['X_bin'] = (df_points['X'] / resolution).round().astype(int)
df_points['Y_bin'] = (df_points['Y'] / resolution).round().astype(int)

df_points = df_points.sort_values(by=['Y', 'X'], ascending=[True, True])
downsampled_df = df_points.drop_duplicates(subset=['X_bin', 'Y_bin'])
downsampled_df = downsampled_df[['X_bin', 'Y_bin', 'Z']]
downsampled_df = downsampled_df.rename(columns={'Y_bin': 'Z', 'Z': 'Y', 'X_bin':'X'})
downsampled_df['X'] = downsampled_df['X'] * 10
downsampled_df['Z'] = downsampled_df['Z'] * 10
downsampled_df = downsampled_df[sorted(downsampled_df.columns)]
downsampled_df.to_csv(folder_path +"/downsampled_file.xyz", sep=' ', header=True, index=False)
