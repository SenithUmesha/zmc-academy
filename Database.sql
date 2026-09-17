IF DB_ID('ZMC_Academy') IS NULL
BEGIN
    CREATE DATABASE ZMC_Academy;
END
GO

USE ZMC_Academy;
GO

-- Development schema for the historical WinForms project.
-- This script intentionally contains no personal student records, real credentials,
-- full payment-card numbers, or card verification values.

DROP TABLE IF EXISTS Report_problem;
DROP TABLE IF EXISTS Attendence2;
DROP TABLE IF EXISTS Attendence1;
DROP TABLE IF EXISTS Pastpaperlist;
DROP TABLE IF EXISTS Pastpapers;
DROP TABLE IF EXISTS Booklist;
DROP TABLE IF EXISTS Books;
DROP TABLE IF EXISTS News;
DROP TABLE IF EXISTS Payment;
DROP TABLE IF EXISTS Course;
DROP TABLE IF EXISTS Other_Qualifications1;
DROP TABLE IF EXISTS Other_Qualifications;
DROP TABLE IF EXISTS Al_Results;
DROP TABLE IF EXISTS Ol_Results;
DROP TABLE IF EXISTS Registration;
GO

CREATE TABLE Registration (
    Name varchar(80) NOT NULL,
    Id varchar(10) NOT NULL PRIMARY KEY,
    Password varchar(255) NOT NULL,
    Address varchar(120) NULL,
    Contact_number varchar(20) NULL,
    Birth_of_date date NULL,
    Email varchar(120) NULL
);
GO

CREATE UNIQUE INDEX UX_Registration_Email
    ON Registration(Email)
    WHERE Email IS NOT NULL;
GO

CREATE TABLE Ol_Results (
    Year int NULL,
    Mathematics char(2) NULL,
    Science char(2) NULL,
    Sinhala char(2) NULL,
    English char(2) NULL,
    History char(2) NULL,
    Religion char(2) NULL,
    Bucket_1 char(2) NULL,
    Bucket_2 char(2) NULL,
    Bucket_3 char(2) NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_OlResults_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

CREATE TABLE Al_Results (
    Year int NULL,
    Stream varchar(30) NULL,
    Bucket_1 char(2) NULL,
    Bucket_2 char(2) NULL,
    Bucket_3 char(2) NULL,
    English char(2) NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_AlResults_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

-- Kept as two tables for compatibility with the original coursework UI.
CREATE TABLE Other_Qualifications (
    Category varchar(50) NULL,
    Name varchar(80) NULL,
    Reason varchar(200) NULL,
    Year int NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_OtherQualifications_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

CREATE TABLE Other_Qualifications1 (
    Category varchar(50) NULL,
    Name varchar(80) NULL,
    Reason varchar(200) NULL,
    Year int NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_OtherQualifications1_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

CREATE TABLE Course (
    Course_school varchar(80) NULL,
    Course_name varchar(80) NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_Course_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

-- Historical payment-form prototype. Only non-sensitive display metadata is stored.
-- Never store the CVC; the current app keeps only the final four digits of the card number.
CREATE TABLE Payment (
    Method varchar(30) NULL,
    Type varchar(40) NULL,
    Card_last4 varchar(4) NULL,
    Expire_date date NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_Payment_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

CREATE TABLE News (
    News_id varchar(10) NOT NULL PRIMARY KEY,
    News_name varchar(180) NOT NULL,
    News_date date NOT NULL
);
GO

CREATE TABLE Books (
    B_id varchar(10) NOT NULL PRIMARY KEY,
    B_name varchar(100) NOT NULL,
    B_author varchar(100) NOT NULL
);
GO

CREATE TABLE Booklist (
    B_addeddate date NOT NULL,
    B_returndate date NOT NULL,
    B_id varchar(10) NOT NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_Booklist_Books
        FOREIGN KEY (B_id) REFERENCES Books(B_id),
    CONSTRAINT FK_Booklist_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id),
    CONSTRAINT UQ_Booklist_StudentBook UNIQUE (Id, B_id)
);
GO

CREATE TABLE Pastpapers (
    P_id varchar(10) NOT NULL PRIMARY KEY,
    Subject varchar(50) NOT NULL,
    Year int NOT NULL
);
GO

CREATE TABLE Pastpaperlist (
    P_addeddate date NOT NULL,
    P_returndate date NOT NULL,
    P_id varchar(10) NOT NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_Pastpaperlist_Pastpapers
        FOREIGN KEY (P_id) REFERENCES Pastpapers(P_id),
    CONSTRAINT FK_Pastpaperlist_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id),
    CONSTRAINT UQ_Pastpaperlist_StudentPaper UNIQUE (Id, P_id)
);
GO

-- The original UI explicitly switches between these two known tables.
CREATE TABLE Attendence1 (
    Id varchar(10) NOT NULL,
    [date] date NOT NULL,
    Attended varchar(10) NOT NULL,
    CONSTRAINT FK_Attendence1_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id),
    CONSTRAINT UQ_Attendence1_StudentDate UNIQUE (Id, [date])
);
GO

CREATE TABLE Attendence2 (
    Id varchar(10) NOT NULL,
    [date] date NOT NULL,
    Attended varchar(10) NOT NULL,
    CONSTRAINT FK_Attendence2_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id),
    CONSTRAINT UQ_Attendence2_StudentDate UNIQUE (Id, [date])
);
GO

CREATE TABLE Report_problem (
    Summary varchar(100) NOT NULL,
    Details varchar(500) NOT NULL,
    [date] datetime NOT NULL,
    Id varchar(10) NOT NULL,
    CONSTRAINT FK_ReportProblem_Registration
        FOREIGN KEY (Id) REFERENCES Registration(Id)
);
GO

INSERT INTO News(News_id, News_name, News_date) VALUES
    ('N001', 'Welcome to the development academy database', '2021-01-04'),
    ('N002', 'Library and past-paper sections are available', '2021-01-11'),
    ('N003', 'Remember to review your attendance history', '2021-01-18');
GO

INSERT INTO Books(B_id, B_name, B_author) VALUES
    ('B001', 'Adventures of Tom Sawyer', 'Mark Twain'),
    ('B002', 'Alice in Wonderland', 'Lewis Carroll'),
    ('B003', 'Animal Farm', 'George Orwell'),
    ('B004', 'A Passage to India', 'E. M. Forster'),
    ('B005', 'Anna Karenina', 'Leo Tolstoy');
GO

INSERT INTO Pastpapers(P_id, Subject, Year) VALUES
    ('P001', 'GUI', 2020),
    ('P002', 'OOP', 2020),
    ('P003', 'Computer Networks', 2020),
    ('P004', 'Operating Systems', 2021),
    ('P005', 'Computer Organization', 2021);
GO
