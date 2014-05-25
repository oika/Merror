using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Libs.Merror
{
    /// <summary>
    /// メンバ検索のパラメータ情報を表すクラスです。
    /// </summary>
    public class ReflectorParam
    {
        /// <summary>
        /// パラメータの型を取得します。
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// パラメータの値を取得します。
        /// </summary>
        public object Value { get; internal set; }

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="type">パラメータの型を指定します。</param>
        /// <param name="value">パラメータの値を指定します。</param>
        /// <param name="isRef">パラメータにrefまたはout修飾子がつく場合は
        /// Trueを指定します。
        /// typeパラメータに指定した型がByRef型になっている場合は、このパラメータは無視されます。</param>
        public ReflectorParam(Type type, object value, bool isRef = false)
        {
            if (isRef && !type.IsByRef)
            {
                this.Type = type.MakeByRefType();
            }
            else
            {
                this.Type = type;
            }

            this.Value = value;
        }

        /// <summary>
        /// 新規インスタンスを生成します。
        /// </summary>
        /// <typeparam name="T">パラメータの型を指定します。</typeparam>
        /// <param name="value">パラメータの値を指定します。</param>
        /// <param name="isRef">パラメータにrefまたはout修飾子がつく場合は
        /// Trueを指定します。</param>
        /// <returns>生成されたインスタンスを返します。</returns>
        public static ReflectorParam New<T>(T value, bool isRef = false)
        {
            return new ReflectorParam(typeof(T), value, isRef);
        }

        /// <summary>
        /// 内部的に使用されるコンストラクタです。
        /// </summary>
        /// <param name="value"></param>
        internal ReflectorParam(object value)
        {
            this.Type = value == null ? typeof(object) : value.GetType();
            this.Value = value;
        }

        #endregion
    }
}
