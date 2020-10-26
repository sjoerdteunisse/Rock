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

namespace Rock.Bus.Queue
{
    /// <summary>
    /// A Rock Message Bus Queue for Starting Tasks
    /// </summary>
    public sealed class StartTaskQueue : RockQueue
    {
        /// <summary>
        /// Gets the queue name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => "rock-start-task-queue";

        /// <summary>
        /// Gets a value indicating whether this queue broadcasts messages or delivers them to a single Rock instance.
        /// Broadcasting is good for events that need to be known by all Rock instances like cache invalidation.
        /// Not broadcasting is better for things like starting jobs where only one instance needs to execute the job.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is broadcast; otherwise, <c>false</c>.
        /// </value>
        public override bool IsBroadcast => false;
    }
}
