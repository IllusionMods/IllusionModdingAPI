using BepInEx.Configuration;
using System.Linq;

namespace KKAPI.Utilities
{
    /// <summary>
    /// <see cref="AcceptableValueList{T}"/> for enums
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AcceptableValueEnums<T> : AcceptableValueBase where T : System.Enum
    {
        /// <summary>
        /// List of values that a setting can take.
        /// </summary>
        public virtual T[] AcceptableValues { get; }

        /// <summary>
        /// Specify the list of acceptable values for a setting.
        /// If the setting does not equal any of the values, it will be set to the first one.
        /// </summary>
        /// <param name="acceptableValues"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public AcceptableValueEnums(params T[] acceptableValues)
            : base(typeof(T))
        {
            if (acceptableValues == null)
            {
                throw new System.ArgumentNullException("acceptableValues");
            }

            if (acceptableValues.Length == 0)
            {
                throw new System.ArgumentException("At least one acceptable value is needed", "acceptableValues");
            }

            AcceptableValues = acceptableValues;
        }

        /// <summary>
        /// Clamp the given value inside the options.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object Clamp(object value)
        {
            if (IsValid(value))
            {
                return value;
            }

            return AcceptableValues[0];
        }

        /// <summary>
        /// Test whether a given value is valid for the specified options.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (value is T v)
            {
                return AcceptableValues.Any(x => x.Equals(v));
            }

            return false;
        }

        /// <summary>
        /// Stringify the options in a properly formatted manner.
        /// </summary>
        /// <returns></returns>
        public override string ToDescriptionString()
        {
            return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select(x => x.ToString()).ToArray());
        }
    }
}