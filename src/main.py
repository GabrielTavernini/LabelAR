from fastapi import FastAPI, HTTPException, status
import psycopg2
from psycopg2.extras import RealDictCursor
import uvicorn
import random
import utils
import requests
from typing import List
from pydantic import BaseModel

lastVisibility = 10000

def get_visibility(latitude, longitude):
    global lastVisibility
    url = "https://api.open-meteo.com/v1/forecast"
    params = {
        "latitude": latitude,
        "longitude": longitude,
        "hourly": "visibility",
        "past_hours": 0,
        "forecast_days": 1,
        "forecast_hours": 1
    }    
    try:
        r = requests.get(url, params, timeout=1).json()['hourly']['visibility']
        visibility = r[0]
        lastVisibility = visibility
    except:
        visibility = lastVisibility

    return float(visibility)

class AddLabel(BaseModel):
    north: float
    east: float
    height: float
    name: str
    buildings: List[str]

class DeleteLabel(BaseModel):
    name: str

class EditLabel(BaseModel):
    oldName: str
    newName: str

app = FastAPI()

@app.post("/add_label")
async def process_data(data: AddLabel):
    db_params = {
    'dbname': 'label_ar',
    'user': 'postgres',
    'password': 'postgres',
    'host': '127.0.0.1',  
    'port': '5432',
    'options': '-c client_encoding=UTF8'  
    }
    print(data)
    connection = psycopg2.connect(**db_params)
    with connection:
        cursor = connection.cursor(cursor_factory=RealDictCursor)
        try:
            longitude,latitude, _ = utils.lv95_to_gps(data.east,data.north,data.height)
            cursor.execute("""INSERT INTO coordinates (name, landmark, height) 
                            VALUES (%s, ST_SetSRID(ST_MakePoint(%s, %s), 4326), %s)""",
                            (data.name, longitude, latitude, data.height))
            connection.commit()
            cursor.execute("SELECT id FROM coordinates WHERE name = %s", (data.name,))
            results = cursor.fetchall()
            #assert len(results) == 1 
            id = results[0]['id']
            for building_id in data.buildings:
                cursor.execute("INSERT INTO buildings_ids (building_id, swisstopo_id) VALUES (%s, %s)", (id, building_id,))
            response = {
                "message" : "Label inserted correctly",
            }     
        except:
            raise HTTPException(status_code=403, detail="Some problem occured, please contact the team!")
    connection.commit()
    cursor.close()
    connection.close() 
    return response

@app.post("/delete_label")
async def process_data(data: DeleteLabel):
    db_params = {
    'dbname': 'label_ar',
    'user': 'postgres',
    'password': 'postgres',
    'host': '127.0.0.1',  
    'port': '5432',
    'options': '-c client_encoding=UTF8'  
    }
    connection = psycopg2.connect(**db_params)
    with connection:
        cursor = connection.cursor(cursor_factory=RealDictCursor)
        try:
            cursor.execute("DELETE FROM coordinates WHERE name = %s",(data.name,))
            connection.commit()
            response = {
                "message" : f"Label {data.name} deleted correctly",
            }  
            cursor.execute("SELECT SETVAL('coordinates_id_seq',(SELECT MAX(id) FROM coordinates))")   
            connection.commit()
        except Exception as e:
            print(e)
            raise HTTPException(status_code=403, detail="Some problem occured, please contact the team!")
    cursor.close()
    connection.close() 
    return response

@app.post("/edit_label")
async def process_data(data: EditLabel):
    db_params = {
    'dbname': 'label_ar',
    'user': 'postgres',
    'password': 'postgres',
    'host': '127.0.0.1',  
    'port': '5432',
    'options': '-c client_encoding=UTF8'  
    }
    connection = psycopg2.connect(**db_params)
    with connection:
        cursor = connection.cursor(cursor_factory=RealDictCursor)
        try:
            cursor.execute("""UPDATE coordinates
                              SET name = %s 
                              WHERE name= %s""",(data.newName, data.oldName))
            connection.commit()
            response = {
                "message" : f"Label {data.newName} updated correctly",
            }     
        except psycopg2.errors.UniqueViolation:
            raise HTTPException(status_code=400, detail=f"The name {data.newName} is already taken")
        except:
            raise HTTPException(status_code=403, detail="Some problem occured, please contact the team!")
    cursor.close()
    connection.close() 
    return response

# /status endpoint
@app.get("/status")
async def status():
    return {"message": "hello there"}

# /get_labels endpoint
@app.get("/get_labels")
async def get_labels(mapName): 
    db_params = {
    'dbname': 'label_ar',
    'user': 'postgres',
    'password': 'postgres',
    'host': '127.0.0.1',  
    'port': '5432',
    'options': '-c client_encoding=UTF8'  
    }
    connection = psycopg2.connect(**db_params)
    with connection:
        with connection.cursor(cursor_factory=RealDictCursor) as cursor:
            try:
                cursor.execute(f"SELECT ST_X(landmark::geometry) AS longitude, ST_Y(landmark::geometry) AS latitude, height, id as location_id FROM locations where name=%s", (mapName,))
                results = cursor.fetchall()
                assert len(results) == 1 
                location_id = results[0]['location_id']
                x,z,y = utils.gps_to_lv95(results[0]['latitude'], results[0]['longitude'], results[0]['height']) if len(results) else (0,0,0)
                visibility = get_visibility(latitude = results[0]['latitude'], longitude = results[0]['longitude'])
                cursor.execute("""SELECT   c1.name AS name, id,
                                            ST_Distance(c1.landmark, (SELECT l.landmark from locations l where l.name=%s )) AS distance_meters, 
                                            c1.height as height, ST_X(c1.landmark::geometry) AS longitude, 
                                            ST_Y(c1.landmark::geometry) AS latitude,
                                            id
                                            FROM coordinates c1
                                            ORDER BY distance_meters""", (mapName,))
                results = cursor.fetchall()
                labels = []
                buildings = []
                for row in results: 
                    cursor.execute("""SELECT swisstopo_id
                                    FROM buildings_ids
                                    WHERE building_id = %s""", (row['id'],))
                    buildings_list = []
                    for entry in cursor.fetchall() : buildings_list.append(entry['swisstopo_id']) 
                    buildings += buildings_list
                    east, north, height = utils.gps_to_lv95(row['latitude'], row['longitude'], row['height'])
                    labels.append({ "name": row['name'] ,"distance" : round(row['distance_meters'], 2), "x" : east-x, "y" : height-y, "z" : north-z, "buildings": buildings_list})

                cursor.execute("""SELECT mesh_1, mesh_2, mesh_3 FROM alignment_labels WHERE location_id = %s""", (location_id, ))   
                results = cursor.fetchall()
                alignment_labels = [results[0]['mesh_1'],results[0]['mesh_2'], results[0]['mesh_3'] ] if len(results) else []
                response = {"coordinates" : {"east": x, "north": z, "altitude": y}, "labels" : labels, "buildings": buildings, "visibility": visibility, "alignment_labels": alignment_labels}
            except Exception as e:
                print(e)
                raise HTTPException(status_code=400, detail="Some problem occured, please contact the team!")
        cursor.close()
    connection.close()
    return response


if __name__ == "__main__":
    uvicorn.run(app, host="10.2.0.135", port=8000)
    
