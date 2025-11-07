--Comment Ctrl + K + C
--Uncomment Ctrl + K + U
--Drop database hiện có

--USE [master];
--GO
--ALTER DATABASE [ManageLibrary] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
--GO
--DROP DATABASE [ManageLibrary];
--GO

CREATE DATABASE ManageLibrary;
GO
USE ManageLibrary;
GO

-- BẢNG NHÂN VIÊN
CREATE TABLE Employees (
    EmployeeId VARCHAR(20) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Telephone NVARCHAR(20),
    Role NVARCHAR(50)
);

-- BẢNG ĐỘC GIẢ
CREATE TABLE Readers (
    ReaderId VARCHAR(20) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE,
    NationalId NVARCHAR(20),
    TypeOfReader NVARCHAR(50),
    Email NVARCHAR(100),
    Telephone NVARCHAR(20),
    Address NVARCHAR(200),
    Department NVARCHAR(100)
);

-- BẢNG TÀI KHOẢN
CREATE TABLE Account (
    AccountId VARCHAR(20) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(100) NOT NULL,
    EmployeeId VARCHAR(20),
    ReaderId VARCHAR(20),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (ReaderId) REFERENCES Readers(ReaderId),
    CONSTRAINT UQ_Account_Reader UNIQUE (ReaderId)
);

-- BẢNG TÁC GIẢ
CREATE TABLE Author (
    AuthorId VARCHAR(20) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- BẢNG NHÀ XUẤT BẢN
CREATE TABLE Publisher (
    PublisherId VARCHAR(20) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200),
    Telephone NVARCHAR(20)
);

