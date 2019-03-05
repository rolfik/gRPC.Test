using System;

namespace Epos.Service.Interface.Notifications
{
    /// <summary>
    /// Data item change notifications
    /// </summary>
    public static class ItemChanges
    {
        /// <summary>
        /// Raised (with OnItemsChanged) when item changes occure
        /// </summary>
        /// <remarks>This event should be raised and handled by server (sent to interested clients) and clients (receive from server) and both should update item caches</remarks>
        public static event Action<ItemsChanged> ItemsChanged;

        public static bool OnItemsChanged(ItemsChanged change) => OnItemsChanged(ItemsChanged, change);

        private static bool OnItemsChanged(Action<ItemsChanged> itemsChanged, ItemsChanged change)
        {
            Console.WriteLine($"{nameof(ItemsChanged)}:\n{change}");
            if (
                itemsChanged is null ||
                !change.HasChanges
                )
            {
                return false;
            }
            itemsChanged(change);
            return true;
        }
    }
}
