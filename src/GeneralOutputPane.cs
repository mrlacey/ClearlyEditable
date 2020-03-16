﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClearlyEditable
{
    public class GeneralOutputPane
    {
        private static GeneralOutputPane instance;

        private readonly IVsOutputWindowPane generalPane;

        private GeneralOutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;

            if (ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outWindow
             && (ErrorHandler.Failed(outWindow.GetPane(ref generalPaneGuid, out this.generalPane)) || this.generalPane == null))
            {
                if (ErrorHandler.Failed(outWindow.CreatePane(ref generalPaneGuid, "General", 1, 0)))
                {
                    System.Diagnostics.Debug.WriteLine("Creating Output Pane Failed");
                    return;
                }

                if (ErrorHandler.Failed(outWindow.GetPane(ref generalPaneGuid, out this.generalPane)) || (this.generalPane == null))
                {
                    System.Diagnostics.Debug.WriteLine("Accessing Output Pane Failed");
                }
            }
        }

        public static GeneralOutputPane Instance => instance ?? (instance = new GeneralOutputPane());

        public void Activate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.generalPane?.Activate();
        }

        public void Write(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.generalPane?.OutputString($"Clearly Editable - {message}{Environment.NewLine}");
        }
    }
}
