--
-- File generated with SQLiteStudio v3.2.1 on Wed Oct 9 22:27:50 2019
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: AuthorBooks
CREATE TABLE AuthorBooks (
    Book_Id   INTEGER NOT NULL
                      REFERENCES Books (Id),
    Author_Id INTEGER NOT NULL
                      REFERENCES Authors (Id),
    Id        INTEGER PRIMARY KEY AUTOINCREMENT
                      NOT NULL
);


-- Table: Authors
CREATE TABLE Authors (
    Id   INTEGER       PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR (100) NOT NULL
);


-- Table: BookFiles
CREATE TABLE BookFiles (
    Id         INTEGER      PRIMARY KEY AUTOINCREMENT,
    Format     VARCHAR (10) NOT NULL,
    RawContent BLOB         NOT NULL,
    BookId     INT          REFERENCES Books (Id) 
                            NOT NULL
);


-- Table: Books
CREATE TABLE Books (
    Id       INTEGER       PRIMARY KEY AUTOINCREMENT,
    Name     VARCHAR (100) NOT NULL,
    SeriesId INTEGER       REFERENCES Series (Id),
    IsRead   BOOL,
    Cover    BLOB,
    WhenRead INT
);


-- Table: Quotes
CREATE TABLE Quotes (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Text   VARCHAR,
    BookId INTEGER REFERENCES Books (Id),
    Note   VARCHAR
);


-- Table: Series
CREATE TABLE Series (
    Id   INTEGER       PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR (100) NOT NULL
);


-- Table: UserCollectionBooks
CREATE TABLE UserCollectionBooks (
    Book_Id           INTEGER NOT NULL
                              REFERENCES Books (Id),
    UserCollection_Id INTEGER NOT NULL
                              REFERENCES UserCollections (Id),
    Id                INTEGER PRIMARY KEY AUTOINCREMENT
                              NOT NULL
);


-- Table: UserCollections
CREATE TABLE UserCollections (
    Id  INTEGER      PRIMARY KEY AUTOINCREMENT,
    Tag VARCHAR (50) NOT NULL
                     UNIQUE
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
