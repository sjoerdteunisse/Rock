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
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Bus.Consumer;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Bus
{
    [DisplayName( "Consumer List" )]
    [Category( "Bus" )]
    [Description( "Shows a list of all message bus consumers." )]

    public partial class ConsumerList : RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;
            gList.RowItemText = "Consumer";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion Base Control Methods

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var viewModels = RockConsumer.GetConsumerTypes()
                .Select( ct =>
                {
                    var queue = RockConsumer.GetQueue( ct );

                    return new ConsumerViewModel
                    {
                        ConsumerName = ct.Name,
                        QueueName = queue == null ? null : queue.Name
                    };
                } )
                .AsQueryable();

            // Sort the query based on the column that was selected to be sorted
            var sortProperty = gList.SortProperty;

            if ( gList.AllowSorting && sortProperty != null )
            {
                viewModels = viewModels.Sort( sortProperty );
            }
            else
            {
                viewModels = viewModels
                    .OrderBy( vm => vm.ConsumerName )
                    .ThenBy( vm => vm.QueueName );
            }

            gList.DataSource = viewModels.ToList();
            gList.DataBind();
        }

        #endregion Methods

        #region ViewModels

        /// <summary>
        /// Consumer View Model
        /// </summary>
        public sealed class ConsumerViewModel
        {
            /// <summary>
            /// Gets or sets the name of the consumer.
            /// </summary>
            /// <value>
            /// The name of the consumer.
            /// </value>
            public string ConsumerName { get; set; }

            /// <summary>
            /// Gets or sets the name of the queue.
            /// </summary>
            /// <value>
            /// The name of the queue.
            /// </value>
            public string QueueName { get; set; }
        }

        #endregion ViewModels
    }
}