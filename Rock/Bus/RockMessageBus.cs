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
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task Publish<TQueue, TMessage>( TMessage message )
            where TQueue : IRockQueue, new()
            where TMessage : class, IRockMessage<TQueue>
        {
            await Publish( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task Publish<TQueue>( IRockMessage<TQueue> message, Type messageType )
            where TQueue : IRockQueue, new()
        {
            if ( !IsReady() )
            {
                return;
            }

            var queue = RockQueue.Get<TQueue>();
            await _bus.Publish( message, messageType, context =>
            {
                context.TimeToLive = TimeSpan.FromSeconds( queue.TimeToLiveSeconds );
            } );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task Send<TQueue, TMessage>( TMessage message )
            where TQueue : IRockQueue, new()
            where TMessage : class, IRockMessage<TQueue>
        {
            await Send( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task Send<TQueue>( IRockMessage<TQueue> message, Type messageType )
            where TQueue : IRockQueue, new()
        {
            if ( !IsReady() )
            {
                return;
            }

            var queue = RockQueue.Get<TQueue>();
            var endpoint = _transportComponent.GetSendEndpoint( _bus, queue.Name );
            await endpoint.Send( message, messageType, context =>
            {
                context.TimeToLive = TimeSpan.FromSeconds( queue.TimeToLiveSeconds );
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
            return _transportComponent != null && _bus != null;
        }
    }
}
