// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class EventBus : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "IsMessageBusEventPublishEnabled", c => c.Boolean(nullable: false));
            CmsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CmsDown();
            DropColumn("dbo.EntityType", "IsMessageBusEventPublishEnabled");
        }

        /// <summary>
        /// CMS up.
        /// </summary>
        private void CmsUp()
        {
            // Add Page Message Bus to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Message Bus", "", "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41", "fa fa-bus" );

            // Add/Update BlockType Bus Controls
            RockMigrationHelper.UpdateBlockType( "Bus Controls", "Control the bus.", "~/Blocks/Bus/BusControls.ascx", "Bus", "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" );

            // Add Block Transports to Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "Transports", "Main", @"", @"", 1, "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC" );

            // Add Block Restart Required Notification to Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Restart Required Notification", "Main", @"", @"", 0, "70E9E506-8E74-4D89-A1BC-345449618D18" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Message Bus,  Zone: Main,  Block: Restart Required Notification
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '70E9E506-8E74-4D89-A1BC-345449618D18'" );

            // Update Order for Page: Message Bus,  Zone: Main,  Block: Transports
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC'" );

            // Add/Update HtmlContent for Block: Restart Required Notification
            RockMigrationHelper.UpdateHtmlContentBlock(
                "70E9E506-8E74-4D89-A1BC-345449618D18",
@"<div class=""alert alert-warning"">
    Any changes made to the Message Bus configuration require a Rock restart to take affect.
</div>",
                "15833A55-B710-463B-8060-6FD368E56968" );

            // Block Attribute Value for Transports ( Page: Message Bus, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.Bus.Transport.TransportContainer, Rock" );

            // Block Attribute Value for Transports ( Page: Message Bus, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "A4889D7B-87AA-419D-846C-3E618E79D875", @"False" );

            // Block Attribute Value for Transports ( Page: Message Bus, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"False" );

            // Block Attribute Value for Transports ( Page: Message Bus, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );

            // Block Attribute Value for Transports ( Page: Message Bus, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "07BBF752-6CB5-4591-989F-05BCE78BC73C", @"False" );

            // Add/Update BlockType Consumer List
            RockMigrationHelper.UpdateBlockType( "Consumer List", "Shows a list of all message bus consumers.", "~/Blocks/Bus/ConsumerList.ascx", "Bus", "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5" );

            // Add Block Consumer List to Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5".AsGuid(), "Consumer List", "Main", @"", @"", 2, "E9BDBA5E-264C-4227-B304-338A788A730C" );

            // Update Order for Page: Message Bus,  Zone: Main,  Block: Consumer List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E9BDBA5E-264C-4227-B304-338A788A730C'" );
        }

        /// <summary>
        /// CMS down.
        /// </summary>
        private void CmsDown()
        {
            // Remove Block: Restart Required Notification, from Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "70E9E506-8E74-4D89-A1BC-345449618D18" );

            // Remove Block: Transports, from Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC" );

            // Delete BlockType Bus Controls
            RockMigrationHelper.DeleteBlockType( "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" ); // Bus Controls

            // Delete BlockType Consumer List
            RockMigrationHelper.DeleteBlockType( "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5" ); // Consumer List

            // Delete Page Message Bus from Site:Rock RMS
            RockMigrationHelper.DeletePage( "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41" ); //  Page: Message Bus, Layout: Full Width, Site: Rock RMS
        }
    }
}
