﻿//------------------------------------------------------------------------------
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

namespace LinkViewerVSIX
{
    /// <summary>
    /// LinkViewerTextAdornment places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class LinkViewerTextAdornment
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private readonly Brush brush;

        /// <summary>
        /// Adornment pen.
        /// </summary>
        private readonly Pen pen;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkViewerTextAdornment"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public LinkViewerTextAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

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

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        /// <summary>
        /// Adds the scarlet box behind the 'a' characters within the given line
        /// </summary>
        /// <param name="line">Line to add the adornments</param>
        private void CreateVisuals(ITextViewLine line)
        {
            var regex = new Regex(@"(http|https)[-a-zA-Z0-9:_\+.~#?&//=]{2,256}\.[^@\ ][a-z]{2,12}\b(\/[-a-zA-Z0-9:%_\+.~#?&//=]*)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var lineText = line.Extent.Snapshot.GetText(Span.FromBounds(line.Start, line.End));
            if (regex.IsMatch(lineText))
            {
                var currentLine = view.GetTextViewLineContainingBufferPosition(view.Caret.Position.BufferPosition);
                Trace.WriteLine(currentLine.Extent.GetText());

                var match = regex.Match(currentLine.Extent.GetText());
                ShowImageAsync(match.Value);

            }

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

        protected async void ShowImageAsync(string source)
        {
            var img = new Image
            {
                Source = await LoadImageSourceAsync(source)
            };

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
