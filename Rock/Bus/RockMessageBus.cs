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
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        /// The of an entity that will cause publishing a message on the <see cref="RockMessageBus"/>
        /// </summary>
        private static readonly HashSet<EntityState> _statesToPublishOnBus = new HashSet<EntityState> {
            EntityState.Added,
            EntityState.Modified,
            EntityState.Deleted
        };

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
        /// The start task queue
        /// </summary>
        private static StartTaskQueue _startTaskQueue = new StartTaskQueue();

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
                await _bus.StartAsync();
            }
            catch ( Exception e )
            {
                throw new ConfigurationException( "The Message Bus is required for Rock to run correctly, but it did not initialize correctly", e );
            }
        }

        /// <summary>
        /// Should entity updates be published for this entity type.
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static bool ShouldPublishEntityUpdate( int entityTypeId, EntityState entityState )
        {
            return
                _statesToPublishOnBus.Contains( entityState ) &&
                ( EntityTypeCache.Get( entityTypeId )?.IsMessageBusEventPublishEnabled ?? false );
        }

        /// <summary>
        /// Publishes the entity update.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task PublishEntityUpdate( IEntityWasUpdatedMessage message )
        {
            if ( !IsReady() )
            {
                return;
            }

            await _bus.Publish( message );
        }

        /// <summary>
        /// Publishes the entity update.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task SendStartTask( IEventBusTransaction message )
        {
            if ( !IsReady() )
            {
                return;
            }

            var endpoint = _sendEndpoints.GetValueOrNull( _startTaskQueue.Name );

            if ( endpoint == null )
            {
                endpoint = _transportComponent.GetSendEndpoint( _bus, _startTaskQueue.Name );
                _sendEndpoints[_startTaskQueue.Name] = endpoint;
            }

            await endpoint.Send( message );
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
