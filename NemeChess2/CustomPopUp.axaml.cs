using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NemeChess2
{
    public partial class CustomPopUp : Window//TODO: when the game has ended, close the stream, open this window, add fireworks
    {
        public CustomPopUp(string message)
        {
            InitializeComponent();
            this.FindControl<TextBlock>("MessageText").Text = message;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
