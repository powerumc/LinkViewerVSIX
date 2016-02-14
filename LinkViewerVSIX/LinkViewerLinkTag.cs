using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace LinkViewerVSIX
{

	public class LinkViewerLinkTag : IntraTextAdornmentTag //TextMarkerTag
	{
		//public LinkViewerLinkTag() : base(LinkViewerLinkTagFormatDefinition.NAME_LinkViewerLinkTagFormatDefinition) { }

		public LinkViewerLinkTag()
			: this(new Label()  {Content = "ABC", Foreground = Brushes.Yellow}, null)
		{ }
		
		public LinkViewerLinkTag(UIElement adornment, AdornmentRemovedCallback removalCallback, double? topSpace, double? baseline, double? textHeight, double? bottomSpace, PositionAffinity? affinity) : base(adornment, removalCallback, topSpace, baseline, textHeight, bottomSpace, affinity)
		{
		}

		public LinkViewerLinkTag(UIElement adornment, AdornmentRemovedCallback removalCallback, PositionAffinity? affinity) : base(adornment, removalCallback, affinity)
		{
		}

		public LinkViewerLinkTag(UIElement adornment, AdornmentRemovedCallback removalCallback) : base(adornment, removalCallback)
		{
		}
	}
}
