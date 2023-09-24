﻿using Semver;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using VolumeControl.Core;
using VolumeControl.Helpers.Win32;
using VolumeControl.Log;
using VolumeControl.WPF;
using VolumeControl.WPF.Collections;

namespace VolumeControl.Helpers
{
    public abstract class VCSettings : INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Constructors
        public VCSettings()
        {
            Log.Debug($"{nameof(VCSettings)} initializing...");

            // Initialize the HWndHook
            this.HWndHook = new(WindowHandleGetter.GetHwndSource(this.MainWindowHandle = WindowHandleGetter.GetWindowHandle()));
            this.HWndHook.AddMaximizeBugFixHandler();

            // Get the executable path
            this.ExecutablePath = GetExecutablePath();
            Log.Debug($"{nameof(VCSettings)}.{nameof(this.ExecutablePath)} = '{this.ExecutablePath}'");

            // Get the current version number
            this.CurrentVersion = Settings.__VERSION__;

#       if DEBUG // Show 'DEBUG' in the version string when in Debug configuration
            this.CurrentVersionString = $"DEBUG | {this.CurrentVersion}";
#       else // Use the normal version string in Release configuration
            this.CurrentVersionString = this.CurrentVersion.ToString();
#       endif

#       if DEBUG // Debug configuration:
            Log.Debug($"Volume Control version {this.CurrentVersion} (DEBUG)");
#       elif RELEASE // Release configuration:
            Log.Debug($"Volume Control version {this.CurrentVersion} (Portable)");
#       elif RELEASE_FORINSTALLER // Release-ForInstaller configuration:
            Log.Debug($"Volume Control version {this.CurrentVersion} (Installed)");
#       endif

            Log.Debug($"{nameof(VCSettings)} initialization completed.");
        }
        #endregion Constructors

