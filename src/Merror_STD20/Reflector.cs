using System;
using System.Linq;
using System.Reflection;

namespace Oika.Libs.Merror
{
    /// <summary>
    /// Provides functions to access public/non-public members of public/non-public classes, using Reflection.
    /// </summary>
    public class Reflector
    {
        static readonly BindingFlags StaticFlags
                    = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        static readonly BindingFlags InstanceFlags
                    = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        readonly Type trgType;

        #region Constructor

        /// <summary>
        /// Creates new Reflector instance with the type info.
        /// </summary>
        /// <param name="targetType">Type of the target object.
        /// Use overload with the type name and the assembly info instead if the target class isn't public.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public Reflector(Type targetType)
        {
            this.trgType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        }

        /// <summary>
        /// Creates new Reflector instance with the type name and another type in the same assembly.
        /// </summary>
        /// <param name="typeFullName">Full name of the target class, includes namespace.</param>
        /// <param name="anotherTypeInAssembly">An public type in the target assembly.
        /// This is used to identify the assembly.
        /// </param>
        /// <exception cref="System.TypeLoadException">Thrown when the target type is not found.</exception>
        public Reflector(string typeFullName, Type anotherTypeInAssembly)
            : this(typeFullName, Assembly.GetAssembly(anotherTypeInAssembly))
        {
        }

        /// <summary>
        /// Creates new Reflector instance with the type name and the assembly info.
        /// </summary>
        /// <param name="typeFullName">Full name of the target class, includes namespace.</param>
        /// <param name="targetAssembly">The assembly which contains target class.</param>
        /// <exception cref="System.TypeLoadException">Thrown when the target type is not found.</exception>
        public Reflector(string typeFullName, Assembly targetAssembly)
            : this(targetAssembly.GetType(typeFullName, true, false))
        {
        }

        #endregion

        #region Create instance

        /// <summary>
        /// Creates an instance of the target class using the constructor which matches with the passed parameters.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException">Thrown when the constructor is not found.</exception>
        public object NewInstance(params object[] args)
        {
            return NewInstanceExact(args.Select(a => new ReflectorParam(a)).ToArray());
        }

        /// <summary>
        /// Creates an instance of the target class using the constructor which matches with the strictly specified parameters.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException">Thrown when the constructor is not found.</exception>
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

        #region Access to static members

        /// <summary>
        /// Sets value to a static field.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetStaticField(string name, object value)
        {
            _GetFieldInfo(name, true).SetValue(null, value);
        }
        /// <summary>
        /// Gets value from a static field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetStaticField(string name)
        {
            return _GetFieldInfo(name, true).GetValue(null);
        }
        /// <summary>
        /// Sets value to a static property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetStaticProperty(string name, object value)
        {
            _GetPropInfo(name, true).SetValue(null, value, null);
        }
        /// <summary>
        /// Gets value from a static property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetStaticProperty(string name)
        {
            return _GetPropInfo(name, true).GetValue(null, null);
        }
        /// <summary>
        /// Calls a static method and returns value.
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
        /// Calls a static method with strictly specified parameters and returns value.
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

        #region Access to instance members

        /// <summary>
        /// Sets value to a field.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetField(object instance, string name, object value)
        {
            _GetFieldInfo(name, instance == null).SetValue(instance, value);
        }
        /// <summary>
        /// Gets value from a field.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetField(object instance, string name)
        {
            return _GetFieldInfo(name, instance == null).GetValue(instance);
        }
        /// <summary>
        /// Sets value to a property.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="System.MemberAccessException"></exception>
        public void SetProperty(object instance, string name, object value)
        {
            _GetPropInfo(name, instance == null).SetValue(instance, value, null);
        }
        /// <summary>
        /// Gets value from a property.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object GetProperty(object instance, string name)
        {
            return _GetPropInfo(name, instance == null).GetValue(instance, null);
        }
        /// <summary>
        /// Calls a method and returns value.
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
        /// Calls a method with strictly specified parameters and returns value.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.MemberAccessException"></exception>
        public object InvokeExact(object instance, string name, params ReflectorParam[] args)
        {
            var info = _GetMethodInfo(name, args.Select(a => a.Type).ToArray(), instance == null);

            var argVals = args.Select(a => a.Value).ToArray();
            var rtn = info.Invoke(instance, argVals);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Type.IsByRef) args[i].Value = argVals[i];
            }
            return rtn;
        }

        /// <summary>
        /// Sets value to the index of an indexer.
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
        /// Sets value to the index of an indexer with strictly specified parameters.
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
        /// Gets value from the index of an indexer.
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
        /// Gets value from the index of an indexer with strictly specified parameters.
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

        #region Private methods

        private PropertyInfo _GetIndexerInfo(Type[] indexTypes)
        {
            var flg = InstanceFlags;
            var prop = trgType.GetProperty("Item", InstanceFlags, null, null, indexTypes, null);
            if (prop == null) throw new MemberAccessException();

            return prop;
        }

        private PropertyInfo _GetPropInfo(string name, bool isStatic)
        {
            var flg = isStatic ? StaticFlags : InstanceFlags;
            var prop = trgType.GetProperty(name, flg);
            if (prop == null) throw new MemberAccessException();

            return prop;
        }

        private FieldInfo _GetFieldInfo(string name, bool isStatic)
        {
            var flg = isStatic ? StaticFlags : InstanceFlags;
            var fld = trgType.GetField(name, flg);
            if (fld == null) throw new MemberAccessException();

            return fld;
        }

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
