﻿// <copyright>
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge.Component
{

    /// <summary>
    /// 
    /// </summary>
    [Description( "Displays a badge if the entity is in a chosen DataView." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "In Data View" )]

    [DataViewField( "Data View", "The dataview to use as the source for the query. Only those people in the DataView will be given the badge.", true, entityTypeName: "Rock.Model.Person", order: 0 )]
    [CodeEditorField( "Badge Content", "The text or HTML of the badge to display. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "<div class='badge badge-icon'><i class='fa fa-smile-o'></i></div>", order: 1 )]
    public class InDataView : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            RockContext rockContext = new RockContext();
            var dataViewAttributeGuid = GetAttributeValue( badge, "DataView" ).AsGuid();
            var dataViewService = new DataViewService( rockContext );
            if ( dataViewAttributeGuid != Guid.Empty )
            {
                var dataView = dataViewService.Get( dataViewAttributeGuid );
                if ( dataView != null )
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        DatabaseTimeoutSeconds = 30
                    };

                    var qry = dataView.GetQuery( dataViewGetQueryArgs );

                    var isEntityFound = false;
                    if ( qry != null )
                    {
                        isEntityFound = qry.Where( e => e.Id == Entity.Id ).Any();
                        stopwatch.Stop();
                        DataViewService.AddRunDataViewTransaction( dataView.Id,
                                                        Convert.ToInt32( stopwatch.Elapsed.TotalMilliseconds ) );
                    }

                    if ( isEntityFound )
                    {
                        Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                        mergeValues.Add( "Person", Person );
                        mergeValues.Add( "Entity", Entity );
                        writer.Write( GetAttributeValue( badge, "BadgeContent" ).ResolveMergeFields( mergeValues ) );
                    }
                }
            }
        }
    }
}
