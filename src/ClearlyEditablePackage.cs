﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClearlyEditable
{
    [ProvideAutoLoad(UIContextGuids.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(ClearlyEditablePackage.PackageGuidString)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideOptionPage(typeof(OptionPageGrid), "Clearly Editable", "General", 0, 0, true)]
    [ProvideProfileAttribute(typeof(OptionPageGrid), "Clearly Editable", "General", 106, 107, isToolsOptionPage: true, DescriptionResourceID = 108)]
    public sealed class ClearlyEditablePackage : AsyncPackage
    {
        public const string PackageGuidString = "aaa6f2f0-3c79-4d1f-95e4-3c868411475f";

        public OptionPageGrid Options
        {
            get
            {
                return (OptionPageGrid)this.GetDialogPage(typeof(OptionPageGrid));
            }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // TODO: force refresh of background colors when package loads
            await this.SetUpRunningDocumentTableEventsAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SetUpRunningDocumentTableEventsAsync(CancellationToken cancellationToken)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var runningDocumentTable = new RunningDocumentTable(this);

            var componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            var editorFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            var plugin = new MyRunningDocTableEvents(this, runningDocumentTable, editorFactory);

            runningDocumentTable.Advise(plugin);
        }
    }
}
