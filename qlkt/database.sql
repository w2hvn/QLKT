DROP DATABASE IF EXISTS MilitaryReward;
CREATE DATABASE MilitaryReward;
USE MilitaryReward;

CREATE TABLE Units (
    UnitID INT PRIMARY KEY AUTO_INCREMENT,
    UnitName VARCHAR(255),
    ParentID INT
);

CREATE TABLE Soldiers (
    SoldierID INT PRIMARY KEY AUTO_INCREMENT,
    SoldierCode VARCHAR(50),
    FullName VARCHAR(100),
    Gender VARCHAR(10),
    Rank VARCHAR(50),
    Position VARCHAR(100),
    UnitID INT,
    Ethnicity VARCHAR(50),
    Religion VARCHAR(50),
    AcademicLevel VARCHAR(100),
    PoliticalTheory VARCHAR(100),
    SchoolsAttended TEXT,
    WorkHistory TEXT
);

CREATE TABLE Rewards (
    RewardID INT PRIMARY KEY AUTO_INCREMENT,
    SoldierID INT,
    DecisionNumber VARCHAR(100),
    RewardType VARCHAR(100),
    Reason TEXT,
    DateSigned DATE
);

CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50),
    Password VARCHAR(255),
    FullName VARCHAR(100),
    Role VARCHAR(20)
);

-- Sample Data
INSERT INTO Users (UserID, Username, Password, FullName, Role) VALUES (1, 'admin', 'admin123', 'Quản trị viên', 'Admin');

INSERT INTO Units (UnitID, UnitName, ParentID) VALUES 
(1, 'Sư đoàn 308', NULL),
(2, 'Trung đoàn 102', 1),
(3, 'Tiểu đoàn 4', 2),
(4, 'Đại đội 1', 3);

INSERT INTO Soldiers (SoldierID, SoldierCode, FullName, Gender, `Rank`, Position, UnitID, Ethnicity, Religion, AcademicLevel, PoliticalTheory, SchoolsAttended, WorkHistory) 
VALUES (1, 'SQ001', 'Nguyễn Văn Quyết', 'Nam', 'Thượng tá', 'Trung đoàn trưởng', 2, 'Kinh', 'Không', 'Thạc sĩ', 'Cao cấp', 'Học viện Lục quân', 'Tham gia chiến dịch X');
