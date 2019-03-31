using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oika.Libs.Merror
{
    /// <summary>
    /// リフレクションを使用した、クラスメンバへのアクセス機能を提供するクラスです。
    /// </summary>
    public class Reflector
    {
        /// <summary>
        /// staticメンバ検索用フラグ
        /// </summary>
        static readonly BindingFlags StaticFlags
                    = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        /// <summary>
        /// インスタンスメンバ検索用フラグ
        /// </summary>
        static readonly BindingFlags InstanceFlags
                    = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// 対象の型
        /// </summary>
        readonly Type trgType;

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="targetType">アクセス対象の型を指定します。
        /// アクセス対象の型が非公開の場合は、
        /// 型名とアセンブリ情報をパラメータにとるオーバーロードを使用します。
        /// </param>
        public Reflector(Type targetType)
        {
            this.trgType = targetType;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="typeFullName">アクセス対象の型の名称を名前空間を含めて指定します。</param>
        /// <param name="anotherTypeInAssembly">アクセス対象の型を含むアセンブリ内の
        /// 任意の公開型を指定します。
        /// この値はアセンブリ特定のために使用されます。
        /// </param>
        /// <exception cref="System.TypeLoadException">指定された型が見つからない場合にスローされます。</exception>
        public Reflector(string typeFullName, Type anotherTypeInAssembly)
            : this(typeFullName, Assembly.GetAssembly(anotherTypeInAssembly))
        {
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="typeFullName">アクセス対象の型の名称を名前空間を含めて指定します。</param>
        /// <param name="targetAssembly">アクセス対象の型を含むアセンブリを指定します。</param>
        /// <exception cref="System.TypeLoadException">指定された型が見つからない場合にスローされます。</exception>
        public Reflector(string typeFullName, Assembly targetAssembly)
            : this(targetAssembly.GetType(typeFullName, true, false))
        {
        }

        #endregion

        #region インスタンス生成

        /// <summary>
        /// 指定されたパラメータに一致するコンストラクタを使って
        /// 対象の型のインスタンスを生成します。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object NewInstance(params object[] args)
        {
            return NewInstanceExact(args.Select(a => new ReflectorParam(a)).ToArray());
        }

        /// <summary>
        /// 厳密に指定されたパラメータ情報に一致するコンストラクタを使って
        /// 対象の型のインスタンスを生成します。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object NewInstanceExact(params ReflectorParam[] args)
        {
            var cnst = trgType.GetConstructor(InstanceFlags, null, args.Select(a => a.Type).ToArray(), null);
            if (cnst == null) throw new MemberAccessException();

            var argVals = args.Select(a => a.Value).ToArray();

            var rtn = cnst.Invoke(argVals);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Type.IsByRef) args[i].Value = argVals[i];
            }
            return rtn;
        }

        #endregion

        #region 静的メンバアクセス

        /// <summary>
        /// 静的フィールドに値を設定します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetStaticField(string name, object value)
        {
            _GetFieldInfo(name, true).SetValue(null, value);
        }
        /// <summary>
        /// 静的フィールドの値を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetStaticField(string name)
        {
            return _GetFieldInfo(name, true).GetValue(null);
        }
        /// <summary>
        /// 静的プロパティに値を設定します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetStaticProperty(string name, object value)
        {
            _GetPropInfo(name, true).SetValue(null, value, null);
        }
        /// <summary>
        /// 静的プロパティの値を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetStaticProperty(string name)
        {
            return _GetPropInfo(name, true).GetValue(null, null);
        }
        /// <summary>
        /// 静的メソッドを実行し、戻り値を返します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object InvokeStatic(string name, params object[] args)
        {
            return InvokeStaticExact(name, args.Select(a => new ReflectorParam(a)).ToArray());
        }
        /// <summary>
        /// 厳密に指定されたパラメータ情報に一致する静的メソッドを実行し、戻り値を返します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object InvokeStaticExact(string name, params ReflectorParam[] args)
        {
            var info = _GetMethodInfo(name, args.Select(a => a.Type).ToArray(), true);

            var argVals = args.Select(a => a.Value).ToArray();
            var rtn = info.Invoke(null, argVals);

            //参照渡しの値を反映
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Type.IsByRef) args[i].Value = argVals[i];
            }
            return rtn;
        }

        #endregion

        #region インスタンスメンバアクセス

        /// <summary>
        /// インスタンスフィールドに値を設定します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetField(object instance, string name, object value)
        {
            _GetFieldInfo(name, false).SetValue(instance, value);
        }
        /// <summary>
        /// インスタンスフィールドの値を取得します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetField(object instance, string name)
        {
            return _GetFieldInfo(name, false).GetValue(instance);
        }
        /// <summary>
        /// インスタンスプロパティに値を設定します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetProperty(object instance, string name, object value)
        {
            _GetPropInfo(name, false).SetValue(instance, value, null);
        }
        /// <summary>
        /// インスタンスプロパティの値を取得します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetProperty(object instance, string name)
        {
            return _GetPropInfo(name, false).GetValue(instance, null);
        }
        /// <summary>
        /// インスタンスメソッドを実行し、戻り値を返します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object Invoke(object instance, string name, params object[] args)
        {
            return InvokeExact(instance, name, args.Select(a => new ReflectorParam(a)).ToArray());
        }
        /// <summary>
        /// 厳密に指定されたパラメータ情報に一致するインスタンスメソッドを実行し、戻り値を返します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object InvokeExact(object instance, string name, params ReflectorParam[] args)
        {
            var info = _GetMethodInfo(name, args.Select(a => a.Type).ToArray(), false);

            var argVals = args.Select(a => a.Value).ToArray();
            var rtn = info.Invoke(instance, argVals);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Type.IsByRef) args[i].Value = argVals[i];
            }
            return rtn;
        }

        /// <summary>
        /// インデクサにアクセスし、指定したインデクスに値を設定します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <param name="indexes"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetIndexer(object instance, object value, params object[] indexes)
        {
            SetIndexerExact(instance, value, indexes.Select(i => new ReflectorParam(i)).ToArray());
        }
        /// <summary>
        /// 厳密に指定されたパラメータ情報に一致するインデクサにアクセスし、
        /// 指定したインデクスに値を設定します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <param name="indexes"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetIndexerExact(object instance, object value, params ReflectorParam[] indexes)
        {
            var info = _GetIndexerInfo(indexes.Select(i => i.Type).ToArray());
            info.SetValue(instance, value, indexes.Select(i => i.Value).ToArray());
        }
        /// <summary>
        /// インデクサにアクセスし、指定したインデクスの値を取得します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetIndexer(object instance, params object[] indexes)
        {
            return GetIndexerExact(instance, indexes.Select(i => new ReflectorParam(i)).ToArray());
        }
        /// <summary>
        /// 厳密に指定されたパラメータ情報に一致するインデクサにアクセスし、
        /// 指定したインデクスの値を取得します。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetIndexerExact(object instance, params ReflectorParam[] indexes)
        {
            var info = _GetIndexerInfo(indexes.Select(i => i.Type).ToArray());
            return info.GetValue(instance, indexes.Select(i => i.Value).ToArray());
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// インデクサの情報を検索
        /// </summary>
        /// <param name="indexTypes"></param>
        /// <returns></returns>
        private PropertyInfo _GetIndexerInfo(Type[] indexTypes)
        {
            var flg = InstanceFlags;
            var prop = trgType.GetProperty("Item", InstanceFlags, null, null, indexTypes, null);
            if (prop == null) throw new MemberAccessException();

            return prop;
        }

        /// <summary>
        /// プロパティの情報を検索
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private PropertyInfo _GetPropInfo(string name, bool isStatic)
        {
            var flg = isStatic ? StaticFlags : InstanceFlags;
            var prop = trgType.GetProperty(name, flg);
            if (prop == null) throw new MemberAccessException();

            return prop;
        }

        /// <summary>
        /// フィールドの情報を検索
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private FieldInfo _GetFieldInfo(string name, bool isStatic)
        {
            var flg = isStatic ? StaticFlags : InstanceFlags;
            var fld = trgType.GetField(name, flg);
            if (fld == null) throw new MemberAccessException();

            return fld;
        }

        /// <summary>
        /// メソッドの情報を検索
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paramTypes"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private MethodInfo _GetMethodInfo(string name, Type[] paramTypes, bool isStatic)
        {
            var flg = isStatic ? StaticFlags : InstanceFlags;
            var mtd = trgType.GetMethod(name, flg, null, paramTypes, null);
            if (mtd == null) throw new MemberAccessException();

            return mtd;
        }

        #endregion
    }
}
