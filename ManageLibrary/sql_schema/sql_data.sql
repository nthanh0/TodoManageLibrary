IF DB_ID('ManageLibrary') IS NULL
    CREATE DATABASE ManageLibrary;
GO
USE ManageLibrary;
GO

-- ==============================================
-- BẢNG NHÂN VIÊN
-- ==============================================
CREATE TABLE Employees (
    EmployeeId VARCHAR(20) PRIMARY KEY,              -- Mã nhân viên (VD: NV001)
    FullName NVARCHAR(100) NOT NULL,                 -- Họ tên nhân viên
    Email NVARCHAR(100),                             -- Email
    Telephone NVARCHAR(20),                          -- Số điện thoại
    Role NVARCHAR(50)                                -- Chức vụ (thủ thư, quản lý, ...)
);

-- ==============================================
-- BẢNG ĐỘC GIẢ
-- ==============================================
CREATE TABLE Readers (
    ReaderId VARCHAR(20) PRIMARY KEY,                -- Mã độc giả (VD: DG001)
    FullName NVARCHAR(100) NOT NULL,                 -- Họ tên
    DateOfBirth DATE,                                -- Ngày sinh
    NationalId NVARCHAR(20),                         -- CCCD/CMND
    TypeOfReader NVARCHAR(50),                       -- Loại độc giả (Sinh viên, Giảng viên,...)
    Email NVARCHAR(100),
    Telephone NVARCHAR(20),
    Address NVARCHAR(200),
    Department NVARCHAR(100)                         -- Khoa / Phòng ban
);

-- ==============================================
-- BẢNG TÀI KHOẢN
-- 1-1 với Reader, 1-n với Employee
-- ==============================================
CREATE TABLE Account (
    AccountId VARCHAR(20) PRIMARY KEY,               -- Mã tài khoản (VD: TK001)
    Username NVARCHAR(50) UNIQUE NOT NULL,           -- Tên đăng nhập
    Password NVARCHAR(100) NOT NULL,                 -- Mật khẩu
    EmployeeId VARCHAR(20) NULL,                     -- Liên kết nhân viên (nếu là tài khoản nhân viên)
    ReaderId VARCHAR(20) NULL,                       -- Liên kết độc giả (nếu là tài khoản độc giả)
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (ReaderId) REFERENCES Readers(ReaderId),
    CONSTRAINT UQ_Account_Reader UNIQUE (ReaderId)   -- Mỗi độc giả chỉ có 1 tài khoản
);

-- ==============================================
-- BẢNG TÁC GIẢ
-- ==============================================
CREATE TABLE Author (
    AuthorId VARCHAR(20) PRIMARY KEY,                -- Mã tác giả (VD: TG001)
    Name NVARCHAR(100) NOT NULL                      -- Tên tác giả
);

-- ==============================================
-- BẢNG NHÀ XUẤT BẢN
-- ==============================================
CREATE TABLE Publisher (
    PublisherId VARCHAR(20) PRIMARY KEY,             -- Mã NXB (VD: NXB001)
    Name NVARCHAR(100) NOT NULL,                     -- Tên nhà xuất bản
    Address NVARCHAR(200),                           -- Địa chỉ
    Telephone NVARCHAR(20)                           -- SĐT
);

-- ==============================================
-- BẢNG THỂ LOẠI
-- ==============================================
CREATE TABLE Category (
    CategoryId VARCHAR(20) PRIMARY KEY,              -- Mã thể loại (VD: TL001)
    Name NVARCHAR(100) NOT NULL                      -- Tên thể loại (CNTT, Văn học,...)
);

-- ==============================================
-- BẢNG SÁCH
-- ==============================================
CREATE TABLE Books (
    BookId VARCHAR(20) PRIMARY KEY,                  -- Mã sách (VD: S001)
    Name NVARCHAR(200) NOT NULL,                     -- Tên sách
    YearOfPublic INT,                                -- Năm xuất bản
    Position NVARCHAR(50),                           -- Vị trí trên kệ
    NumOfPage INT,                                   -- Số trang
    Cost DECIMAL(10,2),                              -- Giá
    CategoryId VARCHAR(20),                          -- Thể loại
    AuthorId VARCHAR(20),                            -- Tác giả
    PublisherId VARCHAR(20),                         -- Nhà xuất bản
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
    FOREIGN KEY (AuthorId) REFERENCES Author(AuthorId),
    FOREIGN KEY (PublisherId) REFERENCES Publisher(PublisherId)
);

