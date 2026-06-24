-- Run this in pgAdmin Query Tool if you use local PostgreSQL (Option B)
-- and the database does not exist yet.

CREATE DATABASE infinite_journey_saas
    WITH ENCODING = 'UTF8'
         LC_COLLATE = 'en_US.utf8'
         LC_CTYPE = 'en_US.utf8'
         TEMPLATE = template0;

-- Then start the API — EF Core migrations create all tables automatically.
