using System;
using System.Collections.Generic;
using System.Text;

namespace JsCommon
{
    public static class Options
    {
        public static Option<T> Filter<T>(this T v, Predicate<T> test)
            => test(v) ? Option<T>.Of(v) : Option<T>.None();

        public static Option<T> AsSome<T>(this T v) => Option<T>.AsSome(v);
    }
    public abstract class Option<T>
    {
        public static Option<T> None() => new Option<T>._None();
        public static Option<T> AsSome(T v) => new Option<T>.Some(v);
        public static Option<T> Of(T v) => (v == null) ? None() : AsSome(v);


        public abstract Option<U> Map<U>(Func<Some, U> whenSome, Func<U> whenNone = null);
        public abstract Option<U> FlatMap<U>(Func<Some, Option<U>> whenSome, Func<Option<U>> whenNone = null);
        public abstract Option<T> Do(Action<T> action);
        public abstract Option<T> DoWhenNone(Action action);
        public abstract T OrElse(Func<T> supplier);
        public abstract T OrElse(Func<Some> supplier);

        /// <summary>
        /// Holds not-null value.
        /// </summary>
        public class Some : Option<T>
        {
            public readonly T v;

            internal Some(T v)
            {
                if(v == null)
                {
                    throw new ArgumentNullException();
                }
                this.v = v;
            }

            public override Option<U> Map<U>(Func<Some, U> whenSome, Func<U> whenNone) => Option<U>.AsSome(whenSome(this));
            public override Option<U> FlatMap<U>(Func<Some, Option<U>> whenSome, Func<Option<U>> whenNone) => whenSome(this);
            public override Option<T> Do(Action<T> action)
            {
                action(v);
                return this;
            }
            public override Option<T> DoWhenNone(Action action) => this;
            public override T OrElse(Func<T> supplier) => v;
            public override T OrElse(Func<Some> supplier) => v;
        }
        class _None : Option<T>
        {
            public override Option<U> Map<U>(Func<Some, U> whenSome, Func<U> whenNone)
                => (whenNone == null) ? Option<U>.None() : Option<U>.AsSome(whenNone());
            public override Option<U> FlatMap<U>(Func<Some, Option<U>> whenSome, Func<Option<U>> whenNone)
                => (whenNone == null) ? Option<U>.None() : whenNone();
            public override Option<T> Do(Action<T> action) => this;
            public override Option<T> DoWhenNone(Action action)
            {
                action();
                return this;
            }
            public override T OrElse(Func<T> supplier) => supplier();
            public override T OrElse(Func<Some> supplier) => supplier().v;
        }
    }
    public static class OptionExtension {
        /// <summary>
        /// Returns itself when it is Some. Returns Some when it is None.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="op"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Option<T> BindWhenNone<T>(this Option<T> op, T other)
            => op.BindWhenNone(() => other);
        public static Option<T> BindWhenNone<T>(this Option<T> op, Option<T> other)
            => op.BindWhenNone(() => other);
        public static Option<T> BindWhenNone<T>(this Option<T> op, Func<T> supplier)
            => op.BindWhenNone(() => Option<T>.AsSome(supplier()));
        public static Option<T> BindWhenNone<T>(this Option<T> op, Func<Option<T>> supplier)
            => op.FlatMap(v => v, supplier);

        /// <summary>
        ///  Returns None when it is Some. Returns Some when it is None.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="op"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static Option<U> BindOnlyWhenNone<T, U>(this Option<T> op, U other)
            => op.BindOnlyWhenNone(() => other);
        public static Option<U> BindOnlyWhenNone<T, U>(this Option<T> op, Option<U> other)
            => op.BindOnlyWhenNone(() => other);
        public static Option<U> BindOnlyWhenNone<T, U>(this Option<T> op, Func<U> supplier)
            => op.BindOnlyWhenNone(() => Option<U>.AsSome(supplier()));
        public static Option<U> BindOnlyWhenNone<T, U>(this Option<T> op, Func<Option<U>> supplier)
            => op.FlatMap(_ => Option<U>.None(), supplier);

        public static Option<Tuple<T, U>> Concat<T, U>(this Option<T> op, U v)
            => op.Concat(_ => v);
        public static Option<Tuple<T, U>> Concat<T, U>(this Option<T> op, Func<T, U> converter)
            => op.FlatMap(v1 => Option<U>.AsSome(converter(v1.v)).Map(v2 => new Tuple<T, U>(v1.v, v2.v)));

        public static Option<Tuple<T, U>> Both<T, U>(this Option<T> op, Option<U> other)
            => op.Both(v => other);
        public static Option<Tuple<T, U>> Both<T, U>(this Option<T> op, Func<T, Option<U>> converter)
            => op.FlatMap(v1 => converter(v1.v).Map(v2 => new Tuple<T, U>(v1.v, v2.v)));

        public static Option<T> Filter<T>(this Option<T> op, Predicate<T> test)
            => op.FlatMap(v => test(v.v) ? v : Option<T>.None());

        public static T OrElse<T>(this Option<T> op, T defaultValue)
            => op.OrElse(() => defaultValue);
        public static T OrElse<T>(this Option<T> op, Option<T>.Some defaultValue)
            => op.OrElse(() => defaultValue);
        public static T OrDefault<T>(this Option<T> op) => op.OrElse(default(T));

        public static Option<T> ThrowWhenSome<T, X>(this Option<T> op, Func<X> exceptionSupplier) where X : Exception
            => op.Do(_ =>
            {
                throw exceptionSupplier();
            });
        public static Option<T> ThrowWhenNone<T, X>(this Option<T> op, Func<X> exceptionSupplier) where X : Exception
            => op.DoWhenNone(() =>
            {
                throw exceptionSupplier();
            });
    }
}
