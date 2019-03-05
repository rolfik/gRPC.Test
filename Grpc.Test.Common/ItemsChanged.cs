using System.Linq;

namespace Epos.Service.Interface.Notifications
{
    public partial class ItemsChanged
    {
        /// <summary>
        /// Represents any change which cannot be determined (because client lost server connection etc.)
        /// </summary>
        public static readonly ItemsChanged Any = new ItemsChanged
        {
            Name = "?",
            AddedIds = { string.Empty },
            UpdatedIds = { string.Empty },
            RemovedIds = { string.Empty }
        };

        public bool HasChanges =>
            Added ||
            Updated ||
            Removed;

        public bool Added => AddedIds.Count > 0;
        public bool Updated => UpdatedIds.Count > 0;
        public bool Removed => RemovedIds.Count > 0;

        public bool? IdsAvailable => HasChanges ?
            AddedIds.FirstOrDefault() != string.Empty &&
            UpdatedIds.FirstOrDefault() != string.Empty &&
            RemovedIds.FirstOrDefault() != string.Empty :
            (bool?)null;
    }
}
