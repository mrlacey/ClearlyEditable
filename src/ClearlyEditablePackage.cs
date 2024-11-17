// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;
using Task = System.Threading.Tasks.Task;

namespace ClearlyEditable
{
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[Guid(ClearlyEditablePackage.PackageGuidString)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)] // Info on this package for Help/About
	[ProvideOptionPage(typeof(OptionPageGrid), Vsix.Name, "General", 0, 0, true)]
	[ProvideProfileAttribute(typeof(OptionPageGrid), Vsix.Name, "General", 106, 107, isToolsOptionPage: true)]
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

			await this.SetUpRunningDocumentTableEventsAsync(cancellationToken).ConfigureAwait(false);

			// Make sure any documents opened before the package loads are colored correctly;
			MyRunningDocTableEvents.Instance.RefreshAll();

			await SponsorRequestHelper.CheckIfNeedToShowAsync().ConfigureAwait(false);

			TrackBasicUsageAnalytics();
		}

		private static void TrackBasicUsageAnalytics()
		{
#if !DEBUG
			try
			{
				if (string.IsNullOrWhiteSpace(AnalyticsConfig.TelemetryConnectionString))
				{
					return;
				}

				var config = new TelemetryConfiguration
				{
					ConnectionString = AnalyticsConfig.TelemetryConnectionString,
				};

				var client = new TelemetryClient(config);

				var properties = new Dictionary<string, string>
				{
					{ "VsixVersion", Vsix.Version },
					{ "VsVersion", Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession?.GetSharedProperty("VS.Core.ExeVersion") },
				};

				client.TrackEvent(Vsix.Name, properties);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc);
				throw;
			}
#endif
		}

		private async Task SetUpRunningDocumentTableEventsAsync(CancellationToken cancellationToken)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			var runningDocumentTable = new RunningDocumentTable(this);

			var componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel;
			var editorFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

			MyRunningDocTableEvents.Instance.Initialize(this, runningDocumentTable, editorFactory);

			runningDocumentTable.Advise(MyRunningDocTableEvents.Instance);
		}
	}
}
