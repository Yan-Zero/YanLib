using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// From BepInEx https://github.com/BepInEx/BepInEx

namespace YanLib.ModHelper
{
    /// <summary>
    ///     Arguments for events concerning a change of a setting.
    /// </summary>
    /// <inheritdoc />
    public sealed class SettingChangedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public SettingChangedEventArgs(ConfigEntryBase changedSetting)
        {
            ChangedSetting = changedSetting;
        }

        /// <summary>
        ///     Setting that was changed
        /// </summary>
        public ConfigEntryBase ChangedSetting { get; }
    }


}