-- BẢNG THỂ LOẠI
CREATE TABLE Category (
    CategoryId VARCHAR(20) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- BẢNG SÁCH
CREATE TABLE Books (
    BookId VARCHAR(20) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    YearOfPublic INT,
    Position NVARCHAR(50),
    NumOfPage INT,
    Cost DECIMAL(10,2),
    CategoryId VARCHAR(20),
    AuthorId VARCHAR(20),
    PublisherId VARCHAR(20),
    Quantity INT NOT NULL DEFAULT 0,
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
    FOREIGN KEY (AuthorId) REFERENCES Author(AuthorId),
    FOREIGN KEY (PublisherId) REFERENCES Publisher(PublisherId)
);

-- PHIẾU MƯỢN
CREATE TABLE LoanSlip (
    LoanId VARCHAR(20) PRIMARY KEY,
    ReaderId VARCHAR(20) NOT NULL,
    EmployeeId VARCHAR(20) NOT NULL,
    LoanDate DATE NOT NULL,
    ExpiredDate DATE,
    ReturnDate DATE,
    Status NVARCHAR(50),
    FOREIGN KEY (ReaderId) REFERENCES Readers(ReaderId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);

-- CHI TIẾT MƯỢN
CREATE TABLE LoanDetail (
    LoanDetailId VARCHAR(20) PRIMARY KEY,
    LoanId VARCHAR(20) NOT NULL,
    BookId VARCHAR(20) NOT NULL,
    LoanStatus NVARCHAR(50),
    ReturnStatus NVARCHAR(50),
    IsLose BIT DEFAULT 0,
    Fine DECIMAL(10,2) DEFAULT 0,
    FOREIGN KEY (LoanId) REFERENCES LoanSlip(LoanId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);

-- NHIỀU TÁC GIẢ
CREATE TABLE BookAuthor (
    BookId VARCHAR(20),
    AuthorId VARCHAR(20),
    PRIMARY KEY (BookId, AuthorId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId),
    FOREIGN KEY (AuthorId) REFERENCES Author(AuthorId)
);

-- =========================
-- DỮ LIỆU MẪU
-- =========================
INSERT INTO Employees (EmployeeId, FullName, Role)
VALUES ('NV001', N'Admin Quản Trị', N'Quản lý');

INSERT INTO Account (AccountId, Username, Password, EmployeeId)
VALUES ('TK001', N'admin', N'123', 'NV001');

-- CATEGORY
INSERT INTO Category (CategoryId, Name)
VALUES (N'TL001', N'Công nghệ thông tin'),
       (N'TL002', N'Mạng và bảo mật'),
       (N'TL003', N'Cơ sở dữ liệu');

-- AUTHOR
INSERT INTO Author (AuthorId, Name)
VALUES (N'TG001', N'Nguyễn Văn A'),
       (N'TG002', N'Trần Thị B'),
       (N'TG003', N'Lê Văn C'),
       (N'TG004', N'Phạm Minh D'),
       (N'TG005', N'Vũ Thị E'),
       (N'TG006', N'Đặng Tiến F');

-- PUBLISHER
INSERT INTO Publisher (PublisherId, Name, Address, Telephone)
VALUES (N'NXB001', N'Nhà xuất bản Giáo dục', N'Hà Nội', N'0241234567'),
       (N'NXB002', N'Nhà xuất bản Trẻ', N'TP.HCM', N'0287654321'),
       (N'NXB003', N'Nhà xuất bản Khoa học', N'Đà Nẵng', N'0236123456');

-- BOOKS
INSERT INTO Books (BookId, Name, YearOfPublic, Position, NumOfPage, Cost, CategoryId, AuthorId, PublisherId, Quantity)
VALUES 
('S001', N'Lập trình C# cơ bản', 2020, N'A1', 350, 100000, 'TL001', 'TG001', 'NXB001', 10),
('S002', N'Lập trình Java nâng cao', 2021, N'B2', 420, 120000, 'TL001', 'TG002', 'NXB002', 8),
('S003', N'Thiết kế web với HTML, CSS', 2022, N'C1', 320, 90000, 'TL002', 'TG003', 'NXB003', 15),
('S004', N'Khoa học dữ liệu với Python', 2023, N'A3', 500, 150000, 'TL001', 'TG004', 'NXB001', 7),
('S005', N'Cơ sở dữ liệu SQL', 2019, N'B1', 380, 85000, 'TL003', 'TG001', 'NXB002', 12);
-- READERS
INSERT INTO Readers (ReaderId, FullName, DateOfBirth, NationalId, TypeOfReader, Email, Telephone, Address, Department)
VALUES
('DG001', N'Nguyễn Văn Hùng', '2000-05-10', '123456789', N'Sinh viên', 'hungnv@example.com', '0901234567', N'123 Nguyễn Trãi, Hà Nội', N'Công nghệ thông tin'),
('DG002', N'Trần Thị Lan', '1999-08-22', '987654321', N'Sinh viên', 'lantr@example.com', '0912345678', N'45 Hai Bà Trưng, Hà Nội', N'Kinh tế'),
('DG003', N'Lê Minh Tuấn', '1998-12-01', '192837465', N'Cựu sinh viên', 'tuanlm@example.com', '0934567890', N'67 Láng Hạ, Hà Nội', N'Quản trị kinh doanh'),
('DG004', N'Phạm Hồng Nhung', '2001-03-18', '564738291', N'Sinh viên', 'nhungph@example.com', '0945678901', N'89 Cầu Giấy, Hà Nội', N'Sư phạm'),
('DG005', N'Hoàng Văn Tài', '1997-11-05', '837465920', N'Giảng viên', 'taihv@example.com', '0956789012', N'12 Kim Mã, Hà Nội', N'Công nghệ thông tin'),
('DG006', N'Vũ Thị Mai', '2000-02-14', '475839201', N'Sinh viên', 'maivt@example.com', '0967890123', N'34 Lê Lợi, TP.HCM', N'Ngôn ngữ Anh'),
('DG007', N'Ngô Đức Duy', '1999-09-25', '829104756', N'Sinh viên', 'duynd@example.com', '0978901234', N'90 Nguyễn Huệ, Đà Nẵng', N'Kỹ thuật phần mềm'),
('DG008', N'Đặng Thị Hòa', '1998-04-30', '910283746', N'Cựu sinh viên', 'hoadt@example.com', '0989012345', N'56 Phan Đình Phùng, Hải Phòng', N'Tài chính - Ngân hàng'),
('DG009', N'Bùi Quốc Khánh', '2001-06-12', '384756920', N'Sinh viên', 'khanhbq@example.com', '0990123456', N'23 Tôn Đức Thắng, Cần Thơ', N'Luật học'),
('DG010', N'Nguyễn Thị Yến', '2000-01-20', '564738920', N'Sinh viên', 'yentn@example.com', '0902345678', N'101 Phạm Văn Đồng, Hà Nội', N'Thiết kế đồ họa');

-- =========================
-- PHIẾU MƯỢN/TRẢ MẪU
-- =========================
-- Các trạng thái sử dụng: 'Đang mượn', 'Đã trả', 'Quá hạn'
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
('PM001', 'DG001', 'NV001', '2024-10-01', '2024-10-15', '2024-10-10', N'Đã trả'),
('PM002', 'DG002', 'NV001', '2024-10-20', '2024-10-27', NULL,             N'Đang mượn'),
('PM003', 'DG003', 'NV001', '2024-09-25', '2024-10-02', NULL,             N'Quá hạn'),
('PM004', 'DG004', 'NV001', '2024-10-15', '2024-10-29', '2024-11-01',     N'Đã trả');

-- CHI TIẾT PHIẾU MƯỢN/TRẢ
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
-- PM001: Đã trả, sách tốt
('CT001', 'PM001', 'S001', N'Mới', N'Tốt', 0, 0),
('CT002', 'PM001', 'S003', N'Mới', N'Tốt', 0, 0),
-- PM002: Đang mượn (chưa trả)
('CT003', 'PM002', 'S002', N'Mới', NULL, 0, 0),
('CT004', 'PM002', 'S004', N'Mới', NULL, 0, 0),
-- PM003: Quá hạn (chưa trả)
('CT005', 'PM003', 'S005', N'Bình thường', NULL, 0, 0),
-- PM004: Đã trả, 1 cuốn mất, có phạt
('CT006', 'PM004', 'S002', N'Bình thường', N'Hư hỏng nặng', 1, 100000),
('CT007', 'PM004', 'S001', N'Bình thường', N'Tốt', 0, 0);

PRINT N'Cơ sở dữ liệu đã khởi tạo và thêm dữ liệu mẫu thành công!';
GO
