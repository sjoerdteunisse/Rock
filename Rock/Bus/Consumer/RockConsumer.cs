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
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Bus.Consumer
{
    /// <summary>
    /// Rock Consumer Interface
    /// </summary>
    /// <seealso cref="IConsumer" />
    public interface IRockConsumer<TQueue, TMessage> : IConsumer<TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
    }

    /// <summary>
    /// Rock Consumer
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="Rock.Bus.Consumer.IRockConsumer{TQueue, TMessage}" />
    public abstract class RockConsumer<TQueue, TMessage> : IRockConsumer<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// The context
        /// </summary>
        protected ConsumeContext<TMessage> _consumeContext = null;

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void Consume( TMessage message );

        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual Task Consume( ConsumeContext<TMessage> context )
        {
            _consumeContext = context;
            Consume( context.Message );
            return Task.Delay( 0 );
        }

        /// <summary>
        /// Gets an instance of the queue.
        /// </summary>
        /// <returns></returns>
        public static IRockQueue GetQueue()
        {
            if ( _queue == null )
            {
                _queue = Activator.CreateInstance<TQueue>();
            }

            return _queue;
        }
        private static IRockQueue _queue = null; 
    }

    /// <summary>
    /// Rock Message Bus Consumer Helpers
    /// </summary>
    public static class RockConsumer
    {
        private static readonly Type _genericInterfaceType = typeof( IRockConsumer<,> );

        /// <summary>
        /// Configures the rock consumers.
        /// </summary>
        public static void ConfigureRockConsumers( IBusFactoryConfigurator configurator )
        {
            var consumersByQueue = GetConsumerTypes()
                .GroupBy( c => GetQueue( c )?.Name )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var queueNames = consumersByQueue.Keys;

            foreach ( var queueName in queueNames )
            {
                var consumers = consumersByQueue[queueName];
                var queue = GetQueue( consumers.First() );

                if ( queue != null )
                {
                    configurator.ReceiveEndpoint( queue.Name, e =>
                    {
                        consumers.ForEach( c => e.Consumer( c, Activator.CreateInstance ) );
                    } );
                }
            }
        }

        /// <summary>
        /// Gets the consumer types.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetConsumerTypes()
        {
            var consumerTypes = new Dictionary<string, Type>();
            var assemblies = Reflection.GetRockAndPluginAssemblies();
            var types = assemblies
                .SelectMany( a => a.GetTypes()
                .Where( t => t.IsClass && ( t.IsPublic || t.IsNestedPublic ) ) );

            foreach ( var type in types )
            {
                if ( IsRockConsumer( type ) )
                {
                    consumerTypes.AddOrIgnore( type.FullName, type );
                }
            }

            var consumerTypeList = consumerTypes.Select( kvp => kvp.Value ).ToList();
            return consumerTypeList;
        }

        /// <summary>
        /// Determines if the type is a Rock Consumer type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock consumer] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockConsumer( Type type )
        {
            if ( type.IsAbstract || type.ContainsGenericParameters )
            {
                return false;
            }

            var typeInterfaces = type.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the queue for the consumer type.
        /// </summary>
        /// <param name="consumerType">Type of the consumer.</param>
        /// <returns></returns>
        public static IRockQueue GetQueue( Type consumerType )
        {
            var queueInterface = typeof( IRockQueue );
            var typeInterfaces = consumerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    foreach ( var genericTypeArgument in typeInterface.GenericTypeArguments )
                    {
                        if ( genericTypeArgument.GetInterfaces().Contains( queueInterface ) )
                        {
                            return Activator.CreateInstance( genericTypeArgument ) as IRockQueue;
                        }
                    }
                }
            }

            return null;
        }
    }
}
