using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Phrenapates.Utils;

public static class CollectionEntryExt
{
    public static void Reload(this CollectionEntry source)
    {
        if (source.CurrentValue != null)
        {
            foreach (var item in source.CurrentValue)
            {
                source.EntityEntry.Context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            source.CurrentValue = null;
            source.IsLoaded = false;
            source.Load();
        }
    }
}