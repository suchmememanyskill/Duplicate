using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LegendaryGUIv2.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public Control? Control { get; set; }
        public virtual void SetControl() { 
        }
    }

    public class ViewModelBase<T> : ViewModelBase where T: Control
    {
        public new T? Control { get; set; }
        public override void SetControl()
        {
            Control = base.Control as T;
        }
    }
}
