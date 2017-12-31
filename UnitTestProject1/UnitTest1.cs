using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsCommon
{
    [TestClass]
    public class OptionTest
    {
        private static readonly long N = 1_000_000_00;

        [TestMethod]
        public void 多態性分岐()
        {
            var a = B<string>.Of("a");
            var b = B<string>.Of(null);
            for (int i = 0; i < N; ++i)
            {
                a.Hoge();
                b.Hoge();
            }

        }
        [TestMethod]
        public void if分岐()
        {
            var a = new A<string>("a");
            var b = new A<string>(null);
            for(int i=0; i<N; ++i)
            {
                a.Hoge();
                b.Hoge();
            }
        }
        [TestMethod]
        public void 多態性分岐2()
        {
            var a = B<string>.Of("a");
            var b = B<string>.Of(null);
            for (int i = 0; i < N; ++i)
            {
                a.Hoge();
                b.Hoge();
            }

        }
        [TestMethod]
        public void if分岐2()
        {
            var a = new A<string>("a");
            var b = new A<string>(null);
            for (int i = 0; i < N; ++i)
            {
                a.Hoge();
                b.Hoge();
            }
        }

        class A<T>
        {
            readonly T v;
            internal A(T v)
            {
                this.v = v;
            }
            internal string Hoge() => (v == null) ? "Nullやで" : v.ToString();
        }


        abstract class B<T>
        {
            internal static B<T> Of(T v)
                => (v == null) ? new B<T>.None() : (B<T>)new B<T>.Some(v);

            internal abstract string Hoge();

            internal class Some : B<T>
            {
                readonly T v;
                internal Some(T v)
                {
                    this.v = v;
                }
                internal override string Hoge() => v.ToString();
            }
            internal class None : B<T>
            {
                internal override string Hoge() => "Nullやで";
            }
        }
    }


}
