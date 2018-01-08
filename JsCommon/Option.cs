using System;
using System.Collections.Generic;
using System.Text;

namespace JsCommon
{
    public static class Option
    {
        /// <summary>
        /// 指定した型のNoneを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Option<T> None<T>() => new _None<T>();
        /// <summary>
        /// 指定した値が非nullであればSomeにラップし、nullであればNoneを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Option<T> Of<T>(T v) => (v == null) ? None<T>() : new Some<T>(v);

        /// <summary>
        /// 指定した値をSome化する。
        /// nullを指定すると<see cref="ArgumentNullException"/>が発生。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Some<T> AsSome<T>(this T v)
        {
            if (v == null)
            {
                throw new ArgumentNullException();
            }
            return new Some<T>(v);
        }
        /// <summary>
        /// 指定した値が非nullかつ条件をみたす場合、Some化する。
        /// そうでなければNoneを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static Option<T> AsSomeIf<T>(this T v, Predicate<T> test) => Option.Of(v).Filter(test);

        /// <summary>
        /// Someを返す関数に変換する。
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Func<Some<R>> AsSomeSupplier<R>(this Func<R> f)
            => () => Option.AsSome(f());
        /// <summary>
        /// Someを返す関数に変換する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Func<T, Some<R>> AsSomeSupplier<T, R>(this Func<T, R> f)
            => v => Option.AsSome(f(v));
    }

    /// <summary>
    /// 値を持つ場合(Some)と持たない場合(None)を透過的に扱うためのクラス。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Option<T>
    {
        internal Option() { }

        /// <summary>
        /// 保持する値を変換する。
        /// SomeならwhenSomeを実行して保持する値を変換する。
        /// NoneかつwhenNoneを指定した場合は、whenNoneの戻り値を保持するSomeを返す。
        /// whenSomeおよびwhenNoneの戻り値は非nullでなければならない。Noneを返したい
        /// 場合は<see cref="Option{T}.FlatMap{U}(Func{T, Option{U}}, Func{Option{U}})"/>を使うこと。
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="whenSome"></param>
        /// <param name="whenNone"></param>
        /// <returns></returns>
        public abstract Option<U> Map<U>(Func<T, U> whenSome, Func<U> whenNone = null);
        /// <summary>
        /// 他のOptionに置き換える。
        /// SomeならwhenSomeを実行して、結果のOptionを返す。
        /// NoneかつwhenNoneを指定した場合は、whenNoneの戻り値を返す。
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="whenSome"></param>
        /// <param name="whenNone"></param>
        /// <returns></returns>
        public abstract Option<U> FlatMap<U>(Func<T, Option<U>> whenSome, Func<Option<U>> whenNone = null);
        /// <summary>
        /// 処理の実行。
        /// Someの場合のみ、保持する値を引数として指定した処理を実行し、自身を返す。
        /// </summary>
        /// <param name="actionWhenSome"></param>
        /// <returns></returns>
        public abstract Option<T> Do(Action<T> actionWhenSome);
        /// <summary>
        /// 処理の実行。
        /// Noneの場合のみ指定した処理を実行し、自身を返す。
        /// </summary>
        /// <param name="actionWhenNone"></param>
        /// <returns></returns>
        public abstract Option<T> DoWhenNone(Action actionWhenNone);

        /// <summary>
        /// Some化。
        /// Someなら自身を返し、Noneなら指定した処理の戻り値を返す。
        /// </summary>
        /// <param name="supplierWhenNone"></param>
        /// <returns></returns>
        public abstract Some<T> OrElse(Func<Some<T>> supplierWhenNone);

        /// <summary>
        /// Someなら保持する値がUにキャスト可能な場合のみ、U型のSomeに置き換える。
        /// Noneなら無条件にU型のNoneに置き換える。
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public Option<U> Cast<U>() => this.Filter(v => v is U).Map(v => (U)(object)v);


        /// <summary>
        /// 内部処理用。whenSomeの引数は自身。
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="whenSome"></param>
        /// <param name="whenNone"></param>
        /// <returns></returns>
        internal abstract Option<U> FlatMapMe<U>(Func<Some<T>, Option<U>> whenSome, Func<Option<U>> whenNone = null);
    }
    /// <summary>
    /// Holds not-null value.
    /// </summary>
    public class Some<T> : Option<T>
    {
        public readonly T v;

        internal Some(T v)
        {
            this.v = v;
        }

        public override Option<U> Map<U>(Func<T, U> whenSome, Func<U> whenNone = null)
            => FlatMap(whenSome.AsSomeSupplier());
        public override Option<U> FlatMap<U>(Func<T, Option<U>> whenSome, Func<Option<U>> whenNone = null)
            => whenSome(v);

        public override Option<T> Do(Action<T> actionWhenSome) => Do2(actionWhenSome);
        public override Option<T> DoWhenNone(Action actionWhenNone) => this;
        public override Some<T> OrElse(Func<Some<T>> supplierWhenNone) => this;

        internal override Option<U> FlatMapMe<U>(Func<Some<T>, Option<U>> whenSome, Func<Option<U>> whenNone = null)
            => whenSome(this);