-- ==============================================
-- BẢNG PHIẾU MƯỢN
-- ==============================================
CREATE TABLE LoanSlip (
    LoanId VARCHAR(20) PRIMARY KEY,                  -- Mã phiếu mượn (VD: PM001)
    ReaderId VARCHAR(20) NOT NULL,                   -- Mã độc giả mượn
    EmployeeId VARCHAR(20) NOT NULL,                 -- Mã nhân viên lập phiếu
    LoanDate DATE NOT NULL,                          -- Ngày mượn
    ExpiredDate DATE,                                -- Ngày hết hạn
    ReturnDate DATE,                                 -- Ngày trả
    Status NVARCHAR(50),                             -- Trạng thái (Đang mượn, Đã trả,...)
    FOREIGN KEY (ReaderId) REFERENCES Readers(ReaderId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);

-- ==============================================
-- BẢNG CHI TIẾT MƯỢN
-- ==============================================
CREATE TABLE LoanDetail (
    LoanDetailId VARCHAR(20) PRIMARY KEY,            -- Mã chi tiết mượn (VD: CT001)
    LoanId VARCHAR(20) NOT NULL,                     -- Mã phiếu mượn
    BookId VARCHAR(20) NOT NULL,                     -- Mã sách
    LoanStatus NVARCHAR(50),                         -- Trạng thái khi mượn (Bình thường, Hư,...)
    ReturnStatus NVARCHAR(50),                       -- Trạng thái khi trả
    IsLose BIT DEFAULT 0,                            -- Có mất không (0 = Không, 1 = Có)
    Fine DECIMAL(10,2) DEFAULT 0,                    -- Tiền phạt
    FOREIGN KEY (LoanId) REFERENCES LoanSlip(LoanId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);
-- ==============================================
-- BẢNG SÁCH - NHIỀU TÁC GIẢ (TÙY CHỌN)
-- ==============================================
CREATE TABLE BookAuthor (
    BookId VARCHAR(20),                              -- Mã sách
    AuthorId VARCHAR(20),                            -- Mã tác giả
    PRIMARY KEY (BookId, AuthorId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId),
    FOREIGN KEY (AuthorId) REFERENCES Author(AuthorId)
);


-- 1. Thêm một Nhân viên mẫu (vì tài khoản Admin cần liên kết với Employee)
INSERT INTO Employees (EmployeeId, FullName, Role)
VALUES ('NV001', N'Admin Quản Trị', N'Quản lý');
GO

-- 2. Thêm một Tài khoản Admin liên kết với nhân viên 'NV001'
-- TÀI KHOẢN: admin
-- MẬT KHẨU: 123 (văn bản thuần, khớp với code đăng nhập hiện tại)
INSERT INTO Account (AccountId, Username, Password, EmployeeId, ReaderId)
VALUES ('TK001', N'admin', N'123', 'NV001', NULL);
GO

-- 3. (Tùy chọn) Thêm một Độc giả mẫu
INSERT INTO Readers (ReaderId, FullName, TypeOfReader)
VALUES ('DG001', N'Nguyễn Văn A', N'Sinh viên');
GO

-- 4. (Tùy chọn) Thêm một Tài khoản Độc giả liên kết với 'DG001'
-- TÀI KHOẢN: nguyenvana
-- MẬT KHẨU: 123
INSERT INTO Account (AccountId, Username, Password, EmployeeId, ReaderId)
VALUES ('TK002', N'nguyenvana', N'123', NULL, 'DG001');
GO

-- Bổ sung dữ liệu cha trước khi thêm Sách để tránh lỗi khóa ngoại
-- Thêm dữ liệu vào bảng Category
INSERT INTO Category (CategoryId, Name) 
VALUES 
(N'TL001', N'Công nghệ thông tin'), 
(N'TL002', N'Mạng và bảo mật'), 
(N'TL003', N'Cơ sở dữ liệu');

-- Thêm dữ liệu vào bảng Author
INSERT INTO Author (AuthorId, Name)
VALUES
(N'TG001', N'Nguyễn Văn A'),
(N'TG002', N'Trần Thị B'),
(N'TG003', N'Lê Văn C'),
(N'TG004', N'Phạm Minh D'),
(N'TG005', N'Vũ Thị E'),
(N'TG006', N'Đặng Tiến F');

-- Thêm dữ liệu vào bảng Publisher
INSERT INTO Publisher (PublisherId, Name, Address, Telephone)
VALUES
(N'NXB001', N'Nhà xuất bản Giáo dục', N'Hà Nội', N'0241234567'),
(N'NXB002', N'Nhà xuất bản Trẻ', N'TP.HCM', N'0287654321'),
(N'NXB003', N'Nhà xuất bản Khoa học', N'Đà Nẵng', N'0236123456');

PRINT N'Đã thêm dữ liệu mẫu thành công!';

-- Thêm dữ liệu vào bảng Books (đã có Category/Author/Publisher)
INSERT INTO Books (BookId, Name, YearOfPublic, Position, NumOfPage, Cost, CategoryId, AuthorId, PublisherId)
VALUES 
(N'S001', N'Lập trình C# cơ bản', 2020, N'A1', 350, 100000, N'TL001', N'TG001', N'NXB001'),
(N'S002', N'Lập trình Java nâng cao', 2021, N'B2', 420, 120000, N'TL001', N'TG002', N'NXB002'),
(N'S003', N'Thiết kế web với HTML, CSS', 2022, N'C1', 320, 90000, N'TL002', N'TG003', N'NXB003'),
(N'S004', N'Khoa học dữ liệu với Python', 2023, N'A3', 500, 150000, N'TL001', N'TG004', N'NXB001'),
(N'S005', N'Cơ sở dữ liệu SQL', 2019, N'B1', 380, 85000, N'TL003', N'TG001', N'NXB002'),
(N'S006', N'An toàn mạng máy tính', 2021, N'C2', 450, 135000, N'TL002', N'TG005', N'NXB003'),
(N'S007', N'Giới thiệu về Trí tuệ nhân tạo', 2022, N'D1', 330, 110000, N'TL001', N'TG006', N'NXB001');

-- Thêm dữ liệu độc giả 
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG123456789', N'Trần Văn An', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG111222333', N'Nguyễn Thị Bình', N'Sinh viên', N'Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG444555666', N'Lê Văn Cường', N'Sinh viên', N'Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG777888999', N'Phạm Thị Dung', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG101101101', N'Hoàng Văn Giang', N'Sinh viên', N'Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG202202202', N'Vũ Thị Hương', N'Sinh viên', N'Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG303303303', N'Đặng Văn Khánh', N'Sinh viên', N'Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG404404404', N'Bùi Thị Lan', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG505505505', N'Hồ Văn Minh', N'Sinh viên', N'Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG606606606', N'Ngô Thị Nga', N'Sinh viên', N'Tài chính ngân hàng');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG707707707', N'Dương Văn Phúc', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG808808808', N'Mai Thị Quyên', N'Sinh viên', N'Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG909909909', N'Lý Văn Sơn', N'Sinh viên', N'Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG010010010', N'Trịnh Thị Thảo', N'Sinh viên', N'Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG012012012', N'Phan Văn Toàn', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG013013013', N'Đoàn Thị Uyên', N'Sinh viên', N'Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG014014014', N'Lâm Văn Vĩ', N'Sinh viên', N'Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG015015015', N'Châu Thị Xuân', N'Sinh viên', N'Tài chính ngân hàng');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG016016016', N'Vương Văn Yến', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG017017017', N'Tô Văn Hùng', N'Sinh viên', N'Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG018018018', N'Nguyễn Bảo Nam', N'Sinh viên', N'Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG019019019', N'Lê Kim Chi', N'Sinh viên', N'Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG020020020', N'Hà Tuấn Kiệt', N'Sinh viên', N'Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG021021021', N'Đỗ Phương Anh', N'Sinh viên', N'Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG022022022', N'Huỳnh Gia Huy', N'Sinh viên', N'Công nghệ thông tin');

-- ==============================================
-- 25 GIẢNG VIÊN
-- ==============================================

INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG987654321', N'Nguyễn Văn Thành', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG888777666', N'Trần Thị Thu Hằng', N'Giảng viên', N'Khoa Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG555444333', N'Lê Minh Long', N'Giảng viên', N'Khoa Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG222111000', N'Phạm Hùng Cường', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG100100100', N'Hoàng Thị Mai', N'Giảng viên', N'Khoa Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG200200200', N'Vũ Đức Thắng', N'Giảng viên', N'Khoa Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG300300300', N'Đặng Thu Hà', N'Giảng viên', N'Khoa Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG400400400', N'Bùi Thanh Tùng', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG500500500', N'Hồ Ngọc Bích', N'Giảng viên', N'Khoa Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG600600600', N'Ngô Minh Quân', N'Giảng viên', N'Khoa Tài chính ngân hàng');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG700700700', N'Dương Chí Thiện', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG800800800', N'Mai Lan Hương', N'Giảng viên', N'Khoa Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG900900900', N'Lý Hoàng Nam', N'Giảng viên', N'Khoa Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG011011011', N'Trịnh Tuấn Anh', N'Giảng viên', N'Khoa Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG023023023', N'Phan Thanh Bình', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG034034034', N'Đoàn Mỹ Lệ', N'Giảng viên', N'Khoa Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG045045045', N'Lâm Hoàng Long', N'Giảng viên', N'Khoa Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG056056056', N'Châu Minh Triết', N'Giảng viên', N'Khoa Tài chính ngân hàng');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG067067067', N'Vương Thanh Tâm', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG078078078', N'Tô Gia Bảo', N'Giảng viên', N'Khoa Kế toán');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG089089089', N'Nguyễn Hoàng Yến', N'Giảng viên', N'Khoa Quản trị kinh doanh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG091091091', N'Lê Quang Huy', N'Giảng viên', N'Khoa Công nghệ thông tin');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG092092092', N'Hà Mỹ Linh', N'Giảng viên', N'Khoa Ngôn ngữ Anh');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG093093093', N'Đỗ Minh Hiếu', N'Giảng viên', N'Khoa Thiết kế đồ họa');
INSERT INTO Readers (ReaderId, FullName, TypeOfReader, Department) VALUES (N'DG094094094', N'Huỳnh Ngọc Ánh', N'Giảng viên', N'Khoa Công nghệ thông tin');

