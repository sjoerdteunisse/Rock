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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to process achievements for updated source entities
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class AchievementsProcessingTransaction : IBusStartedTransaction, IRockConsumer<StartTaskQueue, AchievementsProcessingTransaction.PropsClass>
    {
        /// <summary>
        /// Source Entity
        /// </summary>
        public sealed class SourceEntity
        {
            /// <summary>
            /// Gets or sets the entity type identifier.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public int EntityId { get; set; }
        }

        /// <summary>
        /// Props
        /// </summary>
        public sealed class PropsClass : IRockMessage
        {
            /// <summary>
            /// The entities that need to be processed
            /// </summary>
            public IEnumerable<SourceEntity> SourceEntities { get; set; }
        }

        /// <summary>
        /// Gets or sets the props.
        /// </summary>
        /// <value>
        /// The props.
        /// </value>
        public PropsClass Props
        {
            get
            {
                return new PropsClass
                {
                    SourceEntities = SourceEntities.Select( e => new SourceEntity
                    {
                        EntityId = e.Id,
                        EntityTypeId = EntityTypeCache.Get( e.GetType() ).Id
                    } )
                };
            }
            set
            {
                // TODO
            }
        }

        /// <summary>
        /// The entities that need to be processed
        /// </summary>
        private IEnumerable<IEntity> SourceEntities { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementAttemptChangeTransaction"/> class.
        /// </summary>
        public AchievementsProcessingTransaction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementAttemptChangeTransaction"/> class.
        /// </summary>
        /// <param name="sourceEntities">The source entities.</param>
        public AchievementsProcessingTransaction( IEnumerable<IEntity> sourceEntities )
        {
            if ( sourceEntities == null || !sourceEntities.Any() )
            {
                return;
            }

            SourceEntities = sourceEntities;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute()
        {
            if ( SourceEntities == null )
            {
                return;
            }

            foreach ( var sourceEntity in SourceEntities )
            {
                AchievementTypeCache.ProcessAchievements( sourceEntity );
            }
        }

        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task Consume( ConsumeContext<PropsClass> context )
        {
            Props = context.Message;
            Execute();
            return Task.Delay( 0 );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        public Task Send()
        {
            return RockMessageBus.SendOnStartTaskQueue( Props );
        }
    }
}