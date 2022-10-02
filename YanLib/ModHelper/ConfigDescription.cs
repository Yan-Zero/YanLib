using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// From BepInEx https://github.com/BepInEx/BepInEx/

namespace YanLib.ModHelper
{
    /// <summary>
    ///     Metadata of a <see cref="ConfigEntryBase" />.
    /// </summary>
    public class ConfigDescription
    {
        /// <summary>
        ///     Create a new description.
        /// </summary>
        /// <param name="description">Text describing the function of the setting and any notes or warnings.</param>
        /// <param name="acceptableValues">
        ///     Range of values that this setting can take. The setting's value will be automatically
        ///     clamped.
        /// </param>
        /// <param name="tags">Objects that can be used by user-made classes to add functionality.</param>
        public ConfigDescription(string description, AcceptableValueBase acceptableValues = null, params object[] tags)
        {
            AcceptableValues = acceptableValues;
            Tags = tags;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        ///     Text describing the function of the setting and any notes or warnings.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Range of acceptable values for a setting.
        /// </summary>
        public AcceptableValueBase AcceptableValues { get; }

        /// <summary>
        ///     Objects that can be used by user-made classes to add functionality.
        /// </summary>
        public object[] Tags { get; }

        /// <summary>
        ///     An empty description.
        /// </summary>
        public static ConfigDescription Empty { get; } = new ConfigDescription("");
    }

    /// <summary>
    ///     Base type of all classes representing and enforcing acceptable values of config settings.
    /// </summary>
    public abstract class AcceptableValueBase
    {
        /// <param name="valueType">Type of values that this class can Clamp.</param>
        protected AcceptableValueBase(Type valueType)
        {
            ValueType = valueType;
        }

        /// <summary>
        ///     Type of the supported values.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        ///     Change the value to be acceptable, if it's not already.
        /// </summary>
        public abstract object Clamp(object value);

        /// <summary>
        ///     Check if the value is an acceptable value.
        /// </summary>
        public abstract bool IsValid(object value);

        /// <summary>
        ///     Get the string for use in config files.
        /// </summary>
        public abstract string ToDescriptionString();
    }
}
