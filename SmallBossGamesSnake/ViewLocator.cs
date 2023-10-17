using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaNativeApplication1.ViewModels;
using AvaloniaNativeApplication1.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AvaloniaNativeApplication1
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object data)
        {
            return data switch
            {
                GameCanvasViewModel vm => new GameCanvas()
                {
                    DataContext = vm,
                },
                _ => new TextBlock { Text = "Not Found control" }
            };
        }

        public bool Match(object data)
        {
            return data is ObservableObject;
        }
    }
}