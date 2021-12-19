using Avalonia.Controls;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUIv2.Services
{
    public static class Utils
    {
        public static IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message, ButtonEnum buttons = ButtonEnum.Ok) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title,
                ContentMessage = message,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
            });
    }
}
