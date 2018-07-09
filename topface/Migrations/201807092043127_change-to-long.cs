namespace topface.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changetolong : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.InfoAds", "TelNumber", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InfoAds", "TelNumber", c => c.Int(nullable: false));
        }
    }
}
