Create database QuanLySV
GO

USE QuanLySV

-- Tạo bảng Lop
CREATE TABLE Lop (
    MaLop CHAR(3) PRIMARY KEY,
    TenLop NVARCHAR(30) NOT NULL
);

-- Tạo bảng SinhVien với cột NgaySinh
CREATE TABLE SinhVien (
    MaSV CHAR(6) PRIMARY KEY,
    HoTenSV NVARCHAR(40),
    NgaySinh DATE,
    MaLop CHAR(3),
    FOREIGN KEY (MaLop) REFERENCES Lop(MaLop)
);

-- Thêm dữ liệu vào bảng Lop
INSERT INTO Lop (MaLop, TenLop)
VALUES ('L01', 'Cong nghe thong tin'),
       ('L02', 'Quan tri kinh doanh');

-- Thêm dữ liệu vào bảng SinhVien với cột NgaySinh
INSERT INTO SinhVien (MaSV, HoTenSV, NgaySinh, MaLop)
VALUES ('SV001', 'Nguyen Van A', '2001-01-15', 'L01'),
       ('SV002', 'Tran Thi B', '2002-03-22', 'L01'),
       ('SV003', 'Le Van C', '2000-07-10', 'L02'),
       ('SV004', 'Pham Thi D', '2001-12-05', 'L02');


	   select * from Lop
	   select * from SinhVien