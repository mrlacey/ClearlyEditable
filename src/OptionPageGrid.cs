﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace ClearlyEditable
{
    public class OptionPageGrid : DialogPage
    {
        [Category("General")]
        [DisplayName("Enabled")]
        [Description("Enable/disable all functionality.")]
        public bool IsEnabled { get; set; } = true;

        [Category("General")]
        [DisplayName("Extensions of interest")]
        [Description("Comma-delimited list of file extentions to receive editing indication.")]
        public string FileExtensions { get; set; } = ".cs,.vb,.cpp,.fs,.js";

        [Category("ReadOnly files")]
        [DisplayName("Enabled")]
        [Description("Enable/disable changing the color of ReadOnly files.")]
        public bool ReadOnlyEnabled { get; set; } = true;

        [Category("ReadOnly files")]
        [DisplayName("Background Color")]
        [Description("The color to use for the editor's background. Can be a named value or Hex (e.g. '#FF00FF')")]
        public string ReadOnlyColor { get; set; } = "Crimson";

        [Category("ReadOnly files")]
        [DisplayName("Background Opacity %")]
        [Description("The opacity of the background (as a percentage)")]
        public int ReadOnlyOpacity { get; set; } = 10;

        [Category("Generated files")]
        [DisplayName("Enabled")]
        [Description("Enable/disable changing the color of Generated files.")]
        public bool GeneratedEnabled { get; set; } = true;

        [Category("Generated files")]
        [DisplayName("Background Color")]
        [Description("The color to use for the editor's background. Can be a named value or Hex (e.g. '#FF00FF')")]
        public string GeneratedColor { get; set; } = "PaleGreen";

        [Category("Generated files")]
        [DisplayName("Background Opacity %")]
        [Description("The opacity of the background (as a percentage)")]
        public int GeneratedOpacity { get; set; } = 10;

        [Category("Generated files")]
        [DisplayName("Generation Indicators")]
        [Description("Text that indicates a file was generated. Separate multiple entries with pipe (|) characters.")]
        public string GenerationIndicators { get; set; } = "<auto-generated |<autogenerated |</auto-generated>|<auto-generated/>";

        [Category("Temporary files")]
        [DisplayName("Enabled")]
        [Description("Enable/disable changing the color of temporary files.")]
        public bool TempEnabled { get; set; } = true;

        [Category("Temporary files")]
        [DisplayName("Background Color")]
        [Description("The color to use for the editor's background. Can be a named value or Hex (e.g. '#FF00FF')")]
        public string TempColor { get; set; } = "Gold";

        [Category("Temporary files")]
        [DisplayName("Background Opacity %")]
        [Description("The opacity of the background (as a percentage)")]
        public int TempOpacity { get; set; } = 10;

        public List<string> GenIndicatorList
        {
            get
            {
                return this.GenerationIndicators.Split(
                    new[] { '|' },
                    StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public List<string> FileExtensionList
        {
            get
            {
                return this.FileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Make sure any open documents are updated to reflect any changes to options.
            MyRunningDocTableEvents.Instance.RefreshAll();

            base.OnClosed(e);
        }
    }
}
