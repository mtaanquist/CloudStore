using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CloudStoreApp.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        VerifyPropertyName(propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        // Verify that the property name matches a real, 
        // public, instance property on this object. 
        if (TypeDescriptor.GetProperties(this)[propertyName] is null)
        {
            string msg = $"Invalid property name: {propertyName}";
            Debug.Fail(msg);
        }
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // New C# 13 auto-property with notification using 'field' keyword
    // This eliminates the need for backing fields in derived classes
    protected T GetProperty<T>([CallerMemberName] string propertyName = "") => throw new NotImplementedException("Use auto-property with field keyword instead");
    
    protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = "") => throw new NotImplementedException("Use auto-property with field keyword instead");
}
