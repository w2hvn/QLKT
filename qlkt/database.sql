DROP DATABASE IF EXISTS MilitaryReward;
CREATE DATABASE MilitaryReward;
USE MilitaryReward;

-- 1. Table for System Users
CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Hoạt động'
);

-- 2. Table for Units
CREATE TABLE Units (
    UnitID INT PRIMARY KEY AUTO_INCREMENT,
    UnitName VARCHAR(255) NOT NULL,
    ParentID INT NULL
);

-- 3. Table for Soldiers (Profiles)
CREATE TABLE Soldiers (
    SoldierID INT PRIMARY KEY AUTO_INCREMENT,
    SoldierCode VARCHAR(50) NOT NULL UNIQUE,
    FullName VARCHAR(100) NOT NULL,
    Gender VARCHAR(10),
    Rank VARCHAR(50),
    Position VARCHAR(100),
    UnitID INT,
    Ethnicity VARCHAR(50),
    Religion VARCHAR(50),
    AcademicLevel VARCHAR(100),
    PoliticalTheory VARCHAR(100),
    SchoolsAttended TEXT,
    WorkHistory TEXT,
    FOREIGN KEY (UnitID) REFERENCES Units(UnitID)
);

-- 4. Table for Reward Categories
CREATE TABLE RewardCategories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL,
    Description TEXT
);

-- 5. Table for Reward Proposals
CREATE TABLE Proposals (
    ProposalID INT PRIMARY KEY AUTO_INCREMENT,
    ProposalCode VARCHAR(50) NOT NULL UNIQUE,
    SoldierID INT NOT NULL,
    CategoryID INT NOT NULL,
    Reason TEXT,
    DateProposed DATE,
    ProposedByUserID INT NOT NULL,
    Status VARCHAR(50) DEFAULT 'Chờ phê duyệt', -- Statuses: 'Chờ phê duyệt', 'Đã phê duyệt', 'Đã từ chối', 'Yêu cầu bổ sung'
    FOREIGN KEY (SoldierID) REFERENCES Soldiers(SoldierID),
    FOREIGN KEY (CategoryID) REFERENCES RewardCategories(CategoryID),
    FOREIGN KEY (ProposedByUserID) REFERENCES Users(UserID)
);

-- 6. Table for Proposal Approvals (History/Tracking)
CREATE TABLE Approvals (
    ApprovalID INT PRIMARY KEY AUTO_INCREMENT,
    ProposalID INT NOT NULL,
    ApproverUserID INT NOT NULL,
    Decision VARCHAR(50) NOT NULL,
    Comment TEXT,
    DateApproved DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProposalID) REFERENCES Proposals(ProposalID),
    FOREIGN KEY (ApproverUserID) REFERENCES Users(UserID)
);

-- 7. Table for Official Reward Decisions
CREATE TABLE Decisions (
    DecisionID INT PRIMARY KEY AUTO_INCREMENT,
    ProposalID INT NOT NULL,
    DecisionNumber VARCHAR(100) NOT NULL UNIQUE,
    DateSigned DATE NOT NULL,
    SignedBy VARCHAR(100) NOT NULL,
    FOREIGN KEY (ProposalID) REFERENCES Proposals(ProposalID)
);

-- =========================================
-- SAMPLE DATA
-- =========================================

-- Sample Users
INSERT INTO Users (UserID, Username, Password, FullName, Role, Status) VALUES
(1, 'admin', 'admin123', 'Quản trị viên Hệ thống', 'Admin', 'Hoạt động'),
(2, 'hungnv', '123456', 'Nguyễn Văn Hùng', 'Quản trị viên', 'Hoạt động'),
(3, 'lantt', '123456', 'Trần Thị Lan', 'Người dùng', 'Hoạt động'),
(4, 'tulh', '123456', 'Lê Hoàng Tú', 'Người dùng', 'Khóa');

-- Sample Units
INSERT INTO Units (UnitID, UnitName, ParentID) VALUES 
(1, 'Sư đoàn 308', NULL),
(2, 'Trung đoàn 102', 1),
(3, 'Tiểu đoàn 4', 2),
(4, 'Đại đội 1', 3),
(5, 'Phòng Tham mưu', 1),
(6, 'Bệnh xá', 1);

