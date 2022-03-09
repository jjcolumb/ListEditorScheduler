using System;
using DevExpress.ExpressApp.Xpo;

namespace ListEditorScheduler.Blazor.Server.Services {
    public class XpoDataStoreProviderAccessor {
        public IXpoDataStoreProvider DataStoreProvider { get; set; }
    }
}