-- ==============================================
-- TÀI KHOẢN CHO 25 SINH VIÊN
-- (Bắt đầu từ TK003, giả sử TK001 và TK002 đã tồn tại)
-- ==============================================

INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK003', N'tvan', N'123', N'DG123456789');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK004', N'ntbinh', N'123', N'DG111222333');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK005', N'lvcuong', N'123', N'DG444555666');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK006', N'ptdung', N'123', N'DG777888999');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK007', N'hvgiang', N'123', N'DG101101101');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK008', N'vthuong', N'123', N'DG202202202');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK009', N'dvkhanh', N'123', N'DG303303303');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK010', N'btlan', N'123', N'DG404404404');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK011', N'hvminh', N'123', N'DG505505505');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK012', N'ntnga', N'123', N'DG606606606');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK013', N'dvphuc', N'123', N'DG707707707');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK014', N'mtq uyen', N'123', N'DG808808808');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK015', N'lvson', N'123', N'DG909909909');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK016', N'ttthao', N'123', N'DG010010010');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK017', N'pvtoan', N'123', N'DG012012012');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK018', N'dtuyen', N'123', N'DG013013013');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK019', N'lvvi', N'123', N'DG014014014');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK020', N'ctxuan', N'123', N'DG015015015');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK021', N'vvyen', N'123', N'DG016016016');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK022', N'tvhung', N'123', N'DG017017017');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK023', N'nbnam', N'123', N'DG018018018');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK024', N'lkchi', N'123', N'DG019019019');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK025', N'htkiet', N'123', N'DG020020020');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK026', N'dpanh', N'123', N'DG021021021');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK027', N'hghuy', N'123', N'DG022022022');