        #region Properties
        #region Statics
        private static Config Settings => (Config.Default as Config)!;
        private static LogWriter Log => FLog.Log;
        #endregion Statics
        #region ReadOnlyProperties
        /// <inheritdoc/>
        public IntPtr MainWindowHandle { get; }
        /// <inheritdoc/>
        public HWndHook HWndHook { get; }
        /// <inheritdoc/>
        public string ExecutablePath { get; }
        /// <inheritdoc/>
        public string CurrentVersionString { get; }
        /// <inheritdoc/>
        public SemVersion CurrentVersion { get; }
        #endregion ReadOnlyProperties
        /// <inheritdoc cref="Config.ShowIcons"/>
        public bool ShowIcons
        {
            get => Settings.ShowIcons;
            set => Settings.ShowIcons = value;
        }
        /// <inheritdoc cref="Config.DeleteHotkeyConfirmation"/>
        public bool DeleteHotkeyConfirmation
        {
            get => Settings.DeleteHotkeyConfirmation;
            set => Settings.DeleteHotkeyConfirmation = value;
        }
        /// <inheritdoc cref="Config.RunAtStartup"/>
        public bool? RunAtStartup
        {
            get => !RunAtStartupHelper.ValueEquals(this.ExecutablePath) && !RunAtStartupHelper.ValueEqualsNull() ? null : Settings.RunAtStartup;
            set => RunAtStartupHelper.Value = (Settings.RunAtStartup = value ?? false) ? this.ExecutablePath : null;
        }
        /// <inheritdoc cref="Config.StartMinimized"/>
        public bool StartMinimized
        {
            get => Settings.StartMinimized;
            set => Settings.StartMinimized = value;
        }
        /// <inheritdoc cref="Config.CheckForUpdates"/>
        public bool CheckForUpdates
        {
            get => Settings.CheckForUpdates;
            set => Settings.CheckForUpdates = value;
        }
        /// <inheritdoc cref="Config.ShowUpdatePrompt"/>
        public bool ShowUpdateMessageBox
        {
            get => Settings.ShowUpdatePrompt;
            set => Settings.ShowUpdatePrompt = value;
        }
        public ObservableImmutableList<string> CustomLocalizationDirectories
        {
            get => Settings.CustomLocalizationDirectories;
            set => Settings.CustomLocalizationDirectories = value;
        }
        public ObservableImmutableList<string> CustomAddonDirectories
        {
            get => Settings.CustomAddonDirectories;
            set => Settings.CustomAddonDirectories = value;
        }
        public int PeakMeterUpdateIntervalMs
        {
            get => Settings.PeakMeterUpdateIntervalMs;
            set => Settings.PeakMeterUpdateIntervalMs = value;
        }
        public bool ShowPeakMeters
        {
            get => Settings.ShowPeakMeters;
            set => Settings.ShowPeakMeters = value;
        }
        public bool EnableDeviceControl
        {
            get => Settings.EnableDeviceControl;
            set => Settings.EnableDeviceControl = value;
        }
        public bool AlwaysOnTop
        {
            get => Settings.AlwaysOnTop;
            set => Settings.AlwaysOnTop = value;
        }
        public bool ShowInTaskbar
        {
            get => Settings.ShowInTaskbar;
            set => Settings.ShowInTaskbar = value;
        }
        public bool AllowMultipleDistinctInstances
        {
            get => Settings.AllowMultipleDistinctInstances;
            set => Settings.AllowMultipleDistinctInstances = value;
        }
        public int VolumeStepSize
        {
            get => Settings.VolumeStepSize;
            set => Settings.VolumeStepSize = value;
        }
        /// <inheritdoc cref="Config.SessionListNotificationConfig"/>
        public NotificationConfigSection SessionListNotificationConfig
        {
            get => Settings.SessionListNotificationConfig;
            set => Settings.SessionListNotificationConfig = value;
        }
        /// <inheritdoc cref="Config.DeviceListNotificationConfig"/>
        public NotificationConfigSection DeviceListNotificationConfig
        {
            get => Settings.DeviceListNotificationConfig;
            set => Settings.DeviceListNotificationConfig = value;
        }
        /// <inheritdoc cref="Config.NotificationMoveRequiresAlt"/>
        public bool NotificationDragRequiresAlt
        {
            get => Settings.NotificationMoveRequiresAlt;
            set => Settings.NotificationMoveRequiresAlt = value;
        }
        /// <inheritdoc cref="Config.NotificationSavePos"/>
        public bool NotificationSavesPosition
        {
            get => Settings.NotificationSavePos;
            set => Settings.NotificationSavePos = value;
        }
        /// <inheritdoc cref="Config.LockTargetSession"/>
        public bool LockTargetSession
        {
            get => Settings.LockTargetSession;
            set => Settings.LockTargetSession = value;
        }
        /// <inheritdoc cref="Config.LockTargetDevice"/>
        public bool LockTargetDevice
        {
            get => Settings.LockTargetDevice;
            set => Settings.LockTargetDevice = value;
        }
        /// <summary>
        /// This is read-only since there wouldn't be a way for volume control to find the config again after restarting
        /// </summary>
        public string ConfigLocation => System.IO.Path.Combine(Environment.CurrentDirectory, Settings.Location);
        /// <inheritdoc cref="Config.HiddenSessionProcessNames"/>
        public ObservableImmutableList<string> HiddenSessionProcessNames
        {
            get => Settings.HiddenSessionProcessNames;
            set => Settings.HiddenSessionProcessNames = value;
        }
        public bool LogEnabled
        {
            get => Settings.EnableLogging;
            set => Settings.EnableLogging = value;
        }
        public string LogFilePath
        {
            get => Settings.LogPath;
            set => Settings.LogPath = value;
        }
        #endregion Properties

        #region Events
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        public abstract event NotifyCollectionChangedEventHandler? CollectionChanged;
        protected void ForceNotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
        #endregion Events

        #region Methods
        private static string GetExecutablePath() => Process.GetCurrentProcess().MainModule?.FileName is string path
                ? path
                : throw new Exception($"{nameof(VCSettings)} Error:  Retrieving the current executable path failed!");
        #endregion Methods
    }
}
