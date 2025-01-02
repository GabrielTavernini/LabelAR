# LabelAR Backend Setup
In order to setup LabelAR's backend you need to perform these steps:
## 1. DB 
Install and launch PostgresSQL Server on port `5432`. <br>
Install [PostGIS](https://postgis.net/) extension.
Execute the queries in `label_ar.sql` in order to create and populate the DB.
## 2. FastAPI WebServer
- Create a virtual environment (we use Python 3.11.2). <br>
`python3 -m venv venv`
- Install requirements <br>
`pip install -r requirements.txt`
## 3. Execute
- Activate the python environment <br>
`source venv/bin/activate`
- Execute <br>
    `uvicorn src/main:app --host 10.2.0.135 --port 8000 --reload`
# Additional Notes
In our setup we exposed the webserver behind a reverse proxy, we recommend doing the same. 
