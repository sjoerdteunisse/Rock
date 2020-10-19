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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Transactions
{
    /// <summary>
    /// Bus Started Transaction
    /// </summary>
    public abstract class BusStartedTransaction<TMessage> : IRockConsumer<StartTaskQueue, TMessage>
        where TMessage : class, IRockMessage
    {
        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task Consume( ConsumeContext<TMessage> context )
        {
            var json = context.Message.ToJson();
            Debug.WriteLine( $"==================\n{GetType().Name}\n{json}" );

            Execute( context.Message );
            return Task.Delay( 0 );
        }

        /// <summary>
        /// Generate messages from the instance properties.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<TMessage> GetMessages();

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute( TMessage message );

        /// <summary>
        /// Sends the messages.
        /// </summary>
        public async void Send()
        {
            var messages = GetMessages();

            if ( messages?.Any() != true )
            {
                return;
            }

            foreach ( var message in messages )
            {
                await RockMessageBus.SendOnStartTaskQueue( message );
            }
        }
    }
}
