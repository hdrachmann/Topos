﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Topos.Tests.Contracts.Extensions
{
    public static class TestExtensions
    {
        public static IEnumerable<IReadOnlyCollection<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            var list = new List<T>(batchSize);

            foreach (var item in items)
            {
                list.Add(item);

                if (list.Count < batchSize) continue;

                yield return list.ToArray();

                list.Clear();
            }

            if (list.Any())
            {
                yield return list.ToArray();
            }
        }

        public static void WaitOrDie(this ManualResetEvent manualResetEvent, int timeoutSeconds = 10, string errorMessage = null)
        {
            if (manualResetEvent.WaitOne(TimeSpan.FromSeconds(timeoutSeconds))) return;

            throw new TimeoutException($"The reset event was not set within {timeoutSeconds} s timeout - {errorMessage ?? "no additional details were included"}");
        }

        public static async Task WaitOrDie<T>(this ConcurrentQueue<T> queue,
            Expression<Func<ConcurrentQueue<T>, bool>> completionExpression,
            Expression<Func<ConcurrentQueue<T>, bool>> failExpression = null,
            int timeoutSeconds = 5, Func<string> failureDetailsFunction = null)
        {
            failExpression = failExpression ?? (_ => false);
            var completionPredicate = completionExpression.Compile();
            var failPredicate = failExpression.Compile();
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var stopwatch = Stopwatch.StartNew();
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (failPredicate(queue))
                    {
                        var details = failureDetailsFunction?.Invoke();

                        throw new ApplicationException($@"Waiting for

    {completionExpression}

on queue failed, because the failure expression

    {failExpression}

was satisfied after {stopwatch.Elapsed.TotalSeconds:0.0} s.

Details:

{details ?? "NONE"}");
                    }
                    if (completionPredicate(queue)) return;

                    await Task.Delay(117, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
            }

            throw new TimeoutException($@"Waiting for

    {completionExpression}

on queue did not complete in {timeoutSeconds} s");
        }
    }
}