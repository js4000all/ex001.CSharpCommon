using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsCommon
{
    using JsCommon;

    static class OptionTestExtension
    {
        public static Some<T> AssertSome<T>(this Option<T> op)
            => op.IsInstanceOf<Some<T>>();
        public static Some<T> AssertSome<T>(this Option<T> op, T expected)
            => op.IsInstanceOf<Some<T>>().Do2(v => v.Is(expected));

        public static Option<T> AssertNone<T>(this Option<T> op)
        {
            op.IsNotInstanceOf<Some<T>>();
            return op;
        }
    }

    [TestClass]
    public class OptionTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AsSome_null()
            => Option.AsSome<string>(null);
        [TestMethod]
        public void AsSome_非null()
            => "ABC".AsSome().AssertSome("ABC");

        [TestMethod]
        public void None()
            => Option.None<string>().AssertNone();

        [TestMethod]
        public void Of_非null()
            => Option.Of("ABC").AssertSome("ABC");
        [TestMethod]
        public void Of_null()
            => Option.Of<string>(null).AssertNone();

        [TestMethod]
        public void Delete()
            => "ABC".AsSome().Delete().AssertNone();

        [TestMethod]
        public void Filter_Some_条件満たす()
            => "ABC".AsSome().Filter(v => v.StartsWith("A")).AssertSome("ABC");
        [TestMethod]
        public void Filter_Some_条件満たさない()
            => "ABC".AsSome().Filter(v => v.StartsWith("B")).AssertNone();
        [TestMethod]
        public void Filter_None()
            => Option.None<string>().Filter(v => v.StartsWith("A")).AssertNone();

        [TestMethod]
        public void AsSomeIf_条件満たす()
            => "ABC".AsSomeIf(v => v.StartsWith("A")).AssertSome("ABC");
        [TestMethod]
        public void AsSomeIf_条件満たさない()
            => "ABC".AsSomeIf(v => v.StartsWith("B")).AssertNone();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Map_Some_戻り値がnull()
            => "ABC".AsSome().Map<object>(v => null, NotCalled);
        [TestMethod]
        public void Map_Some_戻り値が非null()
            => "ABC".AsSome().Map(v => v.Length, NotCalledI).AssertSome(3);
        [TestMethod]
        public void Map_None()
            => Option.None<string>().Map(NotCalledI, () => 5).AssertSome(5);
        [TestMethod]
        public void Map_None_処理なし()
            => Option.None<string>().Map(NotCalledI).AssertNone();

        [TestMethod]
        public void FlatMap_Some()
            => "ABCDE".AsSome().FlatMap(v => v.Length.AsSome(), NotCalledI2).AssertSome(5);
        [TestMethod]
        public void FlatMap_None()
            => Option.None<string>().FlatMap(NotCalledI2, () => 10.AsSome()).AssertSome(10);
        [TestMethod]
        public void FlatMap_None_処理なし()
            => Option.None<string>().FlatMap(NotCalledI2).AssertNone();

        [TestMethod]
        public void Do_Some()
        {
            var some = "ABC".AsSome();
            string s = null;
            some
                .Do(v => s = v)
                .DoWhenNone(NotCalled3)
                .IsSameReferenceAs(some);
            s.Is("ABC");
        }
        [TestMethod]
        public void Do_None()
        {
            var none = Option.None<string>();
            string s = null;
            none
                .Do(NotCalled3)
                .DoWhenNone(() => s = "OK")
                .IsSameReferenceAs(none);
            s.Is("OK");
        }

        [TestMethod]
        public void OrElse_Some()
            => "ABC".AsSome().OrElse("EFG").AssertSome("ABC");
        [TestMethod]
        public void OrElse_None()
            => Option.None<string>().OrElse("EFG").AssertSome("EFG");

        [TestMethod]
        public void Concat_Some()
        {
            var some = "ABC".AsSome();
            var pair = some.Concat(v => (v + "*").AsSome()).AssertSome();
            pair.v.Item1.Is("ABC");
            pair.v.Item2.Is("ABC*");
        }
        [TestMethod]
        public void Concat_None()
            => Option.None<string>().Concat(NotCalled2).AssertNone();

        [TestMethod]
        public void Both_Option_SomeとSome()
        {
            var some = "ABC".AsSome();
            var pair = some.Both("DEF".AsSome()).AssertSome();
            pair.v.Item1.Is("ABC");
            pair.v.Item2.Is("DEF");
        }
        [TestMethod]
        public void Both_Option_SomeとNone()
            => "ABC".AsSome().Both(Option.None<string>()).AssertNone();
        [TestMethod]
        public void Both_Option_None()
            => Option.None<string>().Both("DEF".AsSome()).AssertNone();

        [TestMethod]
        public void Both_Func_SomeとSome()
        {
            var some = "ABC".AsSome();
            var pair = some.Both(v => (v + "*").AsSome()).AssertSome();
            pair.v.Item1.Is("ABC");
            pair.v.Item2.Is("ABC*");
        }
        [TestMethod]
        public void Both_Func_SomeとNone()
            => "ABC".AsSome().Both(_ => Option.None<string>()).AssertNone();
        [TestMethod]
        public void Both_Func_None()
            => Option.None<string>().Both(NotCalled2).AssertNone();

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "ABC")]
        public void Throw_Some()
            => "ABC".AsSome().Throw(v => new IndexOutOfRangeException(v));
        [TestMethod]
        public void Throw_None()
            => Option.None<string>().Throw(v => new IndexOutOfRangeException(v)).AssertNone();

        [TestMethod]
        public void ThrowWhenNone_Some()
            => "ABC".AsSome().ThrowWhenNone(() => new IndexOutOfRangeException()).AssertSome();
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ThrowWhenNone_None()
            => Option.None<string>().ThrowWhenNone(() => new IndexOutOfRangeException());


        [TestMethod]
        public void Cast_アップキャスト()
        {
            Some<B> someOfB = new B().AsSome();
            Option<A> op = someOfB.Cast<A>();
            op.AssertSome();
        }
        [TestMethod]
        public void Cast_ダウンキャスト()
        {
            Some<A> someOfB = new B().AsSome<A>();
            Option<B> op = someOfB.Cast<B>();
            op.AssertSome();
        }
        [TestMethod]
        public void Cast_キャストできない型()
        {
            Some<A> someOfB = new B().AsSome<A>();
            Option<C> op = someOfB.Cast<C>();
            op.AssertNone();
        }
        [TestMethod]
        public void Cast_None()
        {
            Option<A> noneOfA = Option.None<A>();
            Option<C> op = noneOfA.Cast<C>();
            op.AssertNone();
        }

        class A { }
        class B : A { }
        class C { }

        static string NotCalled(string s)
            => s.AsSome().Do2(_ => Assert.Fail()).v;
        static int NotCalledI(string s)
            => 0.AsSome().Do2(_ => Assert.Fail()).v;

        static string NotCalled() => NotCalled("dummy");
        static int NotCalledI() => NotCalledI("dummy");
        static Some<string> NotCalled2(string s) => NotCalled(s).AsSome();
        static Some<int> NotCalledI2(string s) => NotCalledI(s).AsSome();
        static Some<string> NotCalled2() => NotCalled2("dummy");
        static Some<int> NotCalledI2() => NotCalledI2("dummy");
        static void NotCalled3(string s) => NotCalled(s).AsSome();
        static void NotCalled3() => NotCalled3("dummy");
    }
}
