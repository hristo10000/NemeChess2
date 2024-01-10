using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace NemeChess2
{
    public partial class CustomPopUp : Window
    {
        public CustomPopUp(string message)
        {
            InitializeComponent();
            var messageTextBlock = this.FindControl<TextBlock>("MessageText");

            Dispatcher.UIThread.Post(() =>
            {
                messageTextBlock.Text = message;
                messageTextBlock.Foreground = Brushes.White;
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