-- Sample Soldiers
INSERT INTO Soldiers (SoldierID, SoldierCode, FullName, Gender, `Rank`, Position, UnitID) VALUES
(1, 'SQ001', 'Nguyễn Văn Quyết', 'Nam', 'Thượng tá', 'Trung đoàn trưởng', 2),
(2, 'SQ002', 'Nguyễn Văn A', 'Nam', 'Đại úy', 'Đại đội trưởng', 4),
(3, 'SQ003', 'Trần Thị B', 'Nữ', 'Trung úy', 'Trợ lý', 5),
(4, 'SQ004', 'Lê Văn C', 'Nam', 'Thiếu úy', 'Trung đội trưởng', 4),
(5, 'SQ005', 'Phạm Văn D', 'Nam', 'Trung tá', 'Tiểu đoàn trưởng', 3),
(6, 'SQ006', 'Hoàng Thị E', 'Nữ', 'Thượng úy', 'Y sĩ', 6),
(7, 'SQ007', 'Vũ Văn F', 'Nam', 'Đại úy', 'Chính trị viên', 4);

-- Sample Reward Categories
INSERT INTO RewardCategories (CategoryID, CategoryName) VALUES
(1, 'Huân chương Chiến công'),
(2, 'Bằng khen Bộ Quốc phòng'),
(3, 'Chiến sĩ thi đua'),
(4, 'Giấy khen Lữ đoàn'),
(5, 'Bằng khen Quân khu'),
(6, 'Giấy khen Trung đoàn');

-- Sample Proposals
INSERT INTO Proposals (ProposalID, ProposalCode, SoldierID, CategoryID, Reason, DateProposed, ProposedByUserID, Status) VALUES
(1, 'DX-2023-01', 2, 2, 'Hoàn thành xuất sắc nhiệm vụ diễn tập', '2023-10-15', 3, 'Chờ phê duyệt'),
(2, 'DX-2023-02', 3, 3, 'Đạt giải nhất hội thi chuyên môn', '2023-10-14', 2, 'Đã phê duyệt'),
(3, 'DX-2023-03', 4, 4, 'Thành tích nổi bật trong huấn luyện', '2023-10-12', 3, 'Đã từ chối'),
(4, 'DX-2023-04', 5, 1, 'Lãnh đạo đơn vị xuất sắc', '2023-10-10', 2, 'Chờ phê duyệt'),
(5, 'DX-2023-05', 6, 5, 'Khám chữa bệnh tận tình', '2023-10-08', 3, 'Đã phê duyệt'),
(6, 'DX-2023-06', 7, 3, 'Gương mẫu trong công tác Đảng', '2023-10-05', 2, 'Đã phê duyệt'),
(7, 'DX-2023-07', 1, 6, 'Hoàn thành tốt nhiệm vụ năm', '2023-10-01', 3, 'Chờ phê duyệt');

-- Sample Approvals
INSERT INTO Approvals (ProposalID, ApproverUserID, Decision, Comment, DateApproved) VALUES
(2, 1, 'Phê duyệt', 'Đồng ý với đề xuất. Cần biểu dương kịp thời.', '2023-10-15 08:30:00'),
(3, 1, 'Từ chối', 'Chưa đủ thời gian công tác theo quy định.', '2023-10-13 14:00:00'),
(5, 1, 'Phê duyệt', 'Nhất trí.', '2023-10-09 09:15:00'),
(6, 1, 'Phê duyệt', 'Hoàn toàn xứng đáng.', '2023-10-06 10:45:00');

-- Sample Official Decisions
INSERT INTO Decisions (ProposalID, DecisionNumber, DateSigned, SignedBy) VALUES
(2, 'QD-102/2023', '2023-10-16', 'Tư lệnh Quân khu'),
(5, 'QD-105/2023', '2023-10-10', 'Chính ủy Sư đoàn'),
(6, 'QD-106/2023', '2023-10-07', 'Trung đoàn trưởng');

