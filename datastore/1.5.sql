USE acc_telemetry_tracker;
-- Add the GameVersion column to the MotecFiles table
set @col:= if(
  (SELECT count(column_name) FROM information_schema.columns
  WHERE table_name = 'MotecFiles'
  AND column_name = 'GameVersion') = 0,
  'ALTER TABLE MotecFiles ADD COLUMN GameVersion TEXT',
  'select 1');
PREPARE stmt FROM @col;
EXECUTE stmt;
deallocate prepare stmt;