﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot
{
    public enum legendLocation
    {
        none,
        upperLeft,
        upperRight,
        upperCenter,
        middleLeft,
        middleRight,
        lowerLeft,
        lowerRight,
        lowerCenter,
    }

    public enum shadowDirection
    {
        none,
        upperLeft,
        upperRight,
        lowerLeft,
        lowerRight,
    }

    public class LegendTools
    {
        public static void DrawLegend(Settings settings)
        {
            if (settings.legendLocation == legendLocation.none)
                return;

            // note which plottables are to be included in the legend
            List<int> plottableIndexesNeedingLegend = new List<int>();
            for (int i = 0; i < settings.plottables.Count(); i++)
                if (settings.plottables[i].label != null)
                    plottableIndexesNeedingLegend.Add(i);
            plottableIndexesNeedingLegend.Reverse();

            // figure out where on the graph things should be
            int padding = 3;
            int stubWidth = 40 * (int)settings.legendFont.Size / 12;
            SizeF maxLabelSize = MaxLegendLabelSize(settings);
            float frameWidth = padding * 2 + maxLabelSize.Width + padding + stubWidth;
            float frameHeight = padding * 2 + maxLabelSize.Height * plottableIndexesNeedingLegend.Count();
            Size frameSize = new Size((int)frameWidth, (int)frameHeight);
            Point[] frameAndTextLocations = GetLocations(settings, padding * 2, frameSize, maxLabelSize.Width);
            Point frameLocation = frameAndTextLocations[0];
            Point textLocation = frameAndTextLocations[1];
            Point shadowLocation = frameAndTextLocations[2];
            Rectangle frameRect = new Rectangle(frameLocation, frameSize);
            Rectangle shadowRect = new Rectangle(shadowLocation, frameSize);

            // draw the legend background and shadow
            if (settings.legendShadowDirection != shadowDirection.none)
                settings.gfxData.FillRectangle(new SolidBrush(settings.legendShadowColor), shadowRect);
            settings.gfxData.FillRectangle(new SolidBrush(settings.legendBackColor), frameRect);
            settings.gfxData.DrawRectangle(new Pen(settings.legendFrameColor), frameRect);

            // draw the lines, markers, and text for each legend item
            foreach (int i in plottableIndexesNeedingLegend)
            {
                textLocation.Y -= (int)(maxLabelSize.Height);
                DrawLegendItemString(settings.plottables[i], settings, textLocation, padding, stubWidth, maxLabelSize.Height);
                DrawLegendItemLine(settings.plottables[i], settings, textLocation, padding, stubWidth, maxLabelSize.Height);
                DrawLegendItemMarker(settings.plottables[i], settings, textLocation, padding, stubWidth, maxLabelSize.Height);
            }
        }

        public static SizeF MaxLegendLabelSize(Settings settings)
        {
            SizeF maxLabelSize = new SizeF();

            foreach (Plottable plottable in settings.plottables)
            {
                if (plottable.label != null)
                {
                    SizeF labelSize = settings.gfxData.MeasureString(plottable.label, settings.legendFont);
                    if (labelSize.Width > maxLabelSize.Width)
                        maxLabelSize.Width = labelSize.Width;
                    if (labelSize.Height > maxLabelSize.Height)
                        maxLabelSize.Height = labelSize.Height;
                }
            }

            return maxLabelSize;
        }

        private static Point[] GetLocations(ScottPlot.Settings settings, int padding, Size frameSize, float legendFontMaxWidth)
        {
            Point frameLocation = new Point();
            Point textLocation = new Point();
            Point shadowLocation = new Point();

            int frameWidth = frameSize.Width;
            int frameHeight = frameSize.Height;
            switch (settings.legendLocation)
            {
                case (legendLocation.none):
                    return null;
                case (legendLocation.lowerRight):
                    frameLocation.X = (int)(settings.dataSize.Width - frameWidth - padding);
                    frameLocation.Y = (int)(settings.dataSize.Height - frameHeight - padding);
                    textLocation.X = (int)(settings.dataSize.Width - (legendFontMaxWidth + padding));
                    textLocation.Y = settings.dataSize.Height - padding * 2;
                    break;
                case (legendLocation.upperLeft):
                    frameLocation.X = (int)(padding);
                    frameLocation.Y = (int)(padding);
                    textLocation.X = (int)(frameWidth - legendFontMaxWidth + padding);
                    textLocation.Y = (int)(frameHeight);
                    break;
                case (legendLocation.lowerLeft):
                    frameLocation.X = (int)(padding);
                    frameLocation.Y = (int)(settings.dataSize.Height - frameHeight - padding);
                    textLocation.X = (int)(frameWidth - legendFontMaxWidth + padding);
                    textLocation.Y = settings.dataSize.Height - padding * 2;
                    break;
                case (legendLocation.upperRight):
                    frameLocation.X = (int)(settings.dataSize.Width - frameWidth - padding);
                    frameLocation.Y = (int)(padding);
                    textLocation.X = (int)(settings.dataSize.Width - (legendFontMaxWidth + padding));
                    textLocation.Y = (int)(frameHeight);
                    break;
                case (legendLocation.upperCenter):
                    frameLocation.X = (int)((settings.dataSize.Width) / 2 - frameWidth / 2 - padding * 5);
                    frameLocation.Y = (int)(padding);
                    textLocation.X = (int)(settings.dataSize.Width / 2 - legendFontMaxWidth / 2 + padding / 2);
                    textLocation.Y = (int)(frameHeight);
                    break;
                case (legendLocation.lowerCenter):
                    frameLocation.X = (int)((settings.dataSize.Width) / 2 - frameWidth / 2 - padding * 5);
                    frameLocation.Y = (int)(settings.dataSize.Height - frameHeight - padding);
                    textLocation.X = (int)(settings.dataSize.Width / 2 - legendFontMaxWidth / 2 + padding / 2);
                    textLocation.Y = settings.dataSize.Height - padding * 2;
                    break;
                case (legendLocation.middleLeft):
                    frameLocation.X = (int)(padding);
                    frameLocation.Y = (int)(settings.dataSize.Height / 2 - frameHeight / 2 - padding);
                    textLocation.X = (int)(frameWidth - legendFontMaxWidth + padding);
                    textLocation.Y = (int)(settings.dataSize.Height / 2 + frameHeight / 2 - padding * 2);
                    break;
                case (legendLocation.middleRight):
                    frameLocation.X = (int)(settings.dataSize.Width - frameWidth - padding);
                    frameLocation.Y = (int)(settings.dataSize.Height / 2 - frameHeight / 2 - padding);
                    textLocation.X = (int)(settings.dataSize.Width - (legendFontMaxWidth + padding));
                    textLocation.Y = (int)(settings.dataSize.Height / 2 + frameHeight / 2 - padding * 2);
                    break;
                default:
                    throw new NotImplementedException($"legend location {settings.legendLocation} is not supported");
            }

            switch (settings.legendShadowDirection)
            {
                case (shadowDirection.lowerRight):
                    shadowLocation.X = frameLocation.X + 2;
                    shadowLocation.Y = frameLocation.Y + 2;
                    break;
                case (shadowDirection.lowerLeft):
                    shadowLocation.X = frameLocation.X - 2;
                    shadowLocation.Y = frameLocation.Y + 2;
                    break;
                case (shadowDirection.upperRight):
                    shadowLocation.X = frameLocation.X + 2;
                    shadowLocation.Y = frameLocation.Y - 2;
                    break;
                case (shadowDirection.upperLeft):
                    shadowLocation.X = frameLocation.X - 2;
                    shadowLocation.Y = frameLocation.Y - 2;
                    break;
                default:
                    settings.legendShadowDirection = shadowDirection.none;
                    break;
            }

            textLocation.Y += padding;
            return new Point[] { frameLocation, textLocation, shadowLocation };
        }

        private static void DrawLegendItemString(Plottable plottable, Settings settings, Point textLocation, int padding, int stubWidth, float legendFontLineHeight)
        {
            Brush brushText = new SolidBrush(settings.legendFontColor);
            settings.gfxData.DrawString(plottable.label, settings.legendFont, brushText, textLocation);
        }

        private static void DrawLegendItemLine(Plottable plottable, Settings settings, Point textLocation, int padding, int stubWidth, float legendFontLineHeight)
        {
            Pen pen = new Pen(plottable.color, 1);

            if (plottable is PlottableAxSpan)
                pen.Width = 10;

            switch (plottable.lineStyle)
            {
                case LineStyle.Solid:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    break;
                case LineStyle.Dash:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    break;
                case LineStyle.DashDot:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                    break;
                case LineStyle.DashDotDot:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                    break;
                case LineStyle.Dot:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    break;
            }

            settings.gfxData.DrawLine(pen,
                textLocation.X - padding, textLocation.Y + legendFontLineHeight / 2,
                textLocation.X - padding - stubWidth, textLocation.Y + legendFontLineHeight / 2);
        }

        private static void DrawLegendItemMarker(Plottable plottable, Settings settings, Point textLocation, int padding, int stubWidth, float legendFontLineHeight)
        {
            Brush brushMarker = new SolidBrush(plottable.color);
            Pen penMarker = new Pen(plottable.color, 1);

            PointF corner1 = new PointF(textLocation.X - stubWidth + settings.legendFont.Size / 4, textLocation.Y + settings.legendFont.Size / 4 * padding);
            PointF center = new PointF
            {
                X = corner1.X + settings.legendFont.Size / 4,
                Y = corner1.Y + settings.legendFont.Size / 4
            };

            SizeF bounds = new SizeF(settings.legendFont.Size / 2, settings.legendFont.Size / 2);
            RectangleF rect = new RectangleF(corner1, bounds);

            switch (plottable.markerShape)
            {
                case MarkerShape.none:
                    //Nothing to do because the Drawline needs to be there for all cases, and it's already there
                    break;
                case MarkerShape.asterisk:
                    Font drawFontAsterisk = new Font("CourierNew", settings.legendFont.Size);
                    Point markerPositionAsterisk = new Point(textLocation.X - stubWidth, (int)(textLocation.Y + legendFontLineHeight / 4));
                    settings.gfxData.DrawString("*", drawFontAsterisk, brushMarker, markerPositionAsterisk);
                    break;
                case MarkerShape.cross:
                    Font drawFontCross = new Font("CourierNew", settings.legendFont.Size);
                    Point markerPositionCross = new Point(textLocation.X - stubWidth, (int)(textLocation.Y + legendFontLineHeight / 8));
                    settings.gfxData.DrawString("+", drawFontCross, brushMarker, markerPositionCross);
                    break;
                case MarkerShape.eks:
                    Font drawFontEks = new Font("CourierNew", settings.legendFont.Size);
                    Point markerPositionEks = new Point(textLocation.X - stubWidth, (int)(textLocation.Y));
                    settings.gfxData.DrawString("x", drawFontEks, brushMarker, markerPositionEks);
                    break;
                case MarkerShape.filledCircle:
                    settings.gfxData.FillEllipse(brushMarker, rect);
                    break;
                case MarkerShape.filledDiamond:
                    // Create points that define polygon.
                    PointF point1 = new PointF(center.X, center.Y + settings.legendFont.Size / 4);
                    PointF point2 = new PointF(center.X - settings.legendFont.Size / 4, center.Y);
                    PointF point3 = new PointF(center.X, center.Y - settings.legendFont.Size / 4);
                    PointF point4 = new PointF(center.X + settings.legendFont.Size / 4, center.Y);

                    PointF[] curvePoints = { point1, point2, point3, point4 };

                    //Draw polygon to screen
                    settings.gfxData.FillPolygon(brushMarker, curvePoints);
                    break;
                case MarkerShape.filledSquare:
                    settings.gfxData.FillRectangle(brushMarker, rect);
                    break;
                case MarkerShape.hashTag:
                    Font drawFontHashtag = new Font("CourierNew", settings.legendFont.Size);
                    Point markerPositionHashTag = new Point(textLocation.X - stubWidth, (int)(textLocation.Y + legendFontLineHeight / 8));
                    settings.gfxData.DrawString("#", drawFontHashtag, brushMarker, markerPositionHashTag);
                    break;
                case MarkerShape.openCircle:
                    settings.gfxData.DrawEllipse(penMarker, rect);
                    break;
                case MarkerShape.openDiamond:
                    // Create points that define polygon.
                    PointF point5 = new PointF(center.X, center.Y + settings.legendFont.Size / 4);
                    PointF point6 = new PointF(center.X - settings.legendFont.Size / 4, center.Y);
                    PointF point7 = new PointF(center.X, center.Y - settings.legendFont.Size / 4);
                    PointF point8 = new PointF(center.X + settings.legendFont.Size / 4, center.Y);

                    PointF[] curvePoints2 = { point5, point6, point7, point8 };

                    //Draw polygon to screen
                    settings.gfxData.DrawPolygon(penMarker, curvePoints2);
                    break;
                case MarkerShape.openSquare:
                    settings.gfxData.DrawRectangle(penMarker, corner1.X, corner1.Y, settings.legendFont.Size / 2, settings.legendFont.Size / 2);
                    break;
                case MarkerShape.triDown:
                    // Create points that define polygon.
                    PointF point14 = new PointF(center.X, center.Y + settings.legendFont.Size / 2);
                    PointF point15 = new PointF(center.X, center.Y);
                    PointF point16 = new PointF(center.X - settings.legendFont.Size / 2 * (float)0.866, center.Y - settings.legendFont.Size / 2 * (float)0.5);
                    PointF point17 = new PointF(center.X, center.Y);
                    PointF point18 = new PointF(center.X + settings.legendFont.Size / 2 * (float)0.866, center.Y - settings.legendFont.Size / 2 * (float)0.5);

                    PointF[] curvePoints4 = { point17, point14, point15, point16, point17, point18 };

                    //Draw polygon to screen
                    settings.gfxData.DrawPolygon(penMarker, curvePoints4);

                    break;

                case MarkerShape.triUp:
                    // Create points that define polygon.
                    PointF point9 = new PointF(center.X, center.Y - settings.legendFont.Size / 2);
                    PointF point10 = new PointF(center.X, center.Y);
                    PointF point11 = new PointF(center.X - settings.legendFont.Size / 2 * (float)0.866, center.Y + settings.legendFont.Size / 2 * (float)0.5);
                    PointF point12 = new PointF(center.X, center.Y);
                    PointF point13 = new PointF(center.X + settings.legendFont.Size / 2 * (float)0.866, center.Y + settings.legendFont.Size / 2 * (float)0.5);

                    PointF[] curvePoints3 = { point12, point9, point10, point11, point12, point13 };
                    //Draw polygon to screen
                    settings.gfxData.DrawPolygon(penMarker, curvePoints3);
                    break;

                case MarkerShape.verticalBar:
                    Font drawFontVertical = new Font("CourierNew", settings.legendFont.Size);
                    Point markerPositionVertical = new Point(textLocation.X - stubWidth, (int)(textLocation.Y));
                    settings.gfxData.DrawString("|", drawFontVertical, brushMarker, markerPositionVertical);
                    break;
            }
        }

    }
}
