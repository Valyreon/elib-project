--
-- File generated with SQLiteStudio v3.2.1 on Sun Jun 21 15:22:40 2020
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: AuthorBooks
CREATE TABLE AuthorBooks (
    BookId   INTEGER NOT NULL
                     REFERENCES Books (Id),
    AuthorId INTEGER NOT NULL
                     REFERENCES Authors (Id),
    Id       INTEGER PRIMARY KEY AUTOINCREMENT
                     NOT NULL
);


-- Table: Authors
CREATE TABLE Authors (
    Id   INTEGER       PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR (100) NOT NULL
);


-- Table: Books
CREATE TABLE Books (
    Id             INTEGER       PRIMARY KEY AUTOINCREMENT,
    Title          VARCHAR (100) NOT NULL,
    SeriesId       INTEGER       REFERENCES Series (Id),
    IsRead         BOOL          DEFAULT (false),
    WhenRead       INT,
    NumberInSeries DECIMAL,
    IsFavorite     BOOLEAN       NOT NULL
                                 DEFAULT (false),
    PercentageRead DECIMAL       NOT NULL
                                 DEFAULT (0),
    FileId         INT           REFERENCES EBookFiles (Id) 
                                 NOT NULL,
    CoverId        INTEGER       REFERENCES Covers (Id) 
);


-- Table: Covers
CREATE TABLE Covers (
    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
    Image BLOB    NOT NULL
);


-- Table: EBookFiles
CREATE TABLE EBookFiles (
    Id        INTEGER      PRIMARY KEY AUTOINCREMENT,
    Format    VARCHAR (10) NOT NULL,
    Signature VARCHAR (64) NOT NULL,
    RawFileId INT          REFERENCES RawFiles (Id) 
                           NOT NULL
);


-- Table: Quotes
CREATE TABLE Quotes (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Text   VARCHAR,
    BookId INTEGER REFERENCES Books (Id),
    Note   VARCHAR
);


-- Table: RawFiles
CREATE TABLE RawFiles (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    RawContent BLOB    NOT NULL
);


-- Table: Series
CREATE TABLE Series (
    Id   INTEGER       PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR (100) NOT NULL
);


-- Table: UserCollectionBooks
CREATE TABLE UserCollectionBooks (
    BookId           INTEGER NOT NULL
                             REFERENCES Books (Id),
    UserCollectionId INTEGER NOT NULL
                             REFERENCES UserCollections (Id),
    Id               INTEGER PRIMARY KEY AUTOINCREMENT
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
    BookId
);


-- Index: User_Collection_BookId_Index
CREATE INDEX User_Collection_BookId_Index ON UserCollectionBooks (
    BookId
);


-- View: AuthorId_Book_View
CREATE VIEW AuthorId_Book_View AS
    SELECT *
      FROM (
               SELECT AuthorBooks.AuthorId,
                      Books.Id,
                      Books.Title,
                      Books.IsFavorite,
                      Books.IsRead,
                      Books.FileId,
                      Books.NumberInSeries,
                      Books.SeriesId,
                      Books.PercentageRead,
                      Books.WhenRead
                 FROM Books
                      INNER JOIN
                      AuthorBooks ON Books.Id = AuthorBooks.BookId
           );


-- View: Book_Author_Join
CREATE VIEW Book_Author_Join AS
    SELECT AuthorId_Book_View.Id,
           AuthorId_Book_View.Title,
           AuthorId_Book_View.SeriesId,
           AuthorId_Book_View.IsFavorite,
           AuthorId_Book_View.IsRead,
           AuthorId_Book_View.WhenRead,
           AuthorId_Book_View.NumberInSeries,
           AuthorId_Book_View.PercentageRead,
           AuthorId_Book_View.FileId,
           Authors.Name AS AuthorName
      FROM AuthorId_Book_View
           INNER JOIN
           Authors ON Authors.Id = AuthorId_Book_View.AuthorId;


-- View: Book_Series_Join
CREATE VIEW Book_Series_Join AS
    SELECT Books.Id,
           Books.Title,
           Books.SeriesId,
           Books.IsFavorite,
           Books.IsRead,
           Books.CoverId,
           Books.WhenRead,
           Books.NumberInSeries,
           Books.PercentageRead,
           Books.FileId,
           Series.Name AS SeriesName
      FROM Books
           LEFT JOIN
           Series ON Series.Id = Books.SeriesId;


-- View: BookId_Author_View
CREATE VIEW BookId_Author_View AS
    SELECT *
      FROM (
               SELECT AuthorBooks.BookId,
                      Authors.Id,
                      Authors.Name
                 FROM Authors
                      INNER JOIN
                      AuthorBooks ON Authors.Id = AuthorBooks.AuthorId
           );


-- View: BookId_Collection_View
CREATE VIEW BookId_Collection_View AS
    SELECT *
      FROM (
               SELECT UserCollectionBooks.BookId,
                      UserCollections.Id,
                      UserCollections.Tag
                 FROM UserCollections
                      INNER JOIN
                      UserCollectionBooks ON UserCollections.Id = UserCollectionBooks.UserCollectionId
           );


-- View: CollectionId_Book_View
CREATE VIEW CollectionId_Book_View AS
    SELECT *
      FROM (
               SELECT UserCollectionBooks.UserCollectionId AS CollectionId,
                      Books.Id,
                      Books.Title,
                      Books.IsFavorite,
                      Books.IsRead,
                      Books.FileId,
                      Books.NumberInSeries,
                      Books.SeriesId,
                      Books.PercentageRead
                 FROM Books
                      INNER JOIN
                      UserCollectionBooks ON Books.Id = UserCollectionBooks.BookId
           );


-- View: Full_Join
CREATE VIEW Full_Join AS
    SELECT X.Id,
           X.Title,
           X.SeriesId,
           X.IsFavorite,
           X.IsRead,
           X.WhenRead,
           X.NumberInSeries,
           X.PercentageRead,
           X.FileId,
           X.SeriesName,
           X.AuthorName,
           X.AuthorId,
           X.CoverId,
           Tag,
           BookId_Collection_View.Id AS CollectionId
      FROM (
               SELECT BookId AS Id,
                      BookId_Author_View.Id AS AuthorId,
                      Title,
                      SeriesId,
                      CoverId,
                      IsFavorite,
                      IsRead,
                      WhenRead,
                      NumberInSeries,
                      PercentageRead,
                      FileId,
                      SeriesName,
                      Name AS AuthorName
                 FROM Book_Series_Join
                      INNER JOIN
                      BookId_Author_View ON Book_Series_Join.Id = BookId_Author_View.BookId
           )
           AS X
           LEFT JOIN
           BookId_Collection_View ON BookId_Collection_View.BookId = X.Id;


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
