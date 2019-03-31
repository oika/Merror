using System;

namespace Oika.Libs.Merror
{
    /// <summary>
    /// A class represents a parameter info to find target class members.
    /// </summary>
    public class ReflectorParam
    {
        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="isRef">True if the target parameter has a ref/out modifier.
        /// If ByRef types are set to 'type' parameter, this value is ignored.</param>
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
        /// Creates new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="isRef">True if the target parameter has a ref/out modifier.</param>
        /// <returns>the instance created.</returns>
        public static ReflectorParam New<T>(T value, bool isRef = false)
        {
            return new ReflectorParam(typeof(T), value, isRef);
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="value"></param>
        internal ReflectorParam(object value)
        {
            this.Type = value == null ? typeof(object) : value.GetType();
            this.Value = value;
        }
    }
}
