using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Camera2.UI
{
    internal class NotifiableSettingsObj : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"Error Invoking PropertyChanged: {ex.Message}");
                Plugin.Log?.Error(ex);
            }
        }

        internal void NotifyPropertiesChanged()
        {
            foreach (var x in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                NotifyPropertyChanged(x.Name);
            }
        }
    }
}