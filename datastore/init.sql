-- CREATE DATABASE IF NOT EXISTS acc_telemetry_tracker;
-- USE acc_telemetry_tracker;

DROP TABLE IF EXISTS Cars;
CREATE TABLE Cars (
  Id int NOT NULL AUTO_INCREMENT,
  Name text,
  Class text,
  PRIMARY KEY (Id)
);

DROP TABLE IF EXISTS Tracks;
CREATE TABLE Tracks (
  Id int NOT NULL AUTO_INCREMENT,
  Name text,
  PRIMARY KEY (Id)
);

DROP TABLE IF EXISTS Users;
CREATE TABLE Users (
  Id bigint NOT NULL,
  Username text,
  ServerName text,
  IsValid BOOLEAN NOT NULL,
  Role text,
  SignupDate DATETIME NOT NULL,
  PRIMARY KEY (Id)
);

DROP TABLE IF EXISTS MotecFiles;
CREATE TABLE MotecFiles (
  Id int NOT NULL AUTO_INCREMENT,
  CarId int NOT NULL,
  TrackId int NOT NULL,
  UserId bigint NOT NULL,
  DateInserted DATETIME NOT NULL,
  SessionDate DATETIME NOT NULL,
  NumberOfLaps int NOT NULL,
  FastestLap double NOT NULL,
  FileLocation text,
  Comment text,
  PRIMARY KEY (Id),
  KEY IX_MotecFiles_CarId (CarId),
  KEY IX_MotecFiles_TrackId (TrackId),
  KEY IX_MotecFiles_UserId (UserId),
  CONSTRAINT FK_MotecFiles_Cars_CarId FOREIGN KEY (CarId) REFERENCES Cars (Id) ON DELETE CASCADE,
  CONSTRAINT FK_MotecFiles_Tracks_TrackId FOREIGN KEY (TrackId) REFERENCES Tracks (Id) ON DELETE CASCADE,
  CONSTRAINT FK_MotecFiles_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);

DROP TABLE IF EXISTS AverageLaps;
CREATE TABLE AverageLaps (
  CarId int NOT NULL,
  TrackId int NOT NULL,
  AverageFastestLap double NOT NULL,
  PRIMARY KEY (CarId,TrackId),
  KEY IX_AverageLaps_TrackId (TrackId),
  CONSTRAINT FK_AverageLaps_Cars_CarId FOREIGN KEY (CarId) REFERENCES Cars (Id) ON DELETE CASCADE,
  CONSTRAINT FK_AverageLaps_Tracks_TrackId FOREIGN KEY (TrackId) REFERENCES Tracks (Id) ON DELETE CASCADE
);

DROP TABLE IF EXISTS AuditLogs;
CREATE TABLE AuditLogs (
  Id int NOT NULL AUTO_INCREMENT,
  EventDate text NOT NULL,
  EventType int NOT NULL,
  UserId bigint NOT NULL,
  MotecId int DEFAULT NULL,
  Log text,
  PRIMARY KEY (Id),
  KEY IX_AuditLogs_UserId (UserId),
  CONSTRAINT FK_AuditLogs_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);