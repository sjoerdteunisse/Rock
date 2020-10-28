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
    public partial class WorkflowActionFormAllowPersonEntry2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", "dbo.Attribute");
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryPersonAttributeId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntrySpouseAttributeId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryFamilyAttributeId" });
            AddColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeGuid", c => c.Guid());
            AddColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeGuid", c => c.Guid());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeGuid", c => c.Guid());
            DropColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", c => c.Int());
            DropColumn("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeGuid");
            DropColumn("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeGuid");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryPersonAttributeGuid");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntrySpouseAttributeId", "dbo.Attribute", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryPersonAttributeId", "dbo.Attribute", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryFamilyAttributeId", "dbo.Attribute", "Id");
        }
    }
}
