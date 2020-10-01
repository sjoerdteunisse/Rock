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

namespace Rock.Bus.Message
{
    /// <summary>
    /// Entity Update Message
    /// </summary>
    public interface IEntityWasUpdatedMessage : IRockMessage
    {
        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        int EntityTypeId { get; set; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        int EntityId { get; set; }

        /// <summary>
        /// Gets the state of the entity.
        /// </summary>
        string EntityState { get; set; }
    }

    /// <summary>
    /// Entity Update Message
    /// </summary>
    public class EntityWasUpdatedMessage: IEntityWasUpdatedMessage
    {
        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets the state of the entity.
        /// </summary>
        public string EntityState { get; set; }
    }
}
