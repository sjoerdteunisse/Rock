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

namespace Rock.Bus.Consumer
{
    /// <summary>
    /// Entity Update Consumer
    /// </summary>
    public class RockMessageConsumer : IRockConsumer<StartTaskQueue, IRockMessage>, IRockConsumer<EntityUpdateQueue, IRockMessage>
    {
        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Consume( ConsumeContext<IRockMessage> context )
        {
            var json = context.Message.ToJson();
            Debug.WriteLine( $"==================\nRockMessageConsumer\n{context.ReceiveContext.InputAddress}\n{json}" );
            return Task.Delay( 0 );
        }
    }
}
