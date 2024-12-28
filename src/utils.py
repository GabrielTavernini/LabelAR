from pyproj import Transformer

def gps_to_lv95(lat, lon, height):
    x, y, z = Transformer.from_crs("EPSG:4326", "EPSG:2056", always_xy=True).transform(lon, lat, height)
    return x, y, z

def lv95_to_gps(x, y, height):
    lon, lat, z = Transformer.from_crs("EPSG:2056", "EPSG:4326", always_xy=True).transform(x, y, height)
    return lon, lat, z