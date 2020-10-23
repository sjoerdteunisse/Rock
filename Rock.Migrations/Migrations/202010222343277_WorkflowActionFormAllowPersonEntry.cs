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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class WorkflowActionFormAllowPersonEntry : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowActionForm", "AllowPersonEntry", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryPreHtml", c => c.String());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryPostHtml", c => c.String());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryCampusIsVisible", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryAutofillCurrentPerson", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryHideIfCurrentPersonKnown", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntrySpouseEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryEmailEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryMobilePhoneEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryBirthdateEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryAddressEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryMaritalStatusEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntrySpouseLabel", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryAddressTypeValueId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", c => c.Int());
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryAddressTypeValueId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryAddressTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", "dbo.Attribute", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", "dbo.Attribute", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", "dbo.Attribute", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryAddressTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryFamilyAttributeId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntrySpouseAttributeId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryPersonAttributeId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryAddressTypeValueId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryRecordStatusValueId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryConnectionStatusValueId" });
            DropColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryAddressTypeValueId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntrySpouseLabel");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryMaritalStatusEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryAddressEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryBirthdateEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryMobilePhoneEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryEmailEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntrySpouseEntryOption");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryHideIfCurrentPersonKnown");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryAutofillCurrentPerson");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryCampusIsVisible");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryPostHtml");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryPreHtml");
            DropColumn("dbo.WorkflowActionForm", "AllowPersonEntry");
        }
    }
}
