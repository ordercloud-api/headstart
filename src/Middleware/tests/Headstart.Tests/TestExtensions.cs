using NSubstitute.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Tests
{
    public static class TestExtensions
    {
        public static Task<T> ToTask<T>(this T self)
        {
            return Task.FromResult(self);
        }

        // use this on .Returns when you want to return the first argument
        // passed to the calling function for example on Saves or Creates generally
        public static Task<T> FirstArgument<T>(this CallInfo args)
        {
            return args.ArgAt<T>(0).ToTask();
        }

        // pick a single random item out of a list
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        // pick count number of items out of list
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        // re-arrange items in a list
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
