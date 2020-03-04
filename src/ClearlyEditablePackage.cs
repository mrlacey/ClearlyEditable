using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Task = System.Threading.Tasks.Task;

namespace ClearlyEditable
{
    [ProvideAutoLoad(UIContextGuids.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(ClearlyEditablePackage.PackageGuidString)]
    public sealed class ClearlyEditablePackage : AsyncPackage
    {
        /// <summary>
        /// ClearlyEditablePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "aaa6f2f0-3c79-4d1f-95e4-3c868411475f";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await this.SetUpRunningDocumentTableEventsAsync(cancellationToken);
        }

        private async Task SetUpRunningDocumentTableEventsAsync(CancellationToken cancellationToken)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var runningDocumentTable = new RunningDocumentTable(this);

            IComponentModel componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            IVsEditorAdaptersFactoryService editorFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            var plugin = new RapidXamlRunningDocTableEvents(this, runningDocumentTable, editorFactory);

            runningDocumentTable.Advise(plugin);
        }
    }

    internal class RapidXamlRunningDocTableEvents : IVsRunningDocTableEvents
    {
        private readonly AsyncPackage package;
        private readonly RunningDocumentTable runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactory;

        public RapidXamlRunningDocTableEvents(AsyncPackage package, RunningDocumentTable runningDocumentTable, IVsEditorAdaptersFactoryService editorAdaptersFactory)
        {
            this.package = package;
            this.runningDocumentTable = runningDocumentTable;
            this.editorAdaptersFactory = editorAdaptersFactory;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            var wpfView = this.GetWpfTextView(pFrame);

            if (wpfView != null)
            {
                var documentInfo = this.runningDocumentTable.GetDocumentInfo(docCookie);

                var documentPath = documentInfo.Moniker;

                if (documentPath.Contains(".g."))
                {
                    wpfView.Background = new SolidColorBrush(Colors.PaleGreen) { Opacity = 0.1 };
                }
                else
                {
                    var fileInfo = new System.IO.FileInfo(documentPath);

                    if (fileInfo.IsReadOnly)
                    {
                        wpfView.Background = new SolidColorBrush(Colors.Crimson) { Opacity = 0.1 };
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        private IWpfTextView GetWpfTextView(IVsWindowFrame frame)
        {
            if (frame == null)
            {
                return null;
            }

            var textView = VsShellUtilities.GetTextView(frame);
            if (textView == null)
            {
                return null;
            }

            return this.editorAdaptersFactory.GetWpfTextView(textView);
        }
    }
}
