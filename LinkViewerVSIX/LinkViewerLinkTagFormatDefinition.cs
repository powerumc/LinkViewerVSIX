using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace LinkViewerVSIX
{
	[Export(typeof(EditorFormatDefinition))]
	[UserVisible(true)]
	[Name(NAME_LinkViewerLinkTagFormatDefinition)]
	public class LinkViewerLinkTagFormatDefinition : MarkerFormatDefinition
	{
		internal const string NAME_LinkViewerLinkTagFormatDefinition = "LinkViewer/LinkViewerLinkTagFormatDefinition";

		public LinkViewerLinkTagFormatDefinition()
		{
			this.DisplayName = NAME_LinkViewerLinkTagFormatDefinition;
			this.ForegroundColor = Colors.Yellow;
			this.ZOrder = 5;

		}
	}
}
