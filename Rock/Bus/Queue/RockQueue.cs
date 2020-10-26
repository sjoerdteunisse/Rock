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
using System.Collections.Generic;

namespace Rock.Bus.Queue
{
    /// <summary>
    /// Queue Interface
    /// </summary>
    public interface IRockQueue
    {
        /// <summary>
        /// Gets the name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the Rock instance specific name. This name is unique for this queue and this instance of Rock.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string InstanceSpecificName { get; }

        /// <summary>
        /// Gets the time to live seconds.
        /// </summary>
        /// <value>
        /// The time to live seconds.
        /// </value>
        int TimeToLiveSeconds { get; }

        /// <summary>
        /// Gets a value indicating whether this queue broadcasts messages or delivers them to a single Rock instance.
        /// Broadcasting is good for events that need to be known by all Rock instances like cache invalidation.
        /// Not broadcasting is better for things like starting jobs where only one instance needs to execute the job.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is broadcast; otherwise, <c>false</c>.
        /// </value>
        bool IsBroadcast { get; }
    }

    /// <summary>
    /// Queue Abstract Class
    /// </summary>
    public abstract class RockQueue : IRockQueue
    {
        /// <summary>
        /// Gets the queue name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Gets the Rock instance specific name. This name is unique for this queue and this instance of Rock.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string InstanceSpecificName => $"{Name}_{RockMessageBus.RockInstanceGuid}";

        /// <summary>
        /// Gets the time to live seconds.
        /// </summary>
        /// <value>
        /// The time to live seconds.
        /// </value>
        public virtual int TimeToLiveSeconds => 5 * 60;

        /// <summary>
        /// Gets a value indicating whether this queue broadcasts messages or delivers them to a single Rock instance.
        /// Broadcasting is good for events that need to be known by all Rock instances like cache invalidation.
        /// Not broadcasting is better for things like starting jobs where only one instance needs to execute the job.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is broadcast; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsBroadcast => true;

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <returns></returns>
        public static IRockQueue Get<TQueue>()
        {
            return Get( typeof( TQueue ) );
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <returns></returns>
        public static IRockQueue Get( Type queueType )
        {
            var key = queueType.FullName;
            var queue = _queues.GetValueOrNull( key );

            if ( queue == null )
            {
                queue = Activator.CreateInstance( queueType ) as IRockQueue;
                _queues[key] = queue;
            }

            return queue;
        }
        private static Dictionary<string, IRockQueue> _queues = new Dictionary<string, IRockQueue>();

        /// <summary>
        /// Gets the name for configuration.
        /// </summary>
        /// <returns></returns>
        internal static string GetNameForConfiguration( IRockQueue queue )
        {
            return queue.IsBroadcast ? queue.InstanceSpecificName : queue.Name;
        }
    }
}
