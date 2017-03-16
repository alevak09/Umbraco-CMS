using Umbraco.Core.IO;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager is created for each scope and is aware of if it is nested in an outer scope
    /// </summary>
    /// <remarks>
    /// The outer scope is the only scope that can raise events, the inner scope's will defer to the outer scope
    /// </remarks>
    internal class ScopeEventDispatcher : ScopeEventDispatcherBase
    {
        public ScopeEventDispatcher()
            : base(true)
        { }

        protected override void ScopeExitCompleted()
        {
            // processing only the last instance of each event...
            // this is probably far from perfect, because if eg a content is saved in a list
            // and then as a single content, the two events will probably not be de-duplicated,
            // but it's better than nothing

            foreach (var e in GetEvents(EventDefinitionFilter.LastIn))
            {
                e.RaiseEvent();

                // separating concerns means that this should probably not be here,
                // but then where should it be (without making things too complicated)?
                var delete = e.Args as IDeletingMediaFilesEventArgs;
                if (delete != null && delete.MediaFilesToDelete.Count > 0)
                    MediaFileSystem.DeleteMediaFiles(delete.MediaFilesToDelete);
            }
        }

        private MediaFileSystem _mediaFileSystem;

        private MediaFileSystem MediaFileSystem
        {
            get
            {
                return _mediaFileSystem ?? (_mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem);
            }
        }
    }
}