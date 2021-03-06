namespace OurMemory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFieldToPhotoAlbum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhotoAlbums", "Description", c => c.String());
            AddColumn("dbo.PhotoAlbums", "ImageAlbumUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhotoAlbums", "ImageAlbumUrl");
            DropColumn("dbo.PhotoAlbums", "Description");
        }
    }
}
