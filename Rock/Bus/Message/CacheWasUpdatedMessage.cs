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
using System.Runtime.CompilerServices;
using Rock.Bus.Queue;
using Rock.Web.Cache;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Cache Update Message
    /// </summary>
    public interface ICacheWasUpdatedMessage : IRockMessage<CacheQueue>
    {
        /// <summary>
        /// Gets the cache name.
        /// </summary>
        string CacheName { get; set; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        /// <value>
        /// The event.
        /// </value>
        string EventType { get; set; }
    }

    /// <summary>
    /// Cache Update Message
    /// </summary>
    public interface ICacheWasUpdatedMessage<TCache> : ICacheWasUpdatedMessage
        where TCache : IItemCache
    {
    }

    /// <summary>
    /// Cache Update Message
    /// </summary>
    public class CacheWasUpdatedMessage<TCache> : ICacheWasUpdatedMessage<TCache>
        where TCache : IItemCache
    {
        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        public string CacheName { get; set; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the expiration time.
        /// </summary>
        /// <value>
        /// The expiration time.
        /// </value>
        public int? __ExpirationTime { get; set; }

        /// <summary>
        /// Publishes the specified entity.
        /// </summary>
        /// <param name="callerMethodName">Name of the caller method.</param>
        public static void Publish( [CallerMemberName] string callerMethodName = null )
        {
            var itemType = typeof( TCache );

            var messageType = typeof( CacheWasUpdatedMessage<> ).MakeGenericType( itemType );
            var message = Activator.CreateInstance( messageType ) as ICacheWasUpdatedMessage;

            message.CacheName = itemType.FullName;
            message.EventType = callerMethodName;

            _ = RockMessageBus.Publish( message, messageType );
        }
    }
}
