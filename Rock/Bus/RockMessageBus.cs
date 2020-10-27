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

using MassTransit;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Bus.Transport;
using Rock.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rock.Bus
{
    /// <summary>
    /// Rock Bus Process Controls: Start the bus
    /// </summary>
    public static class RockMessageBus
    {
        /// <summary>
        /// Is the bus started?
        /// </summary>
        private static bool _isBusStarted = false;

        /// <summary>
        /// The bus
        /// </summary>
        private static IBusControl _bus = null;

        /// <summary>
        /// The transport component
        /// </summary>
        private static TransportComponent _transportComponent = null;

        /// <summary>
        /// The Rock instance unique identifier
        /// </summary>
        public static readonly Guid RockInstanceGuid = Guid.NewGuid();

        /// <summary>
        /// Starts this bus.
        /// </summary>
        public static async Task Start()
        {
            var components = TransportContainer.Instance.Components.Select( c => c.Value.Value );
            _transportComponent = components.FirstOrDefault( c => c.IsActive ) ?? components.FirstOrDefault( c => c is InMemory );

            if ( _transportComponent == null )
            {
                throw new ConfigurationException( "An active transport component is required for Rock to run correctly" );
            }

            _bus = _transportComponent.GetBusControl( RockConsumer.ConfigureRockConsumers );
            await _bus.StartAsync();
            _isBusStarted = true;
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task Publish<TQueue, TMessage>( TMessage message )
            where TQueue : IPublishEventQueue, new()
            where TMessage : class, IEventMessage<TQueue>
        {
            await Publish( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task Publish<TQueue>( IEventMessage<TQueue> message, Type messageType )
            where TQueue : IPublishEventQueue, new()
        {
            if ( !IsReady() )
            {
                ExceptionLogService.LogException( $"A message was published before the message bus was ready: {RockMessage.GetLogString( message )}" );
                return;
            }

            await _bus.Publish( message, messageType, context =>
            {
                context.TimeToLive = RockQueue.GetTimeToLive<TQueue>();
            } );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task Send<TQueue, TMessage>( TMessage message )
            where TQueue : ISendCommandQueue, new()
            where TMessage : class, ICommandMessage<TQueue>
        {
            await Send( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Sends the command message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task Send<TQueue>( ICommandMessage<TQueue> message, Type messageType )
            where TQueue : ISendCommandQueue, new()
        {
            if ( !IsReady() )
            {
                ExceptionLogService.LogException( $"A message was sent before the message bus was ready: {RockMessage.GetLogString( message )}" );
                return;
            }

            var queue = RockQueue.Get<TQueue>();
            var queueName = queue.NameForConfiguration;
            var endpoint = _transportComponent.GetSendEndpoint( _bus, queueName );

            await endpoint.Send( message, messageType, context =>
            {
                context.TimeToLive = RockQueue.GetTimeToLive( queue );
            } );
        }

        /// <summary>
        /// Determines whether this instance is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsReady()
        {
            return _isBusStarted && _transportComponent != null && _bus != null;
        }
    }
}
