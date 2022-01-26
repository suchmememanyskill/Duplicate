using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LegendaryGUIv2.ViewModels;
using System;

namespace LegendaryGUIv2
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                Control c = (Control)Activator.CreateInstance(type)!;
                if (data is ViewModelBase)
                {
                    ViewModelBase d = data as ViewModelBase;
                    d.Control = c;
                    d.SetControl();
                }
                return c;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