-- ==============================================
-- TÀI KHOẢN CHO 25 GIẢNG VIÊN
-- ==============================================

INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK028', N'gv_nvthanh', N'123', N'DG987654321');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK029', N'gv_tthhang', N'123', N'DG888777666');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK030', N'gv_lmlong', N'123', N'DG555444333');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK031', N'gv_phcuong', N'123', N'DG222111000');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK032', N'gv_htmai', N'123', N'DG100100100');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK033', N'gv_vdthang', N'123', N'DG200200200');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK034', N'gv_dtha', N'123', N'DG300300300');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK035', N'gv_bttung', N'123', N'DG400400400');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK036', N'gv_hnbich', N'123', N'DG500500500');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK037', N'gv_nmquan', N'123', N'DG600600600');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK038', N'gv_dcthien', N'123', N'DG700700700');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK039', N'gv_mlhuong', N'123', N'DG800800800');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK040', N'gv_lhnam', N'123', N'DG900900900');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK041', N'gv_ttanh', N'123', N'DG011011011');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK042', N'gv_ptbinh', N'123', N'DG023023023');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK043', N'gv_dmle', N'123', N'DG034034034');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK044', N'gv_lhlong', N'123', N'DG045045045');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK045', N'gv_cmtriet', N'123', N'DG056056056');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK046', N'gv_vttam', N'123', N'DG067067067');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK047', N'gv_tgbao', N'123', N'DG078078078');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK048', N'gv_nhyen', N'123', N'DG089089089');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK049', N'gv_lqhuy', N'123', N'DG091091091');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK050', N'gv_hmlinh', N'123', N'DG092092092');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK051', N'gv_dmhieu', N'123', N'DG093093093');
INSERT INTO Account (AccountId, Username, Password, ReaderId) VALUES (N'TK052', N'gv_hnanh', N'123', N'DG094094094');


