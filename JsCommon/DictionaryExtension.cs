using System;
using System.Collections.Generic;
using System.Text;

namespace JsCommon
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// 指定したキーの値を返す。キーが含まれない場合はsupplierから対応する値を
        /// 取得し追加する。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static TValue Find<TKey, TValue>(
            this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> supplier)
            => dic.ContainsKey(key) ? dic[key] : (dic[key] = supplier(key));
    }
}
