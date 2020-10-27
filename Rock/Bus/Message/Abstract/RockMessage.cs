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

using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// A Rock Bus Message
    /// </summary>
    public interface IRockMessage<TQueue>
        where TQueue : IRockQueue, new()
    {
    }

    /// <summary>
    /// Rock Message Static Helpers
    /// </summary>
    public static class RockMessage
    {
        /// <summary>
        /// Gets the log string.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string GetLogString<TQueue>( IRockMessage<TQueue> message )
            where TQueue : IRockQueue, new()
        {
            var messageJson = message.ToJson();
            var queueName = RockQueue.Get<TQueue>().Name;
            var messageType = message.GetType().FullName;

            return $"Queue: {queueName}\nMessageType: {messageType}\n{messageJson}";
        }
    }
}
