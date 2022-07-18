USE acc_telemetry_tracker;
-- remove the MotecFile foreign key on the auditlogs table
set @fk := if(
    (SELECT true FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
    AND TABLE_NAME = 'AuditLogs'
    AND CONSTRAINT_NAME = 'FK_AuditLogs_MotecFiles_MotecId'
    AND CONSTRAINT_TYPE = 'FOREIGN KEY') = true,
  'ALTER TABLE AuditLogs drop foreign key FK_AuditLogs_MotecFiles_MotecId',
  'select 1');

PREPARE stmt FROM @fk;
EXECUTE stmt;
deallocate prepare stmt;

-- Remove the MotecFile ID index on the AuditLogs table
set @ix := if(
  (SELECT true FROM information_schema.statistics
  WHERE table_name = 'AuditLogs'
  AND index_name = 'IX_AuditLogs_MotecId'
  AND table_schema = DATABASE()) = true,
  'DROP INDEX `IX_AuditLogs_MotecId` ON AuditLogs',
  'select 1;');
PREPARE stmt FROM @ix;
EXECUTE stmt;
deallocate prepare stmt;


-- Add the Comment column to the MotecFiles table
set @col:= if(
  (SELECT count(column_name) FROM information_schema.columns
  WHERE table_name = 'MotecFiles'
  AND column_name = 'Comment') = 0,
  'ALTER TABLE MotecFiles ADD COLUMN Comment TEXT',
  'select 1');
PREPARE stmt FROM @col;
EXECUTE stmt;
deallocate prepare stmt;