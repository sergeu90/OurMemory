namespace OurMemory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAutoGenerateDateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Articles", "CreatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AlterColumn("dbo.Articles", "UpdatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AlterColumn("dbo.Veterans", "CreatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AlterColumn("dbo.Veterans", "UpdatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AlterColumn("dbo.ImageVeterans", "CreatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AlterColumn("dbo.ImageVeterans", "UpdatedDateTime", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ImageVeterans", "UpdatedDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ImageVeterans", "CreatedDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Veterans", "UpdatedDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Veterans", "CreatedDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Articles", "UpdatedDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Articles", "CreatedDateTime", c => c.DateTime(nullable: false));
        }
    }
}
