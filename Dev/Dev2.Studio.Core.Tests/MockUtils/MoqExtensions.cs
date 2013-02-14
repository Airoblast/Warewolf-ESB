﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Language.Flow;
using Moq.Language;
using System.Reflection;

namespace Dev2.Core.Tests.MockUtils
{
    public static class MoqExtensions
    {
        public delegate void RefAction<TRef, TParam1>(ref TRef refVal, TParam1 param1);


        // Sashen.Naidoo : 14-02-2012 : This method adds ref support to Moq
        /// <summary>
        /// This method can be used to invoke a method call containing a ref parameter in a mocked method
        /// </summary>
        /// <typeparam name="TRef"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TMock"></typeparam>
        /// <param name="mock"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IReturnsResult<TMock> RefCallback<TRef, TParam1, TMock>(this ICallback mock, RefAction<TRef, TParam1> action) where TMock : class
        {
            mock.GetType().Assembly
                .GetType("Moq.MethodCall")
                .InvokeMember("SetCallbackWithArguments", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null,
                              mock,
                              new object[] { action });
            return mock as IReturnsResult<TMock>;
        }

        public static ICallback IgnoreRefMatching(this ICallback mock)
        {
            try
            {


                FieldInfo matcherField = typeof(Mock).Assembly
                                                        .GetType("Moq.MethodCall")
                                                        .GetField("argumentMatchers",
                                                        BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                IList<object> argumentMatchers = (IList<object>)matcherField.GetValue(mock);
                Type refMatcherType = typeof(Mock).Assembly
                                                    .GetType("Moq.Matchers.RefMatcher");
                FieldInfo equalField = refMatcherType.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                foreach (object matcher in argumentMatchers)
                {
                    if (matcher.GetType() == refMatcherType)
                        equalField.SetValue(matcher, new Func<object, bool>(delegate(object o) { return true; }));
                }

                return mock;
            }
            catch (NullReferenceException nex)
            {
                return mock;
            }
        }


        public delegate void OutAction<TOut, TParam1>(out TOut outValue, TParam1 param1);

        public static IReturnsResult<TMock> OutCallback<TOut, TParam1, TMock>(this ICallback mock, OutAction<TOut, TParam1> action) where TMock : class
        {
            mock.GetType().Assembly
                .GetType("Moq.MethodCall")
                .InvokeMember("SetCallbackWithArguments",
                              BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null,
                              mock,
                              new object[] { action });
            return mock as IReturnsResult<TMock>;
        }

        public static ICallback IgnoreOutMatching<TMock, TResult>(this ICallback mock)
        {
            try
            {
                FieldInfo matcherField = typeof(Mock).Assembly
                                            .GetType("Moq.MethodCall")
                                            .GetField("argumentMatchers",
                                                       BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                IList<object> argumentMatchers = (IList<object>)matcherField.GetValue(mock);
                Type outMatcher = typeof(Mock).Assembly
                                                .GetType("Mock.Matchers.OutMatcher");
                FieldInfo equalField = outMatcher.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                foreach (object matcher in argumentMatchers)
                {
                    if (matcher.GetType() == outMatcher)
                        equalField.SetValue(matcher, new Func<object, bool>(delegate(object o) { return true; }));
                }
                return mock;
            }
            catch (NullReferenceException nex)
            {
                return mock;
            }
        }
    }
}
