using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Animation;
using Avalonia.Markup.Xaml;

public partial class CustomPopUp : Window
{
    public CustomPopUp(string message)
    {
        InitializeComponent();
        this.FindControl<TextBlock>("MessageText").Text = message;

        var image = this.FindControl < a:Image > ("FireworksGif");
        image.Source = new Bitmap("fireworks.gif");
        image.Play();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
