// ReSharper disable CollectionNeverQueried.Local
namespace BlogSampleCodes.ConcurrentStructurePerformanceCheck;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

internal class DictionaryChecker
{
    private static Stopwatch? Stopwatch { get; set; }

    private static long GetTotalAddTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        Stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < count; i++)
        {
            dict.TryAdd(i, i);
        }
        var elapsedTick = Stopwatch.ElapsedTicks;
        return elapsedTick;
    }
    
    private static List<long> GetEachAddTimes(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        var result = Enumerable.Range(0, count).Select(index =>
        {
            if (index == 409203)
            {
                return GetAddTime(dict, index, index);
            }
            
            return GetAddTime(dict, index, index);
        }).ToList();
        return result;
    } 

    private static long GetAddTime(IDictionary<int, int> dict, int key, int value)
    {
        Stopwatch = Stopwatch.StartNew();
        dict.TryAdd(key, value);
        var elapsedTick = Stopwatch.ElapsedTicks;
        if (elapsedTick > TimeSpan.TicksPerMillisecond)
        {
            Console.WriteLine("asdadad");
        }
        return elapsedTick;
    }
    
    private static long GetTotalRemoveTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        foreach (var i in Enumerable.Range(0, count))
        {
            dict.Add(i, i);
        }
        
        Stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) 
        {
            dict.Remove(i);
        }
        var elapsedTick = Stopwatch.ElapsedTicks;
        return elapsedTick;
    }

    private static List<long> GetEachRemoveTimes(IDictionary<int, int> dict, int count) 
    {
        dict.Clear();
        foreach (var i in Enumerable.Range(0, count))
        {
            dict.Add(i, i);
        }
        
        var result = dict.
            Select(pair => GetRemoveTime(dict, pair.Key)).
            ToList();
        return result;
    }
    
    private static long GetRemoveTime(IDictionary<int, int> dict, int key)
    {
        Stopwatch = Stopwatch.StartNew();
        dict.Remove(key);
        var elapsedTick = Stopwatch.ElapsedTicks;
        return elapsedTick;
    }

    private static long GetIterationTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        foreach (var i in Enumerable.Range(0, count))
        {
            dict.Add(i, i);
        }
        Stopwatch = Stopwatch.StartNew();
        var list = dict.Values.ToList();
        var elapsedTick = Stopwatch.ElapsedTicks;
        return elapsedTick;
    }

    public static void GetResult() 
    {
        var thresholdTick = TimeSpan.TicksPerMillisecond;
        var elemCount = 1024*1024;
        var repeatCount = 1;
        // add statistic
        {
            var initialCapacity = 1024*1024;
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: initialCapacity);
            var concurrentResult = GetEachAddTimes(concurrentDict, count: elemCount);
            var concurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => 
                {
                    var dict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: initialCapacity);
                    return GetTotalAddTime(dict, elemCount);
                }).
                Average() / TimeSpan.TicksPerMillisecond;

            var nonConcurrentDict = new Dictionary<int, int>(capacity: initialCapacity);
            var nonConcurrentResult = GetEachAddTimes(nonConcurrentDict, count: elemCount);
            var nonConcurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => 
                {
                    var dict = new Dictionary<int, int>(capacity: initialCapacity);
                    return GetTotalAddTime(dict, elemCount);
                }).
                Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"InitialCapacity,{initialCapacity}\n");
            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");
            stringBuilder.Append($"Max,{concurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond},{nonConcurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond}\n");
            foreach (var (index, resultPair) in concurrentResult.Zip(nonConcurrentResult).Select((pair,index) =>(index,pair))) 
            {
                if (resultPair.First > thresholdTick || resultPair.Second > thresholdTick)
                {
                    stringBuilder.Append($"{index + 1},{resultPair.First / (double)TimeSpan.TicksPerMillisecond},{resultPair.Second / (double)TimeSpan.TicksPerMillisecond}\n");
                }
            }

            Save(Directory.GetCurrentDirectory(), $"dict_capacity{initialCapacity}_add_result", stringBuilder.ToString());
        }

        /*
        // remove statistic
        {
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: 1);
            var concurrentResult = GetEachRemoveTimes(concurrentDict, count: elemCount);
            var concurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => GetTotalRemoveTime(concurrentDict, elemCount)).Average() / TimeSpan.TicksPerMillisecond;

            var nonConcurrentDict = new Dictionary<int, int>();
            var nonConcurrentResult = GetEachRemoveTimes(nonConcurrentDict, count: elemCount);
            var nonConcurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => GetTotalRemoveTime(nonConcurrentDict, elemCount)).Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");
            stringBuilder.Append($"Max,{concurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond},{nonConcurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond}\n");
            foreach (var (index, resultPair) in concurrentResult.Zip(nonConcurrentResult).Select((pair, index) => (index, pair)))
            {
                if (resultPair.First > thresholdTick || resultPair.Second > thresholdTick)
                {
                    stringBuilder.Append($"{index + 1},{resultPair.First / (double)TimeSpan.TicksPerMillisecond},{resultPair.Second / (double)TimeSpan.TicksPerMillisecond}\n");
                }
            }

            Save(Directory.GetCurrentDirectory(), "dict_remove_result", stringBuilder.ToString());
        }
        // iterate statistic
        {
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: 1);
            var nonConcurrentDict = new Dictionary<int, int>();

            for(var i = 0; i < elemCount; i++)
            {
                concurrentDict.TryAdd(i, i);
                nonConcurrentDict.TryAdd(i, i);
            }

            var concurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => GetIterationTime(concurrentDict, elemCount)).Average() / TimeSpan.TicksPerMillisecond;
            var nonConcurrentTotalTime = Enumerable.Range(0, repeatCount).
                Select(_ => GetIterationTime(nonConcurrentDict, elemCount)).Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");

            Save(Directory.GetCurrentDirectory(), "dict_iter_result", stringBuilder.ToString());
        }
        */
    }

    private static void Save(string folder,string filename, string text)
    {
        using TextWriter writer = new StreamWriter($"{folder}/{filename}.csv", false, Encoding.UTF8);
        writer.Write(text);
    }
 
}
