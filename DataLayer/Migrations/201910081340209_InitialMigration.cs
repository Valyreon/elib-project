namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Authors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Cover = c.Binary(),
                        IsRead = c.Boolean(nullable: false),
                        WhenRead = c.DateTime(nullable: false),
                        Series_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookSeries", t => t.Series_Id)
                .Index(t => t.Series_Id, name: "IX_Book_Series_Id");
            
            CreateTable(
                "dbo.UserCollections",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tag = c.String(nullable: false),
                        Book_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.Book_Id)
                .Index(t => t.Book_Id, name: "IX_UserCollection_Book_Id");
            
            CreateTable(
                "dbo.EFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Format = c.String(nullable: false),
                        RawContent = c.Binary(nullable: false),
                        Book_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.Book_Id, cascadeDelete: true)
                .Index(t => t.Book_Id, name: "IX_EFile_Book_Id");
            
            CreateTable(
                "dbo.Quotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        Note = c.String(),
                        Book_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.Book_Id)
                .Index(t => t.Book_Id, name: "IX_Quote_Book_Id");
            
            CreateTable(
                "dbo.BookSeries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AuthorBooks",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        AuthorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.AuthorId })
                .ForeignKey("dbo.Authors", t => t.AuthorId, cascadeDelete: true)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .Index(t => t.BookId, name: "IX_AuthorBook_BookId")
                .Index(t => t.AuthorId, name: "IX_AuthorBook_AuthorId");
            
            CreateTable(
                "dbo.CollectionBooks",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        CollectionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.CollectionId })
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.UserCollections", t => t.CollectionId, cascadeDelete: true)
                .Index(t => t.BookId, name: "IX_CollectionBook_BookId")
                .Index(t => t.CollectionId, name: "IX_CollectionBook_CollectionId");
            
            CreateTable(
                "dbo.AuthorBook1",
                c => new
                    {
                        Author_Id = c.Int(nullable: false),
                        Book_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Author_Id, t.Book_Id })
                .ForeignKey("dbo.Authors", t => t.Author_Id, cascadeDelete: true)
                .ForeignKey("dbo.Books", t => t.Book_Id, cascadeDelete: true)
                .Index(t => t.Author_Id, name: "IX_AuthorBook1_Author_Id")
                .Index(t => t.Book_Id, name: "IX_AuthorBook1_Book_Id");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CollectionBooks", "CollectionId", "dbo.UserCollections");
            DropForeignKey("dbo.CollectionBooks", "BookId", "dbo.Books");
            DropForeignKey("dbo.AuthorBooks", "BookId", "dbo.Books");
            DropForeignKey("dbo.AuthorBooks", "AuthorId", "dbo.Authors");
            DropForeignKey("dbo.AuthorBook1", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.AuthorBook1", "Author_Id", "dbo.Authors");
            DropForeignKey("dbo.Books", "Series_Id", "dbo.BookSeries");
            DropForeignKey("dbo.Quotes", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.EFiles", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.UserCollections", "Book_Id", "dbo.Books");
            DropIndex("dbo.AuthorBook1", "IX_AuthorBook1_Book_Id");
            DropIndex("dbo.AuthorBook1", "IX_AuthorBook1_Author_Id");
            DropIndex("dbo.CollectionBooks", "IX_CollectionBook_CollectionId");
            DropIndex("dbo.CollectionBooks", "IX_CollectionBook_BookId");
            DropIndex("dbo.AuthorBooks", "IX_AuthorBook_AuthorId");
            DropIndex("dbo.AuthorBooks", "IX_AuthorBook_BookId");
            DropIndex("dbo.Quotes", "IX_Quote_Book_Id");
            DropIndex("dbo.EFiles", "IX_EFile_Book_Id");
            DropIndex("dbo.UserCollections", "IX_UserCollection_Book_Id");
            DropIndex("dbo.Books", "IX_Book_Series_Id");
            DropTable("dbo.AuthorBook1");
            DropTable("dbo.CollectionBooks");
            DropTable("dbo.AuthorBooks");
            DropTable("dbo.BookSeries");
            DropTable("dbo.Quotes");
            DropTable("dbo.EFiles");
            DropTable("dbo.UserCollections");
            DropTable("dbo.Books");
            DropTable("dbo.Authors");
        }
    }
}
