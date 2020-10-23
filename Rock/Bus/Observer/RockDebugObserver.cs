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
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Model;

namespace Rock.Bus.Observer
{
    /// <summary>
    /// Abstract Debug Observer
    /// </summary>
    public abstract class RockDebugObserver<TQueue, TMessage> : RockObserver<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Observe( TMessage message )
        {
            var messageJson = message.ToJson();
            var queueName = GetQueue().Name;
            var messageType = typeof( TMessage ).FullName;
            var observerName = GetType().FullName;

            Debug.WriteLine( $"==================\nObserver: {observerName}\nQueue: {queueName}\nMessageType: {messageType}\n{messageJson}" );
        }
    }

    /// <summary>
    /// Person Was Updated
    /// </summary>
    public class FirstPersonWasUpdatedObserver : RockDebugObserver<EntityUpdateQueue, EntityWasUpdatedMessage<Person>>
    {
    }

    /// <summary>
    /// Person Was Updated
    /// </summary>
    public class SecondPersonWasUpdatedObserver : RockDebugObserver<EntityUpdateQueue, EntityWasUpdatedMessage<Person>>
    {
    }

    /// <summary>
    /// Person Was Updated
    /// </summary>
    public class FirstGroupWasUpdatedObserver : RockDebugObserver<EntityUpdateQueue, EntityWasUpdatedMessage<Group>>
    {
    }
}