GO

PRINT N'Bắt đầu thêm dữ liệu bổ sung...';
GO

-- ==============================================
-- BỔ SUNG THÊM THỂ LOẠI
-- ==============================================
INSERT INTO Category (CategoryId, Name)
VALUES
(N'TL004', N'Văn học'),
(N'TL005', N'Kinh tế'),
(N'TL006', N'Tâm lý học'),
(N'TL007', N'Khoa học xã hội'),
(N'TL008', N'Ngoại ngữ');
GO

-- ==============================================
-- BỔ SUNG THÊM TÁC GIẢ
-- ==============================================
INSERT INTO Author (AuthorId, Name)
VALUES
(N'TG007', N'Dale Carnegie'),
(N'TG008', N'Yuval Noah Harari'),
(N'TG009', N'Robert Kiyosaki'),
(N'TG010', N'Paulo Coelho'),
(N'TG011', N'Nguyễn Nhật Ánh');
GO

-- ==============================================
-- BỔ SUNG THÊM NHÀ XUẤT BẢN
-- ==============================================
INSERT INTO Publisher (PublisherId, Name, Address, Telephone)
VALUES
(N'NXB004', N'Nhà xuất bản Kim Đồng', N'Hà Nội', N'0249876543'),
(N'NXB005', N'Nhà xuất bản Tổng hợp TPHCM', N'TP.HCM', N'0281122334');
GO

-- ==============================================
-- BỔ SUNG THÊM SÁCH (S008 - S020)
-- ==============================================
INSERT INTO Books (BookId, Name, YearOfPublic, Position, NumOfPage, Cost, CategoryId, AuthorId, PublisherId)
VALUES
(N'S008', N'Đắc nhân tâm', 2018, N'E1', 320, 79000, N'TL006', N'TG007', N'NXB005'),
(N'S009', N'Sapiens: Lược sử loài người', 2017, N'F2', 512, 169000, N'TL007', N'TG008', N'NXB002'),
(N'S010', N'Cha giàu, cha nghèo', 2000, N'E2', 207, 68000, N'TL005', N'TG009', N'NXB003'),
(N'S011', N'Nhà giả kim', 1988, N'G1', 224, 75000, N'TL004', N'TG010', N'NXB005'),
(N'S012', N'Cho tôi xin một vé đi tuổi thơ', 2008, N'G2', 200, 50000, N'TL004', N'TG011', N'NXB004'),
(N'S013', N'Quẳng gánh lo đi và vui sống', 2019, N'E1', 380, 90000, N'TL006', N'TG007', N'NXB005'),
(N'S014', N'Homo Deus: Lược sử tương lai', 2019, N'F2', 540, 179000, N'TL007', N'TG008', N'NXB002'),
(N'S015', N'Mắt biếc', 1990, N'G2', 250, 65000, N'TL004', N'TG011', N'NXB004'),
(N'S016', N'Lập trình Python từ cơ bản đến nâng cao', 2022, N'A4', 600, 250000, N'TL001', N'TG004', N'NXB001'),
(N'S017', N'Ngữ pháp Tiếng Anh cơ bản', 2020, N'H1', 400, 120000, N'TL008', N'TG003', N'NXB001'),
(N'S018', N'Kế toán tài chính', 2021, N'E3', 550, 180000, N'TL005', N'TG002', N'NXB003'),
(N'S019', N'Quản trị mạng căn bản', 2022, N'C3', 450, 160000, N'TL002', N'TG005', N'NXB001'),
(N'S020', N'Tâm lý học đám đông', 2015, N'F1', 300, 88000, N'TL006', N'TG006', N'NXB002');
GO

