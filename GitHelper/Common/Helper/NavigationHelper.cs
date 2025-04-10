using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace GitHelper.Common
{
    public static class NavigationHelper
    {
        public static bool Navigate<TSource, TTarget>() where TSource : Page where TTarget : Page
        {
            var targetPage = App.ServiceProvider.GetRequiredService<TTarget>();
            return App.ServiceProvider.GetRequiredService<TSource>().NavigationService.Navigate(targetPage);
        }

        public static async Task<bool> NavigateAsync<TSource, TTarget, TTargetModel>(Func<TTargetModel, Task> func)
            where TSource : Page
            where TTarget : Page
            where TTargetModel : ObservableObject
        {
            var targetPage = App.ServiceProvider.GetRequiredService<TTarget>();

            TTargetModel targetModel = (TTargetModel)targetPage.DataContext;

            await func.Invoke(targetModel);

            return App.ServiceProvider.GetRequiredService<TSource>().NavigationService.Navigate(targetPage);
        }

        public static void GoBack<TSource>() where TSource : Page
        {
            App.ServiceProvider.GetRequiredService<TSource>().NavigationService.GoBack();
        }
    }
}
