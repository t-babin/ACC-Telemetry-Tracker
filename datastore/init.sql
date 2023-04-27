-- CREATE DATABASE IF NOT EXISTS acc_telemetry_tracker;
-- USE acc_telemetry_tracker;

DROP TABLE IF EXISTS Cars;
CREATE TABLE Cars (
  Id int NOT NULL AUTO_INCREMENT,
  Name text,
  MotecName text,
  Class text,
  PRIMARY KEY (Id)
);

DROP TABLE IF EXISTS Tracks;
CREATE TABLE Tracks (
  Id int NOT NULL AUTO_INCREMENT,
  Name text,
  MotecName text,
  MinLaptime int,
  MaxLaptime int,
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
  TrackCondition int,
  GameVersion text,
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
  TrackCondition int,
  PRIMARY KEY (CarId,TrackId,TrackCondition),
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

DROP TABLE IF EXISTS GameVersions;
CREATE TABLE GameVersions (
  Id int NOT NULL AUTO_INCREMENT,
  StartDate text NOT NULL,
  EndDate text NOT NULL,
  VersionNumber text NOT NULL,
  PRIMARY KEY (Id)
);

INSERT INTO GameVersions (StartDate, EndDate, VersionNumber)
VALUES
('2010-01-01 00:00:00', '2021-06-29 23:59:59', 'pre-1.7.12'),
('2021-06-30 00:00:00', '2021-08-22 23:59:59', '1.7.12'),
('2021-08-23 00:00:00', '2021-09-08 23:59:59', '1.7.13'),
('2021-09-09 00:00:00', '2021-10-06 23:59:59', '1.7.14'),
('2021-10-07 00:00:00', '2021-11-23 23:59:59', '1.7.15'),
('2021-11-24 00:00:00', '2021-11-24 23:59:59', '1.8.0' ),
('2021-11-25 00:00:00', '2021-11-25 23:59:59', '1.8.1' ),
('2021-11-26 00:00:00', '2021-12-01 23:59:59', '1.8.2' ),
('2021-12-02 00:00:00', '2021-12-02 23:59:59', '1.8.5' ),
('2021-12-03 00:00:00', '2021-12-08 23:59:59', '1.8.6' ),
('2021-12-09 00:00:00', '2021-12-14 23:59:59', '1.8.7' ),
('2021-12-15 00:00:00', '2022-01-06 23:59:59', '1.8.8' ),
('2022-01-07 00:00:00', '2022-01-17 23:59:59', '1.8.9' ),
('2022-01-18 00:00:00', '2022-02-06 23:59:59', '1.8.10'),
('2022-02-07 00:00:00', '2022-03-22 23:59:59', '1.8.11'),
('2022-03-23 00:00:00', '2022-04-01 23:59:59', '1.8.12'),
('2022-04-02 00:00:00', '2022-04-27 23:59:59', '1.8.13'),
('2022-04-28 00:00:00', '2022-06-29 23:59:59', '1.8.14'),
('2022-06-30 00:00:00', '2022-07-05 23:59:59', '1.8.15'),
('2022-07-06 00:00:00', '2022-07-12 23:59:59', '1.8.16'),
('2022-07-13 00:00:00', '2022-08-21 23:59:59', '1.8.17'),
('2022-08-22 00:00:00', '2022-11-15 23:59:59', '1.8.18'),
('2022-11-16 00:00:00', '2022-12-26 23:59:59', '1.8.19'),
('2022-12-27 00:00:00', '2023-01-16 23:59:59', '1.8.20'),
('2023-01-17 00:00:00', '2023-04-18 23:59:59', '1.8.21'),
('2023-04-19 00:00:00', '2023-04-24 23:59:59', '1.9.0'),
('2023-04-25 00:00:00', null, '1.9.1');

INSERT INTO Tracks (MotecName, Name, MinLapTime, MaxLapTime)
VALUES
('barcelona', 'Barcelona', 100, 170),
('brands_hatch', 'Brands Hatch', 80, 160),
('cota', 'Circuit of the Americas', 120, 200),
('donington', 'Donington', 82, 160),
('hungaroring', 'Hungaroring', 100, 170),
('imola', 'Imola', 99, 165),
('indianapolis', 'Indianapolis', 96, 170),
('kyalami', 'Kyalami', 93, 160),
('laguna_seca', 'Laguna Seca', 78, 160),
('misano', 'Misano', 90, 170),
('monza', 'Monza', 103, 170),
('mount_panorama', 'Mount Panorama', 115, 185),
('nurburgring', 'Nurburgring', 110, 180),
('paul_ricard', 'Paul Ricard', 90, 175),
('snetterton', 'Snetterton', 100, 180),
('spa', 'Spa-Francorchamps', 130, 210),
('silverstone', 'Silverstone', 113, 180),
('oulton_park', 'Oulton Park', 90, 170),
('suzuka', 'Suzuka', 115, 180),
('valencia', 'Valencia', 115, 180),
('watkins_glen', 'Watkins Glen', 101, 170),
('zandvoort', 'Zandvoort', 92, 170),
('zolder', 'Zolder', 85, 165);

-- delete from cars;
INSERT INTO cars (Name, MotecName, Class)
VALUES
('Aston Martin V12 Vantage', 'V12 Vantage GT3', 'GT3'),
('Aston Martin V8 Vantage', 'V8 Vantage GT3', 'GT3'),
('Audi R8 LMS', 'R8 LMS', 'GT3'),
('Audi R8 LMS EVO', 'R8 LMS EVO', 'GT3'),
('Audi R8 LMS EVO II', 'R8 LMS EVO II', 'GT3'),
('Bentley Continental 2015', 'Continental 16', 'GT3'),
('Bentley Continental 2018', 'Continental 18', 'GT3'),
('BMW M4', 'M4 GT3', 'GT3'),
('BMW M6', 'M6 GT3', 'GT3'),
('Jaguar', 'Emil Frey G3', 'GT3'),
('Ferrari 488 GT3', '488 GT3', 'GT3'),
('Ferrari 488 GT3 EVO', '488 GT3 Evo', 'GT3'),
('Ferrari 296 GT3', '296 GT3', 'GT3'),
('Honda NSX GT3', 'NSX GT3', 'GT3'),
('Honda NSX GT3 EVO', 'NSX GT3 EVO', 'GT3'),
('Lamborghini Huracan GT3', 'Huracan GT3', 'GT3'),
('Lamborghini Huracan GT3 EVO', 'Huracan GT3 EVO', 'GT3'),
('Lamborghini Huracan GT3 EVO II', 'Huracan GT3 EVO II', 'GT3'),
('Lexus RC F GT3', 'RC F GT3', 'GT3'),
('McLaren 650S GT3', 'McLaren 650S GT3', 'GT3'),
('McLaren 720S GT3', 'McLaren 720S GT3', 'GT3'),
('Mercedes AMG GT3', 'AMG GT3', 'GT3'),
('Mercedes AMG GT3 EVO', 'AMG GT3 Evo', 'GT3'),
('Nissam GT-R Nismo GT3 2015', 'GTR GT3 17', 'GT3'),
('Nissam GT-R Nismo GT3 2018', 'GTR GT3 18', 'GT3'),
('Porsche 991 GT3-R', '991 GT3 R 2016', 'GT3'),
('Porsche 991 II GT3-R', '991ii GT3 R EVO', 'GT3'),
('Porsche 992 GT3-R', '992 GT3 R', 'GT3'),
('Lamborghini Reiter Engineering R-EX GT3', 'Gallardo REX', 'GT3'),
('Alpine A110', 'A110 GT4', 'GT4'),
('Aston Martin Vantage GT4', 'V8 Vantage GT4', 'GT4'),
('Audi R8 LMS GT4', 'R8 GT4', 'GT4'),
('BMW M4 GT4', 'M4 GT4', 'GT4'),
('Chevrolet Camaro GT4', 'Camaro GT4R', 'GT4'),
('Ginetta G55 GT4', 'G55 GT4', 'GT4'),
('KTM X-Bow GT4', 'XBOW GT4', 'GT4'),
('Maserati MC GT4', 'GranTurismo MC GT4', 'GT4'),
('McLaren 570S GT4', '570S GT4', 'GT4'),
('Mercedes AMG GT4', 'AMG GT4', 'GT4'),
('Porsche 718 Cayman GT4', '718 Cayman GT4 MR', 'GT4'),
('Porsche 991 II GT3 Cup', '991II GT3 Cup', 'Cup'),
('Porsche 992 GT3 Cup', '992 GT3 Cup', 'Cup'),
('Lamborghini Huracan SuperTrofeo', 'Huracan ST', 'ST'),
('Lamborghini Huracan SuperTrofeo EVO II', 'Huracan ST Evo2', 'ST'),
('Ferrari 488 Challenge Evo', '488 Challenge Evo', 'CHL'),
('BMW M2 Club Sport Racing', 'M2 CS Racing', 'TCX');