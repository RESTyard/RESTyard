using System;

namespace Bluehands.Hypermedia.Client.Extensions
{
    public static class PatternMatchExtensions
    {
        public static T TypeMatch<TBase, TDerived1, TDerived2, T>(this TBase target, Func<TDerived1, T> handle1, Func<TDerived2, T> handle2)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    return handle1(case1);
                case TDerived2 case2:
                    return handle2(case2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static T TypeMatch<TBase, TDerived1, TDerived2, TDerived3, T>(this TBase target, Func<TDerived1, T> handle1, Func<TDerived2, T> handle2, Func<TDerived3, T> handle3)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
            where TDerived3 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    return handle1(case1);
                case TDerived2 case2:
                    return handle2(case2);
                case TDerived3 case3:
                    return handle3(case3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static T TypeMatch<TBase, TDerived1, TDerived2, TDerived3, TDerived4, T>(this TBase target, Func<TDerived1, T> handle1, Func<TDerived2, T> handle2, Func<TDerived3, T> handle3, Func<TDerived4, T> handle4)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
            where TDerived3 : class, TBase
            where TDerived4 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    return handle1(case1);
                case TDerived2 case2:
                    return handle2(case2);
                case TDerived3 case3:
                    return handle3(case3);
                case TDerived4 case4:
                    return handle4(case4);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static void TypeMatch<TBase, TDerived1, TDerived2>(this TBase target, Action<TDerived1> handle1, Action<TDerived2> handle2)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    handle1(case1);
                    return;
                case TDerived2 case2:
                    handle2(case2);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static void TypeMatch<TBase, TDerived1, TDerived2, TDerived3>(this TBase target, Action<TDerived1> handle1, Action<TDerived2> handle2, Action<TDerived3> handle3)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
            where TDerived3 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    handle1(case1);
                    return;
                case TDerived2 case2:
                    handle2(case2);
                    return;
                case TDerived3 case3:
                    handle3(case3);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static void TypeMatch<TBase, TDerived1, TDerived2, TDerived3, TDerived4>(this TBase target, Action<TDerived1> handle1, Action<TDerived2> handle2, Action<TDerived3> handle3, Action<TDerived4> handle4)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
            where TDerived3 : class, TBase
            where TDerived4 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    handle1(case1);
                    return;
                case TDerived2 case2:
                    handle2(case2);
                    return;
                case TDerived3 case3:
                    handle3(case3);
                    return;
                case TDerived4 case4:
                    handle4(case4);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }

        public static void TypeMatch<TBase, TDerived1, TDerived2, TDerived3, TDerived4, TDerived5>(this TBase target, Action<TDerived1> handle1, Action<TDerived2> handle2, Action<TDerived3> handle3, Action<TDerived4> handle4, Action<TDerived5> handle5)
            where TDerived1 : class, TBase
            where TDerived2 : class, TBase
            where TDerived3 : class, TBase
            where TDerived4 : class, TBase
            where TDerived5 : class, TBase
        {
            switch (target)
            {
                case TDerived1 case1:
                    handle1(case1);
                    return;
                case TDerived2 case2:
                    handle2(case2);
                    return;
                case TDerived3 case3:
                    handle3(case3);
                    return;
                case TDerived4 case4:
                    handle4(case4);
                    return;
                case TDerived5 case5:
                    handle5(case5);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), $"Target has unexpected type {target.GetType().Name}");
            }
        }
    }
}