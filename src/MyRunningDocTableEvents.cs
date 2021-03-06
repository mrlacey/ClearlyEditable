﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
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
        private static MyRunningDocTableEvents instance;

        private ClearlyEditablePackage package;
        private RunningDocumentTable runningDocumentTable;
        private IVsEditorAdaptersFactoryService editorAdaptersFactory;
        private Dictionary<uint, IVsWindowFrame> cache = new Dictionary<uint, IVsWindowFrame>();

        public MyRunningDocTableEvents()
        {
        }

        public static MyRunningDocTableEvents Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MyRunningDocTableEvents();
                }

                return instance;
            }
        }

        public void Initialize(ClearlyEditablePackage package, RunningDocumentTable runningDocumentTable, IVsEditorAdaptersFactoryService editorAdaptersFactory)
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

        public void RefreshAll()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var item in this.cache)
            {
                this.RefreshWindow(item.Key);
            }
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Keep the cached frame reference up to date.
            if (!this.cache.ContainsKey(docCookie))
            {
                this.cache.Add(docCookie, pFrame);
            }
            else
            {
                this.cache[docCookie] = pFrame;
            }

            this.RefreshWindow(docCookie);

            return VSConstants.S_OK;
        }

        public void RefreshWindow(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SolidColorBrush bg = null;
            try
            {
                if (this.package.Options.IsEnabled)
                {
                    var documentInfo = this.runningDocumentTable.GetDocumentInfo(docCookie);

                    var documentPath = documentInfo.Moniker;

                    if (!this.package.Options.FileExtensionList.Contains(System.IO.Path.GetExtension(documentPath)))
                    {
                        return;
                    }

                    if (this.package.Options.GeneratedEnabled)
                    {
                        var isGenerated = false;

                        if (documentPath.Contains(".g."))
                        {
                            isGenerated = true;
                        }
                        else
                        {
                            try
                            {
                                // File may be generated in a temporary location or in memory and so not be accessible.
                                if (System.IO.File.Exists(documentPath))
                                {
                                    var fileContent = System.IO.File.ReadAllText(documentPath);

                                    foreach (var identifier in this.package.Options.GenIndicatorList)
                                    {
                                        if (fileContent.Contains(identifier))
                                        {
                                            isGenerated = true;
                                            break;
                                        }
                                    }
                                }
                            }
#pragma warning disable CA1031 // Do not catch general exception types
                            catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
                            {
                                // Because working with the file-system can be tricky.
                                System.Diagnostics.Debug.WriteLine(exc);
                            }
                        }

                        if (isGenerated)
                        {
                            bg = ColorHelpers.GetColorBrush(
                                this.package.Options.GeneratedColor,
                                ColorHelpers.RationalizeOpacity(this.package.Options.GeneratedOpacity));
                        }
                    }

                    if (bg == null && this.package.Options.ReadOnlyEnabled)
                    {
                        // Internally the editor already knows if this file is read-only but no supported way of knowing from an extension is provided.
                        // So, need to look up read-only status directly
                        var fileInfo = new System.IO.FileInfo(documentPath);

                        if (fileInfo.IsReadOnly)
                        {
                            bg = ColorHelpers.GetColorBrush(
                                this.package.Options.ReadOnlyColor,
                                ColorHelpers.RationalizeOpacity(this.package.Options.ReadOnlyOpacity));
                        }
                    }
                }

                IWpfTextView wpfView = null;

                try
                {
                    wpfView = this.GetWpfTextView(this.cache[docCookie]);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // The cached IVsWindowFrame instances aren't long lasting and so may not be usable.
                    // If can't get the WpfTextView don't worry. Will likely get an updated frame soon.
                    System.Diagnostics.Debug.WriteLine(exc);
                }

                if (wpfView != null)
                {
                    if (bg == null)
                    {
                        if (wpfView.Background != null)
                        {
                            wpfView.Background = new SolidColorBrush(Colors.Transparent);
                        }
                    }
                    else
                    {
                        wpfView.Background = bg;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                GeneralOutputPane.Instance.Write(exc.Message);
                GeneralOutputPane.Instance.Write(exc.Source);
                GeneralOutputPane.Instance.Write(exc.StackTrace);
            }
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
