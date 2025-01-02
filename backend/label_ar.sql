-- Adminer 4.8.1 PostgreSQL 15.8 (Debian 15.8-0+deb12u1) dump

\connect "label_ar";

DROP TABLE IF EXISTS "alignment_labels";
CREATE TABLE "public"."alignment_labels" (
    "location_id" integer NOT NULL,
    "mesh_1" character varying(20),
    "mesh_2" character varying(20),
    "mesh_3" character varying(20)
) WITH (oids = false);

INSERT INTO "alignment_labels" ("location_id", "mesh_1", "mesh_2", "mesh_3") VALUES
(5,	'176C66',	'183530 ',	'8A55'),
(4,	'2DD6D0',	'2FEDF5',	'5BEA1'),
(0,	'8E003',	'15D5C5',	'2FEDF5'),
(3,	'2CCEF',	'9D3DD',	'201C2D'),
(1,	'E7E7',	'E63CB',	'82F35');

DROP TABLE IF EXISTS "buildings_ids";
CREATE TABLE "public"."buildings_ids" (
    "building_id" integer NOT NULL,
    "swisstopo_id" character varying(20) NOT NULL
) WITH (oids = false);


DROP TABLE IF EXISTS "coordinates";
DROP SEQUENCE IF EXISTS coordinates_id_seq;
CREATE SEQUENCE coordinates_id_seq INCREMENT 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1;

CREATE TABLE "public"."coordinates" (
    "id" integer DEFAULT nextval('coordinates_id_seq') NOT NULL,
    "name" character varying(100) NOT NULL,
    "landmark" geography(Point,4326),
    "height" real,
    CONSTRAINT "coordinates_name_key" UNIQUE ("name"),
    CONSTRAINT "coordinates_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


DROP TABLE IF EXISTS "locations";
CREATE TABLE "public"."locations" (
    "id" integer NOT NULL,
    "landmark" geography(Point,4326) NOT NULL,
    "name" character varying(50) NOT NULL,
    "height" real,
    CONSTRAINT "locations_id" PRIMARY KEY ("id")
) WITH (oids = false);


DROP TABLE IF EXISTS "spatial_ref_sys";
CREATE TABLE "public"."spatial_ref_sys" (
    "srid" integer NOT NULL,
    "auth_name" character varying(256),
    "auth_srid" integer,
    "srtext" character varying(2048),
    "proj4text" character varying(2048),
    CONSTRAINT "spatial_ref_sys_pkey" PRIMARY KEY ("srid")
) WITH (oids = false);


ALTER TABLE ONLY "public"."alignment_labels" ADD CONSTRAINT "alignment_labels_location_id_fkey" FOREIGN KEY (location_id) REFERENCES locations(id) ON UPDATE CASCADE ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."buildings_ids" ADD CONSTRAINT "buildings_ids_building_id_fkey" FOREIGN KEY (building_id) REFERENCES coordinates(id) ON UPDATE CASCADE ON DELETE CASCADE NOT DEFERRABLE;

-- 2024-12-28 07:20:11.064229+00
