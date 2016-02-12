//------------------------------------------------------------------------------
// <copyright file="LinkViewerTextAdornment.cs" company="Microsoft">
//     Copyright (c) Microsoft.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LinkViewerVSIX
{
    /// <summary>
    /// LinkViewerTextAdornment places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class LinkViewerTextAdornment
    {
        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;
        private readonly Brush brush;
        private readonly Pen pen;
	    private Image img;
		public LinkViewerTextAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

			view.VisualElement.MouseMove += VisualElementOnMouseMove;

            this.layer = view.GetAdornmentLayer("LinkViewerTextAdornment");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            this.brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 0.5);
            this.pen.Freeze();
        }

		private Regex regex = new Regex(@"(http|https)[-a-zA-Z0-9:_\+.~#?&//=]{2,256}\.[^@\ ][a-z]{2,12}\b(\/[-a-zA-Z0-9:%_\+.~#?&//=]*)?", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
		private void VisualElementOnMouseMove(object sender, MouseEventArgs args)
	    {
			var position = args.GetPosition(view.VisualElement);
            var lineY = view.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y + view.ViewportTop);
            if (lineY == null) return;

            var lineX = lineY.GetBufferPositionFromXCoordinate(position.X + view.ViewportLeft);
            if (lineX == null) return;

            Debug.WriteLine($"X:{position.X},	Y:{position.Y}");
            if (!lineX.HasValue) return;

            for (var i=lineX.Value.Position; i>=0; i--)
            {
                var snapshotSpan = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, i + 1));
                var c = snapshotSpan.GetText();
                Debug.Write(c);

                    
                if (Regex.IsMatch(c, "[ \"\'<>\t]", RegexOptions.IgnoreCase|RegexOptions.Singleline))
                {

                    var textSnapshotSpan = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, lineY.End.Position));
                    Debug.WriteLine("   " + textSnapshotSpan.GetText());
                    var text = textSnapshotSpan.GetText();

                    if (regex.IsMatch(text))
                    {
                        var match = regex.Match(text);
                        
                        ShowImageAsync(match.Value, new Point(position.X + view.ViewportLeft, position.Y + view.ViewportTop));
                    }
                    else
                    {
                        layer.RemoveAllAdornments();
                    }

                    break;
                }

            }
            Debug.WriteLine("");


   //         var text = lineY.Extent.GetText();
   //         Debug.WriteLine(text);
			//if (lineY == null) return;

			//if (regex.IsMatch(text))
			//{
			//	var match = regex.Match(text);
			//	ShowImageAsync(match.Value, new Point(position.X + view.ViewportLeft, position.Y + view.ViewportTop));
			//}
			//else
			//{
			//	layer.RemoveAllAdornments();
			//}
	    }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        private void CreateVisuals(ITextViewLine line)
        {
            //var regex = new Regex(@"(http|https)[-a-zA-Z0-9:_\+.~#?&//=]{2,256}\.[^@\ ][a-z]{2,12}\b(\/[-a-zA-Z0-9:%_\+.~#?&//=]*)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //var lineText = line.Extent.Snapshot.GetText(Span.FromBounds(line.Start, line.End));
            //if (regex.IsMatch(lineText))
            //{
            //    var currentLine = view.GetTextViewLineContainingBufferPosition(view.Caret.Position.BufferPosition);
            //    Trace.WriteLine(currentLine.Extent.GetText());

            //    var match = regex.Match(currentLine.Extent.GetText());
            //    ShowImageAsync(match.Value);

            //}

            IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;

            // Loop through each character, and place a box around any 'a'
            for (int charIndex = line.Start; charIndex < line.End; charIndex++)
            {
                if (this.view.TextSnapshot[charIndex] == 'a')
                {
                    SnapshotSpan span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(charIndex, charIndex + 1));
                    Geometry geometry = textViewLines.GetMarkerGeometry(span);
                    if (geometry != null)
                    {
                        var drawing = new GeometryDrawing(this.brush, this.pen, geometry);
                        drawing.Freeze();

                        var drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        var image = new Image
                        {
                            Source = drawingImage,
                        };

                        // Align the image with the top of the bounds of the text geometry
                        Canvas.SetLeft(image, geometry.Bounds.Left);
                        Canvas.SetTop(image, geometry.Bounds.Top);

                        this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                    }
                }
            }
        }

        protected async void ShowImageAsync(string source, Point point)
        {
	        img = new Image {Source = await LoadImageSourceAsync(source)};
			Canvas.SetTop(img, point.Y);
			Canvas.SetLeft(img, point.X);
	        layer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, img, null);
        }

        private async Task<ImageSource> LoadImageSourceAsync(string address)
        {
            ImageSource imgSource = null;

            try
            {
                var ms = new MemoryStream(await new WebClient().DownloadDataTaskAsync(new Uri(address)));
                var imageSourceConverter = new ImageSourceConverter();
                imgSource = (ImageSource)imageSourceConverter.ConvertFrom(ms);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return imgSource;
        }
    }
}

