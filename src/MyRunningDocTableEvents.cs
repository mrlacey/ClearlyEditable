﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using WpfColorHelper;

namespace ClearlyEditable
{
	internal class MyRunningDocTableEvents : IVsRunningDocTableEvents
	{
		private static MyRunningDocTableEvents instance;

		private readonly Dictionary<uint, IVsWindowFrame> cache = new Dictionary<uint, IVsWindowFrame>();
		private ClearlyEditablePackage package;
		private RunningDocumentTable runningDocumentTable;
		private IVsEditorAdaptersFactoryService editorAdaptersFactory;

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
			// Ensure coloring is up to date after save.
			// This is an extra fallback if background readonly change wasn't already detected.
			this.RefreshWindow(docCookie);

			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
		{
			// Ensure coloring is up to date after any change. (e.g. background readonly change.)
			this.RefreshWindow(docCookie);

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
				if (this.package?.Options?.IsEnabled ?? false)
				{
					var documentInfo = this.runningDocumentTable?.GetDocumentInfo(docCookie);

					if (documentInfo == null)
					{
						return;
					}

					var documentPath = documentInfo?.Moniker;

					if (documentPath == null)
					{
						return;
					}

					if (!this.package?.Options?.FileExtensionList.Contains(System.IO.Path.GetExtension(documentPath)) ?? false)
					{
						return;
					}

					if (this.package?.Options?.GeneratedEnabled ?? false)
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

									if (fileContent != null)
									{

										// Try and avoid false positives by only looking at the top of the file's contents
										if (fileContent.Length > 400)
										{
											fileContent = fileContent.Substring(0, 400);
										}

										foreach (var identifier in this.package?.Options?.GenIndicatorList)
										{
											if (fileContent.Contains(identifier))
											{
												isGenerated = true;
												break;
											}
										}
									}
								}
							}
							catch (Exception exc)
							{
								// Because working with the file-system can be tricky.
								System.Diagnostics.Debug.WriteLine(exc);
							}
						}

						if (isGenerated)
						{
							bg = ColorHelper.GetColorBrush(
								this.package.Options.GeneratedColor,
								ColorHelper.RationalizeOpacity(this.package.Options.GeneratedOpacity));
						}
					}

					if (bg == null && (this.package?.Options?.ReadOnlyEnabled ?? false))
					{
						// Internally the editor already knows if this file is read-only but no supported way of knowing from an extension is provided.
						// So, need to look up read-only status directly
						var fileInfo = new System.IO.FileInfo(documentPath);

						if (fileInfo?.IsReadOnly ?? false)
						{
							bg = ColorHelper.GetColorBrush(
								this.package.Options.ReadOnlyColor,
								ColorHelper.RationalizeOpacity(this.package.Options.ReadOnlyOpacity));
						}
					}

					if (bg == null && (this.package?.Options?.TempEnabled ?? false))
					{
						if (documentPath.ContainsAnyOf("/temp/", "\\temp\\", "/tmp/", "\\tmp\\"))
						{
							bg = ColorHelper.GetColorBrush(
								this.package.Options.TempColor,
								ColorHelper.RationalizeOpacity(this.package.Options.TempOpacity));
						}
					}

					if (bg == null && (this.package?.Options?.LinkEnabled ?? false))
					{
						var dte2 = (DTE2)Package.GetGlobalService(typeof(DTE));
						var projItem = dte2.Solution.FindProjectItem(documentPath);
						var linkProperty = projItem.Properties.Item("IsLink");

						if (linkProperty != null && (bool)linkProperty.Value == true)
						{
							bg = ColorHelper.GetColorBrush(
								this.package.Options.LinkColor,
								ColorHelper.RationalizeOpacity(this.package.Options.LinkOpacity));
						}
					}
				}

				IWpfTextView wpfView = null;

				try
				{
					wpfView = this.GetWpfTextView(this.cache[docCookie]);
				}
				catch (Exception exc)
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
			catch (System.Exception exc)
			{
				ThreadHelper.ThrowIfNotOnUIThread();

				OutputPane.Instance.WriteLine(exc.Message);
				OutputPane.Instance.WriteLine(exc.Source);
				OutputPane.Instance.WriteLine(exc.StackTrace);
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
