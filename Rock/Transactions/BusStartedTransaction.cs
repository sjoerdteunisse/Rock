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
using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Transactions
{
    /// <summary>
    /// Bus Started Transaction
    /// </summary>
    public abstract class BusStartedTransaction<TMessage> : RockConsumer<StartTaskQueue, TMessage>
        where TMessage : class, ICommandMessage<StartTaskQueue>
    {
        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( TMessage message )
        {
            Execute( message );
        }

        /// <summary>
        /// Generate messages from the instance properties that should be sent on the
        /// message bus so that a Rock instance can execute the transactions.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<TMessage> GetMessagesToSend();

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute( TMessage message );

        /// <summary>
        /// Sends the messages.
        /// </summary>
        public void Send()
        {
            var messages = GetMessagesToSend();

            if ( messages?.Any() != true )
            {
                return;
            }

            foreach ( var message in messages )
            {
                _ = RockMessageBus.Send<StartTaskQueue, TMessage>( message );
            }
        }
    }
}
