using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace ClearlyEditable
{
    internal class MyRunningDocTableEvents : IVsRunningDocTableEvents
    {
        private readonly AsyncPackage package;
        private readonly RunningDocumentTable runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactory;

        public MyRunningDocTableEvents(AsyncPackage package, RunningDocumentTable runningDocumentTable, IVsEditorAdaptersFactoryService editorAdaptersFactory)
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