-- ==============================================
-- THÊM DỮ LIỆU MƯỢN TRẢ (PHIẾU MƯỢN)
-- Giả sử hôm nay là ngày 30/10/2025
-- ==============================================

-- Phiếu 1: Đã trả (Sinh viên DG123456789)
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
(N'PM001', N'DG123456789', N'NV001', '2025-09-01', '2025-09-15', '2025-09-14', N'Đã trả');

-- Phiếu 2: Đang mượn, chưa quá hạn (Sinh viên DG111222333)
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
(N'PM002', N'DG111222333', N'NV001', '2025-10-20', '2025-11-03', NULL, N'Đang mượn');

-- Phiếu 3: Quá hạn (Sinh viên DG444555666)
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
(N'PM003', N'DG444555666', N'NV001', '2025-09-10', '2025-09-24', NULL, N'Quá hạn');

-- Phiếu 4: Đã trả (Giảng viên DG987654321)
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
(N'PM004', N'DG987654321', N'NV001', '2025-10-01', '2025-10-30', '2025-10-25', N'Đã trả');

-- Phiếu 5: Đang mượn (Giảng viên DG888777666)
INSERT INTO LoanSlip (LoanId, ReaderId, EmployeeId, LoanDate, ExpiredDate, ReturnDate, Status)
VALUES
(N'PM005', N'DG888777666', N'NV001', '2025-10-15', '2025-11-15', NULL, N'Đang mượn');

GO

-- ==============================================
-- THÊM DỮ LIỆU CHI TIẾT MƯỢN TRẢ
-- ==============================================

-- Chi tiết cho Phiếu PM001 (Đã trả)
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
(N'CT001', N'PM001', N'S001', N'Bình thường', N'Bình thường', 0, 0),
(N'CT002', N'PM001', N'S008', N'Bình thường', N'Bình thường', 0, 0);

-- Chi tiết cho Phiếu PM002 (Đang mượn)
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
(N'CT003', N'PM002', N'S009', N'Bình thường', NULL, 0, 0);

-- Chi tiết cho Phiếu PM003 (Quá hạn)
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
(N'CT004', N'PM003', N'S010', N'Bình thường', NULL, 0, 0),
(N'CT005', N'PM003', N'S011', N'Bình thường', NULL, 0, 0);

-- Chi tiết cho Phiếu PM004 (Đã trả, 1 cuốn mất, 1 cuốn hư hỏng)
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
(N'CT006', N'PM004', N'S012', N'Bình thường', N'Bình thường', 0, 0),
(N'CT007', N'PM004', N'S002', N'Bình thường', N'Hư hỏng nhẹ', 0, 20000), -- Phạt 20k
(N'CT008', N'PM004', N'S003', N'Bình thường', N'Mất sách', 1, 90000);   -- Phạt 90k (bằng giá sách)

-- Chi tiết cho Phiếu PM005 (Đang mượn)
INSERT INTO LoanDetail (LoanDetailId, LoanId, BookId, LoanStatus, ReturnStatus, IsLose, Fine)
VALUES
(N'CT009', N'PM005', N'S016', N'Bình thường', NULL, 0, 0),
(N'CT010', N'PM005', N'S017', N'Bình thường', NULL, 0, 0),
(N'CT011', N'PM005', N'S018', N'Bình thường', NULL, 0, 0);

GO

PRINT N'Đã thêm thành công dữ liệu sách và phiếu mượn!';
GO
