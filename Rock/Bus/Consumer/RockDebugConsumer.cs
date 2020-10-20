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

using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Bus.Consumer
{
    /// <summary>
    /// Entity Update Consumer
    /// </summary>
    public class RockDebugConsumer
    {
        /// <summary>
        /// Entity Update Debug
        /// </summary>
        public class EntityUpdate :
            IRockConsumer<EntityUpdateQueue, IEntityWasUpdatedMessage>,
            IRockConsumer<EntityUpdateQueue, EntityWasUpdatedMessage>,
            IRockConsumer<EntityUpdateQueue, IEntityWasUpdatedMessage<Person>>,
            IRockConsumer<EntityUpdateQueue, EntityWasUpdatedMessage<Person>>
        {
            private const string QueueName = nameof( EntityUpdateQueue );

            /// <summary>
            /// Consumes the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns></returns>
            public Task Consume( ConsumeContext<IEntityWasUpdatedMessage> context )
            {
                var json = context.Message.ToJson();
                WriteToDebug( QueueName, "IEntityWasUpdatedMessage", json );
                return Task.Delay( 0 );
            }

            /// <summary>
            /// Consumes the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns></returns>
            public Task Consume( ConsumeContext<EntityWasUpdatedMessage> context )
            {
                var json = context.Message.ToJson();
                WriteToDebug( QueueName, "EntityWasUpdatedMessage", json );
                return Task.Delay( 0 );
            }

            /// <summary>
            /// Consumes the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns></returns>
            public Task Consume( ConsumeContext<IEntityWasUpdatedMessage<Person>> context )
            {
                var json = context.Message.ToJson();
                WriteToDebug( QueueName, "IEntityWasUpdatedMessage<Person>", json );
                return Task.Delay( 0 );
            }

            /// <summary>
            /// Consumes the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns></returns>
            public Task Consume( ConsumeContext<EntityWasUpdatedMessage<Person>> context )
            {
                var json = context.Message.ToJson();
                WriteToDebug( QueueName, "EntityWasUpdatedMessage<Person>", json );
                return Task.Delay( 0 );
            }
        }

        /// <summary>
        /// Cache Queue Debug
        /// </summary>
        public class CacheUpdate :
            IRockConsumer<CacheQueue, ICacheWasUpdatedMessage<StepTypeCache>>
        {
            private const string QueueName = nameof( CacheQueue );

            /// <summary>
            /// Consumes the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns></returns>
            public Task Consume( ConsumeContext<ICacheWasUpdatedMessage<StepTypeCache>> context )
            {
                var json = context.Message.ToJson();
                WriteToDebug( QueueName, "ICacheWasUpdatedMessage<StepTypeCache>", json );
                return Task.Delay( 0 );
            }
        }

        /// <summary>
        /// Writes to debug.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageJson">The json.</param>
        public static void WriteToDebug( string queueName, string messageType, string messageJson )
        {
            Debug.WriteLine( $"==================\nQueue: {queueName}\nMessageType: {messageType}\n{messageJson}" );
        }
    }
}