        /// <summary>
        /// 無条件にNoneに変換する。
        /// </summary>
        /// <returns></returns>
        public Option<T> Delete() => Option.None<T>();
        /// <summary>
        /// 保持する値でactionを呼び出し、自身を返す。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Some<T> Do2(Action<T> action)
        {
            action(v);
            return this;
        }
        /// <summary>
        /// actionを呼び出し、自身を返す。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Some<T> Do2(Action action) => Do2(_ => action());
    }
    class _None<T> : Option<T>
    {
        public override Option<U> Map<U>(Func<T, U> whenSome, Func<U> whenNone)
            => FlatMap(null, whenNone?.AsSomeSupplier());
        public override Option<U> FlatMap<U>(Func<T, Option<U>> whenSome, Func<Option<U>> whenNone)
            => (whenNone == null) ? Option.None<U>() : whenNone();

        public override Option<T> Do(Action<T> actionWhenSome) => this;
        public override Option<T> DoWhenNone(Action actionWhenNone)
        {
            actionWhenNone();
            return this;
        }
        public override Some<T> OrElse(Func<Some<T>> supplierWhenNone) => supplierWhenNone();

        internal override Option<U> FlatMapMe<U>(Func<Some<T>, Option<U>> whenSome, Func<Option<U>> whenNone)
            => FlatMap(null, whenNone);
    }

    /// <summary>
    /// <see cref="Option{T}"/>の拡張メソッド。
    /// </summary>
    public static class OptionExtension {
        /// <summary>
        /// 状態を入れ替える。
        /// Noneの場合、指定したSomeを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="otherWhenNone"></param>
        /// <returns></returns>
        public static Option<U> Invert<T, U>(this Option<T> op, Some<U> otherWhenNone)
            => op.Invert(() => otherWhenNone);
        /// <summary>
        /// 状態を入れ替える。
        /// Noneの場合、指定した処理の戻り値を返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="supplierWhenNone"></param>
        /// <returns></returns>
        public static Option<U> Invert<T, U>(this Option<T> op, Func<Some<U>> supplierWhenNone)
            => op.FlatMap(_ => Option.None<U>(), supplierWhenNone);


        /// <summary>
        /// Someなら指定した値とのペアを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Option<Tuple<T, U>> Concat<T, U>(this Option<T> op, Some<U> v)
            => op.Map(v1 => new Tuple<T, U>(v1, v.v));
        /// <summary>
        /// Someなら保持する値を引数として指定した処理を実行し、その戻り値とのペアを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static Option<Tuple<T, U>> Concat<T, U>(this Option<T> op, Func<T, Some<U>> converter)
            => op.FlatMapMe(v1 => converter(v1.v).Map(v2 => new Tuple<T, U>(v1.v, v2)));


        /// <summary>
        /// 現在の値と指定した値の両方がSomeの場合、それらのペアを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Option<Tuple<T, U>> Both<T, U>(this Option<T> op, Option<U> other)
            => op.FlatMap(v1 => other.Map(v2 => new Tuple<T, U>(v1, v2)));
        /// <summary>
        /// 現在の値がSomeで、かつ保持する値を引数として指定した処理を実行した結果が
        /// Someである場合、それらのペアを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static Option<Tuple<T, U>> Both<T, U>(this Option<T> op, Func<T, Option<U>> converter)
            => op.Both(op.FlatMap(converter));

        /// <summary>
        /// Someかつ条件を満たす場合のみ状態を維持する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static Option<T> Filter<T>(this Option<T> op, Predicate<T> test)
            => op.FlatMapMe(some => test(some.v) ? some : some.Delete());

        /// <summary>
        /// Someの場合のみ指定した処理を実行し、自身を返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Option<T> Do<T>(this Option<T> op, Action action) => op.Do(_ => action());


        /// <summary>
        /// Some化。
        /// Noneなら、指定した値を保持するSomeに置き換える。値は非nullでなければならない。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Some<T> OrElse<T>(this Option<T> op, T defaultValue)
            => op.OrElse(Option.AsSome(defaultValue));
        /// <summary>
        /// Some化。
        /// Noneなら指定したSomeに置き換える。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Some<T> OrElse<T>(this Option<T> op, Some<T> defaultValue)
            => op.OrElse(() => defaultValue);
        /// <summary>
        /// デフォルト値によってSome化する。デフォルト値は非nullでなければならない。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <returns></returns>
        public static Some<T> OrDefault<T>(this Option<T> op) => op.OrElse(default(T));


        /// <summary>
        /// Someなら、保持する値を引数として指定した処理を実行し、
        /// 結果の例外を投げる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <param name="op"></param>
        /// <param name="exceptionSupplier"></param>
        /// <returns></returns>
        public static Option<T> Throw<T, X>(this Option<T> op, Func<T, X> exceptionSupplier) where X : Exception
            => op.Do(v =>
            {
                throw exceptionSupplier(v);
            });
        /// <summary>
        /// Noneなら指定した処理の戻り値を投げる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <param name="op"></param>
        /// <param name="exceptionSupplier"></param>
        /// <returns></returns>
        public static Option<T> ThrowWhenNone<T, X>(this Option<T> op, Func<X> exceptionSupplier) where X : Exception
            => op.DoWhenNone(() =>
            {
                throw exceptionSupplier();
            });

    }
}
