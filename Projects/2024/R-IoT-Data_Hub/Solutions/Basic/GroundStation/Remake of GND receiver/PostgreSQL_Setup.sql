-- PostgreSQL setup for CanSat Ground Station Receiver
-- Create schema
CREATE SCHEMA IF NOT EXISTS telemetry;

-- Create measurements table
CREATE TABLE IF NOT EXISTS telemetry.measurements (
    id SERIAL PRIMARY KEY,
    devid INTEGER NOT NULL,
    temperature DOUBLE PRECISION,
    humidity DOUBLE PRECISION CHECK (humidity IS NULL OR (humidity >= 0 AND humidity <= 100)),
    pressure DOUBLE PRECISION,
    distance INTEGER,
    counter INTEGER,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    altitude DOUBLE PRECISION,
    launch_servo_open BOOLEAN
);

-- Optional: Indexes for faster queries
CREATE INDEX IF NOT EXISTS idx_measurements_devid ON telemetry.measurements(devid);
CREATE INDEX IF NOT EXISTS idx_measurements_counter ON telemetry.measurements(counter);

-- Example: Grant privileges (adjust as needed)
-- GRANT ALL ON SCHEMA telemetry TO your_user;
-- GRANT ALL ON ALL TABLES IN SCHEMA telemetry TO your_user;

-- You can now use the receiver application to insert telemetry data into this table.
