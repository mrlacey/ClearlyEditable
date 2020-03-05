// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ClearlyEditable
{
    public class OptionPageGrid : DialogPage
    {
        [Category("General")]
        [DisplayName("Enabled")]
        [Description("Enable/disable all functionality.")]
        public bool IsEnabled { get; set; } = true;

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
    }
}
