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
using Rock.Bus.Observer;
using Rock.Bus.Queue;
using Rock.Bus.Transport;
using Rock.Model;
using System;
using System.Collections.Generic;
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
        /// The send endpoints
        /// </summary>
        private static Dictionary<string, ISendEndpoint> _sendEndpoints = new Dictionary<string, ISendEndpoint>();

        /// <summary>
        /// The bus
        /// </summary>
        private static IBusControl _bus = null;

        /// <summary>
        /// The transport component
        /// </summary>
        private static TransportComponent _transportComponent = null;

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

            try
            {
                _bus = _transportComponent.GetBusControl( RockConsumer.ConfigureRockConsumers );
                RockObserver.ConfigureRockObservers( _bus );
                await _bus.StartAsync();
            }
            catch ( Exception e )
            {
                throw new ConfigurationException( "The Message Bus is required for Rock to run correctly, but it did not initialize correctly", e );
            }
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

            ApplyMessageHeaders( message );
            await _bus.Publish( message, messageType );
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
            var endpoint = _sendEndpoints.GetValueOrNull( queue.Name );

            if ( endpoint == null )
            {
                endpoint = _transportComponent.GetSendEndpoint( _bus, queue.Name );
                _sendEndpoints[queue.Name] = endpoint;
            }

            ApplyMessageHeaders( message );
            await endpoint.Send( message, messageType );
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

        /// <summary>
        /// Applies the queue message headers.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        private static void ApplyMessageHeaders<TQueue>( IRockMessage<TQueue> message )
            where TQueue : IRockQueue, new()
        {
            var queue = RockQueue.Get<TQueue>();
            message.__ExpirationTime = RockDateTime.Now.AddSeconds( queue.TimeToLiveSeconds );
        }
    }
}
