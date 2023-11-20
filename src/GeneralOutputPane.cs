// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClearlyEditable
{
    public class OutputPane
    {
        private static Guid cePaneGuid = new Guid("12E59E5C-9C9B-421E-8A91-9B7970582DB8");

        private static OutputPane instance;

        private readonly IVsOutputWindowPane pane;

        private OutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outWindow
             && (ErrorHandler.Failed(outWindow.GetPane(ref cePaneGuid, out this.pane)) || this.pane == null))
            {
                if (ErrorHandler.Failed(outWindow.CreatePane(ref cePaneGuid, Vsix.Name, 1, 0)))
                {
                    System.Diagnostics.Debug.WriteLine("Creating Output Pane Failed");
                    return;
                }

                if (ErrorHandler.Failed(outWindow.GetPane(ref cePaneGuid, out this.pane)) || (this.pane == null))
                {
                    System.Diagnostics.Debug.WriteLine("Accessing Output Pane Failed");
                }
            }
        }

        public static OutputPane Instance => instance ?? (instance = new OutputPane());

        public void Activate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.pane?.Activate();
        }

        public void Write(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.pane?.OutputStringThreadSafe(message);
        }

        public void WriteLine(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.pane?.OutputStringThreadSafe($"{message}{Environment.NewLine}");
        }
    }
}
