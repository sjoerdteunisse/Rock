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
using MassTransit.Transports;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Bus.Observer
{
    /// <summary>
    /// Rock Observer Interface.
    /// Allows observing messages (and not consuming them), which allows other server instances to also observe them.
    /// </summary>
    public interface IRockObserver
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        IRockObserver Instance { get; }
    }

    /// <summary>
    /// Rock Observer Interface.
    /// Allows observing messages (and not consuming them), which allows other server instances to also observe them.
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="IConsumeMessageObserver{TMessage}" />
    public interface IRockObserver<TQueue, TMessage> : IRockObserver, IReceiveObserver
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
    }

    /// <summary>
    /// Rock Observer.
    /// Allows observing messages (and not consuming them), which allows other server instances to also observe them.
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="IRockObserver{TQueue, TMessage}" />
    public abstract class RockObserver<TQueue, TMessage> : IRockObserver<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// The context
        /// </summary>
        protected PublishContext<TMessage> _publishContext = null;

        /// <summary>
        /// Observes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void Observe( TMessage message );

        /// <summary>
        /// Called before the message is consumed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task PrePublish( PublishContext<TMessage> context )
        {
            _publishContext = context;
            Observe( context.Message );
            return Task.Delay( 0 );
        }

        /// <summary>
        /// Called after the message is consumed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task PostPublish( PublishContext<TMessage> context )
        {
            return Task.Delay( 0 );
        }

        /// <summary>
        /// Called on a fault.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public Task PublishFault( PublishContext<TMessage> context, Exception exception )
        {
            return Task.Delay( 0 );
        }

        public Task PreReceive( ReceiveContext context )
        {
            return Task.Delay( 0 );
        }

        public Task PostReceive( ReceiveContext context )
        {
            return Task.Delay( 0 );
        }

        public Task PostConsume<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType ) where T : class
        {
            return Task.Delay( 0 );
        }

        public Task ConsumeFault<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception ) where T : class
        {
            return Task.Delay( 0 );
        }

        public Task ReceiveFault( ReceiveContext context, Exception exception )
        {
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

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public virtual IRockObserver Instance => Activator.CreateInstance( GetType() ) as IRockObserver<TQueue, TMessage>;
    }

    /// <summary>
    /// Rock Message Bus Observer Helpers
    /// </summary>
    public static class RockObserver
    {
        /// <summary>
        /// The generic interface type. Rock observers all implement this interface.
        /// </summary>
        private static readonly Type _genericInterfaceType = typeof( IRockObserver<,> );

        /// <summary>
        /// Configures the rock observers.
        /// </summary>
        /// <param name="bus">The bus.</param>
        public static void ConfigureRockObservers( IBusControl bus )
        {
            var observerTypes = GetObserverTypes();

            foreach ( var observerType in observerTypes )
            {
                var observer = Activator.CreateInstance( observerType ) as IRockObserver;
                bus.ConnectReceiveObserver( observer.Instance as IReceiveObserver );
            }
        }

        /// <summary>
        /// Gets the observer types for this Rock instance.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetObserverTypes()
        {
            var observerTypes = new Dictionary<string, Type>();
            var assemblies = Reflection.GetRockAndPluginAssemblies();
            var types = assemblies
                .SelectMany( a => a.GetTypes()
                .Where( t => t.IsClass && ( t.IsPublic || t.IsNestedPublic ) ) );

            foreach ( var type in types )
            {
                if ( IsRockObserver( type ) )
                {
                    observerTypes.AddOrIgnore( type.FullName, type );
                }
            }

            var observerTypeList = observerTypes.Select( kvp => kvp.Value ).ToList();
            return observerTypeList;
        }

        /// <summary>
        /// Determines whether [is rock observer] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock observer] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockObserver( Type type )
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
        /// Gets the queue type for the observer type.
        /// </summary>
        /// <param name="observerType">Type of the consumer.</param>
        /// <returns></returns>
        public static Type GetQueueType( Type observerType )
        {
            var queueInterface = typeof( IRockQueue );
            var typeInterfaces = observerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    foreach ( var genericTypeArgument in typeInterface.GenericTypeArguments )
                    {
                        if ( genericTypeArgument.GetInterfaces().Contains( queueInterface ) )
                        {
                            return genericTypeArgument;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the message type for the observer type.
        /// </summary>
        /// <param name="observerType">Type of the consumer.</param>
        /// <returns></returns>
        public static Type GetMessageType( Type observerType )
        {
            // Target of the search
            var messageInterface = typeof( IRockMessage<> );

            // The observer type is a type that implements IRockObserver<TQueue, TMessage>
            var typeInterfaces = observerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                // The target generic interface is IRockObserver<>
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There are two type arguments. The first is the queue, and the second is the message
                    return typeInterface.GenericTypeArguments[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the queue for the observer type.
        /// </summary>
        /// <param name="observerType">Type of the consumer.</param>
        /// <returns></returns>
        public static IRockQueue GetQueue( Type observerType )
        {
            var queueType = GetQueueType( observerType );
            return Activator.CreateInstance( queueType ) as IRockQueue;
        }
    }
}
