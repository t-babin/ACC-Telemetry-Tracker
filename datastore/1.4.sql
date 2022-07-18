-- Update the AverageLaps table to include the track condition
set @col:= if(
  (SELECT count(column_name) FROM information_schema.columns
  WHERE table_name = 'AverageLaps'
  AND column_name = 'TrackCondition') = 0,
  'ALTER TABLE AverageLaps DROP PRIMARY KEY, ADD COLUMN TrackCondition int, ADD PRIMARY KEY (CarId,TrackId,TrackCondition)',
  'select 1');
PREPARE stmt FROM @col;
EXECUTE stmt;
deallocate prepare stmt;