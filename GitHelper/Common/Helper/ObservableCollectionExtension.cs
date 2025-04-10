using System.Collections.ObjectModel;

namespace GitHelper.Common.Helper
{
    public static class ObservableCollectionExtension
    {
        public static void UpdateAll<TSource>(this ObservableCollection<TSource> source, IEnumerable<TSource> update)
        {
            source.Clear();
            foreach (var item in update)
            {
                source.Add(item);
            }
        }
    }
}
