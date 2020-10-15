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
using System.Collections.Concurrent;
using System.Linq;
using Rock.Bus;
using Rock.Bus.Message;

namespace Rock.Transactions
{
    /// <summary>
    /// The internal queue used to hold the transactions to run in <see cref="RockQueue"/>
    /// </summary>
    public sealed class RockQueueInternalQueue : ConcurrentQueue<ITransaction>
    {
        /// <summary>
        /// If the transaction is a IEventBusTransaction, then the transaction is sent on the bus. Otherwise,
        /// adds an object to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.
        /// </summary>
        /// <param name="item">The object to add to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.
        /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
        public new void Enqueue( ITransaction item )
        {
            if ( item is IBusStartedTransaction busStartedTransaction )
            {
                busStartedTransaction.Send().Wait();
                return;
            }

            base.Enqueue( item );
        }
    }

    /// <summary>
    /// The Rock Queue
    /// </summary>
    static public class RockQueue
    {
        /// <summary>
        /// Gets the currently executing transaction progress. This should be between 0 and 100
        /// percent or null if the progress cannot be reported.
        /// </summary>
        /// <value>
        /// The currently executing transaction progress.
        /// </value>
        public static int? CurrentlyExecutingTransactionProgress { get; private set; }

        /// <summary>
        /// The currently executing transaction.
        /// </summary>
        /// <value>
        /// The currently executing transaction.
        /// </value>
        public static ITransaction CurrentlyExecutingTransaction { get; private set; }

        /// <summary>
        /// Gets or sets the transaction queue.
        /// </summary>
        /// <value>
        /// The transaction queue.
        /// </value>
        public static RockQueueInternalQueue TransactionQueue { get; set; }

        /// <summary>
        /// Drains this queue.
        /// </summary>
        /// <param name="errorHandler">The error handler.</param>
        public static void Drain( Action<Exception> errorHandler )
        {
            while ( TransactionQueue.TryDequeue( out var transaction ) )
            {
                CurrentlyExecutingTransaction = transaction;
                CurrentlyExecutingTransactionProgress = null;

                if ( CurrentlyExecutingTransaction == null )
                {
                    continue;
                }

                try
                {
                    if ( CurrentlyExecutingTransaction is ITransactionWithProgress )
                    {
                        var transactionWithProgress = CurrentlyExecutingTransaction as ITransactionWithProgress;

                        if ( transactionWithProgress.Progress != null )
                        {
                            transactionWithProgress.Progress.ProgressChanged += ( reporter, progress ) =>
                            {
                                CurrentlyExecutingTransactionProgress = progress;
                            };
                        }
                    }

                    CurrentlyExecutingTransaction.Execute();
                }
                catch ( Exception ex )
                {
                    errorHandler( new Exception( string.Format( "Exception in Global.DrainTransactionQueue(): {0}", transaction.GetType().Name ), ex ) );
                }
                finally
                {
                    CurrentlyExecutingTransactionProgress = 100;
                }
            }
        }

        /// <summary>
        /// Determines whether a transaction of a certain type is being run.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///   <c>true</c> if [has transaction of type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsExecuting<T>() where T : ITransaction 
        {
            return CurrentlyExecutingTransaction?.GetType() == typeof( T );
        }

        /// <summary>
        /// Determines whether a transaction of a certain type is in the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///   <c>true</c> if [has transaction of type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInQueue<T>() where T : ITransaction
        {
            return TransactionQueue.Any( t => t.GetType() == typeof( T ) );
        }

        /// <summary>
        /// Initializes the <see cref="RockQueue" /> class.
        /// </summary>
        static RockQueue()
        {
            TransactionQueue = new RockQueueInternalQueue();
        }
    }
}