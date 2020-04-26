--
-- File generated with SQLiteStudio v3.2.1 on Mon Nov 4 13:58:57 2019
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
CREATE TABLE EBookFiles (
    Id         INTEGER      PRIMARY KEY AUTOINCREMENT,
    Format     VARCHAR (10) NOT NULL,
    Signature  VARCHAR (64) NOT NULL,
    RawFileId  INT          REFERENCES RawFiles(Id)
    				        NOT NULL
);

-- Table: RawFiles
CREATE TABLE RawFiles (
    Id         INTEGER      PRIMARY KEY AUTOINCREMENT,
    RawContent BLOB         NOT NULL
);

-- Table: Books
CREATE TABLE Books (
    Id             INTEGER       PRIMARY KEY AUTOINCREMENT,
    Title          VARCHAR (100) NOT NULL,
    SeriesId       INTEGER       REFERENCES Series (Id),
    IsRead         BOOL          DEFAULT (false),
    Cover          BLOB,
    WhenRead       INT,
    NumberInSeries DECIMAL,
    IsFavorite     BOOLEAN       NOT NULL
                                 DEFAULT (false),
    PercentageRead DECIMAL       NOT NULL
                                 DEFAULT (0),
    FileId         INT           REFERENCES EBookFiles(Id)
    							 NOT NULL
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


-- Index: Author_Book_BookId_Index
CREATE INDEX Author_Book_BookId_Index ON AuthorBooks (
    Book_Id
);


-- Index: User_Collection_BookId_Index
CREATE INDEX User_Collection_BookId_Index ON UserCollectionBooks (
    Book_Id
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
