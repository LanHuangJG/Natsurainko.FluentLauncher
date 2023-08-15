using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class ResourceItemPage : Page
{
    private readonly InterfaceCacheService _interfaceCacheService = App.GetService<InterfaceCacheService>();

    public ResourceItemPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = e.Parameter;

        bool isCurse = e.Parameter is CurseResource;

        var urls = isCurse ? ((CurseResource)e.Parameter).ScreenshotUrls : ((ModrinthResource)e.Parameter).ScreenshotUrls;
        object id = isCurse ? ((CurseResource)e.Parameter).Id : ((ModrinthResource)e.Parameter).Id;

        if (!(urls?.ToList().Any()).GetValueOrDefault())
            stackPanel.Children.Remove(ScreenshotsBorder);

        Task.Run(() =>
        {
            string markdown = default;

            if (!isCurse)
            {
                markdown = _interfaceCacheService.ModrinthClient.GetResourceDescription((string)id);
            }
            else
            {
                var des = _interfaceCacheService.CurseForgeClient.GetResourceDescription((int)id);
                markdown = new ReverseMarkdown.Converter().Convert(des);
            }

            App.MainWindow.DispatcherQueue.TryEnqueue(() => markdownTextBlock.Text = markdown);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                App.MainWindow.DispatcherQueue.TryEnqueue(() => stackPanel.Children.Remove(descriptionBorder));
        });
    }
}
