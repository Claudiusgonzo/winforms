﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Text;
    using System.Collections;
    using System.Windows.Forms;
    using System.Windows.Forms.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;

    using Microsoft.Win32;

    /// <summary>
    /// <para>Represents the table drawn by the <see cref='System.Windows.Forms.DataGrid'/> control at run time.</para>
    /// </summary>
    [
    ToolboxItem(false),
    DesignTimeVisible(false),
    //DefaultProperty("GridTableName")
    ]
    public class DataGridTableStyle : Component, IDataGridEditingService
    {

        // internal for DataGridColumn accessibility...
        //
        internal DataGrid dataGrid = null;

        // relationship UI
        private int relationshipHeight = 0;
        internal const int relationshipSpacing = 1;
        private Rectangle relationshipRect = Rectangle.Empty;
        private int focusedRelation = -1;
        private int focusedTextWidth;

        // will contain a list of relationships that this table has
        private readonly ArrayList relationsList = new ArrayList(2);

        // the name of the table
        private string mappingName = string.Empty;
        private readonly GridColumnStylesCollection gridColumns = null;
        private bool readOnly = false;
        private readonly bool isDefaultTableStyle = false;

        private static readonly object EventAllowSorting = new object();
        private static readonly object EventGridLineColor = new object();
        private static readonly object EventGridLineStyle = new object();
        private static readonly object EventHeaderBackColor = new object();
        private static readonly object EventHeaderForeColor = new object();
        private static readonly object EventHeaderFont = new object();
        private static readonly object EventLinkColor = new object();
        private static readonly object EventLinkHoverColor = new object();
        private static readonly object EventPreferredColumnWidth = new object();
        private static readonly object EventPreferredRowHeight = new object();
        private static readonly object EventColumnHeadersVisible = new object();
        private static readonly object EventRowHeaderWidth = new object();
        private static readonly object EventSelectionBackColor = new object();
        private static readonly object EventSelectionForeColor = new object();
        private static readonly object EventMappingName = new object();
        private static readonly object EventAlternatingBackColor = new object();
        private static readonly object EventBackColor = new object();
        private static readonly object EventForeColor = new object();
        private static readonly object EventReadOnly = new object();
        private static readonly object EventRowHeadersVisible = new object();

        // add a bunch of properties, taken from the dataGrid
        //

        // default values
        //
        private const bool defaultAllowSorting = true;
        private const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;
        private const int defaultPreferredColumnWidth = 75;
        private const int defaultRowHeaderWidth = 35;
        internal static readonly Font defaultFont = Control.DefaultFont;
        internal static readonly int defaultFontHeight = defaultFont.Height;


        // the actual place holders for properties
        //
        private bool allowSorting = defaultAllowSorting;
        private SolidBrush alternatingBackBrush = DefaultAlternatingBackBrush;
        private SolidBrush backBrush = DefaultBackBrush;
        private SolidBrush foreBrush = DefaultForeBrush;
        private SolidBrush gridLineBrush = DefaultGridLineBrush;
        private DataGridLineStyle gridLineStyle = defaultGridLineStyle;
        internal SolidBrush headerBackBrush = DefaultHeaderBackBrush;
        internal Font headerFont = null; // this is ambient property to Font value.
        internal SolidBrush headerForeBrush = DefaultHeaderForeBrush;
        internal Pen headerForePen = DefaultHeaderForePen;
        private SolidBrush linkBrush = DefaultLinkBrush;
        internal int preferredColumnWidth = defaultPreferredColumnWidth;
        private int prefferedRowHeight = defaultFontHeight + 3;
        private SolidBrush selectionBackBrush = DefaultSelectionBackBrush;
        private SolidBrush selectionForeBrush = DefaultSelectionForeBrush;
        private int rowHeaderWidth = defaultRowHeaderWidth;
        private bool rowHeadersVisible = true;
        private bool columnHeadersVisible = true;

        // the dataGrid would need to know when the ColumnHeaderVisible, RowHeadersVisible, RowHeaderWidth
        // and preferredColumnWidth, preferredRowHeight properties are changed in the current dataGridTableStyle
        // also: for GridLineStyle, GridLineColor, ForeColor, BackColor, HeaderBackColor, HeaderFont, HeaderForeColor
        // LinkColor, LinkHoverColor
        //

        [
        SRCategory(nameof(SR.CatBehavior)),
        DefaultValue(defaultAllowSorting),
        SRDescription(nameof(SR.DataGridAllowSortingDescr))
        ]
        public bool AllowSorting
        {
            get
            {
                return allowSorting;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "AllowSorting"));
                }

                if (allowSorting != value)
                {
                    allowSorting = value;
                    OnAllowSortingChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// <para>[To be  supplied]</para>
        /// </summary>
        public event EventHandler AllowSortingChanged
        {
            add => Events.AddHandler(EventAllowSorting, value);
            remove => Events.RemoveHandler(EventAllowSorting, value);
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridAlternatingBackColorDescr))
        ]
        public Color AlternatingBackColor
        {
            get
            {
                return alternatingBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "AlternatingBackColor"));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentAlternatingBackColorNotAllowed);
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor,
                                                              "AlternatingBackColor"));
                }
                if (!alternatingBackBrush.Color.Equals(value))
                {
                    alternatingBackBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnAlternatingBackColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler AlternatingBackColorChanged
        {
            add => Events.AddHandler(EventAlternatingBackColor, value);
            remove => Events.RemoveHandler(EventAlternatingBackColor, value);
        }
        public void ResetAlternatingBackColor()
        {
            if (ShouldSerializeAlternatingBackColor())
            {
                AlternatingBackColor = DefaultAlternatingBackBrush.Color;
                InvalidateInside();
            }
        }

        protected virtual bool ShouldSerializeAlternatingBackColor()
        {
            return !AlternatingBackBrush.Equals(DefaultAlternatingBackBrush);
        }

        internal SolidBrush AlternatingBackBrush
        {
            get
            {
                return alternatingBackBrush;
            }
        }

        protected bool ShouldSerializeBackColor()
        {
            return !System.Windows.Forms.DataGridTableStyle.DefaultBackBrush.Equals(backBrush);
        }

        protected bool ShouldSerializeForeColor()
        {
            return !System.Windows.Forms.DataGridTableStyle.DefaultForeBrush.Equals(foreBrush);
        }

        internal SolidBrush BackBrush
        {
            get
            {
                return backBrush;
            }
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.ControlBackColorDescr))
        ]
        public Color BackColor
        {
            get
            {
                return backBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "BackColor"));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentBackColorNotAllowed);
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor,
                                                              "BackColor"));
                }
                if (!backBrush.Color.Equals(value))
                {
                    backBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler BackColorChanged
        {
            add => Events.AddHandler(EventBackColor, value);
            remove => Events.RemoveHandler(EventBackColor, value);
        }

        public void ResetBackColor()
        {
            if (!backBrush.Equals(DefaultBackBrush))
            {
                BackColor = DefaultBackBrush.Color;
            }
        }

        internal int BorderWidth
        {
            get
            {
                DataGrid dataGrid = DataGrid;
                if (dataGrid == null)
                {
                    return 0;
                }
                // if the user set the GridLineStyle property on the dataGrid.
                // then use the value of that property
                DataGridLineStyle gridStyle;
                int gridLineWidth;
                if (IsDefault)
                {
                    gridStyle = DataGrid.GridLineStyle;
                    gridLineWidth = DataGrid.GridLineWidth;
                }
                else
                {
                    gridStyle = GridLineStyle;
                    gridLineWidth = GridLineWidth;
                }

                if (gridStyle == DataGridLineStyle.None)
                {
                    return 0;
                }

                return gridLineWidth;
            }
        }

        internal static SolidBrush DefaultAlternatingBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Window;
            }
        }
        internal static SolidBrush DefaultBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Window;
            }
        }
        internal static SolidBrush DefaultForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.WindowText;
            }
        }
        private static SolidBrush DefaultGridLineBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Control;
            }
        }
        private static SolidBrush DefaultHeaderBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Control;
            }
        }
        private static SolidBrush DefaultHeaderForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ControlText;
            }
        }
        private static Pen DefaultHeaderForePen
        {
            get
            {
                return new Pen(SystemColors.ControlText);
            }
        }
        private static SolidBrush DefaultLinkBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.HotTrack;
            }
        }
        private static SolidBrush DefaultSelectionBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ActiveCaption;
            }
        }
        private static SolidBrush DefaultSelectionForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ActiveCaptionText;
            }
        }

        internal int FocusedRelation
        {
            get
            {
                return focusedRelation;
            }
            set
            {
                if (focusedRelation != value)
                {
                    focusedRelation = value;
                    if (focusedRelation == -1)
                    {
                        focusedTextWidth = 0;
                    }
                    else
                    {
                        Graphics g = DataGrid.CreateGraphicsInternal();
                        focusedTextWidth = (int)Math.Ceiling(g.MeasureString(((string)RelationsList[focusedRelation]), DataGrid.LinkFont).Width);
                        g.Dispose();
                    }
                }
            }
        }

        internal int FocusedTextWidth
        {
            get
            {
                return focusedTextWidth;
            }
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.ControlForeColorDescr))
        ]
        public Color ForeColor
        {
            get
            {
                return foreBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "ForeColor"));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor,
                                                              "BackColor"));
                }
                if (!foreBrush.Color.Equals(value))
                {
                    foreBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnForeColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler ForeColorChanged
        {
            add => Events.AddHandler(EventForeColor, value);
            remove => Events.RemoveHandler(EventForeColor, value);
        }

        internal SolidBrush ForeBrush
        {
            get
            {
                return foreBrush;
            }
        }

        public void ResetForeColor()
        {
            if (!foreBrush.Equals(DefaultForeBrush))
            {
                ForeColor = DefaultForeBrush.Color;
            }
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridGridLineColorDescr))
        ]
        public Color GridLineColor
        {
            get
            {
                return gridLineBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "GridLineColor"));
                }

                if (gridLineBrush.Color != value)
                {
                    if (value.IsEmpty)
                    {
                        throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "GridLineColor"));
                    }

                    gridLineBrush = new SolidBrush(value);

                    // Invalidate(layout.Data);
                    OnGridLineColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler GridLineColorChanged
        {
            add => Events.AddHandler(EventGridLineColor, value);
            remove => Events.RemoveHandler(EventGridLineColor, value);
        }

        protected virtual bool ShouldSerializeGridLineColor()
        {
            return !GridLineBrush.Equals(DefaultGridLineBrush);
        }

        public void ResetGridLineColor()
        {
            if (ShouldSerializeGridLineColor())
            {
                GridLineColor = DefaultGridLineBrush.Color;
            }
        }

        internal SolidBrush GridLineBrush
        {
            get
            {
                return gridLineBrush;
            }
        }

        internal int GridLineWidth
        {
            get
            {
                Debug.Assert(GridLineStyle == DataGridLineStyle.Solid || GridLineStyle == DataGridLineStyle.None, "are there any other styles?");
                return GridLineStyle == DataGridLineStyle.Solid ? 1 : 0;
            }
        }

        [
         SRCategory(nameof(SR.CatAppearance)),
         DefaultValue(defaultGridLineStyle),
         SRDescription(nameof(SR.DataGridGridLineStyleDescr))
        ]
        public DataGridLineStyle GridLineStyle
        {
            get
            {
                return gridLineStyle;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "GridLineStyle"));
                }

                //valid values are 0x0 to 0x1. 
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)DataGridLineStyle.None, (int)DataGridLineStyle.Solid))
                {
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataGridLineStyle));
                }
                if (gridLineStyle != value)
                {
                    gridLineStyle = value;
                    // Invalidate(layout.Data);
                    OnGridLineStyleChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler GridLineStyleChanged
        {
            add => Events.AddHandler(EventGridLineStyle, value);
            remove => Events.RemoveHandler(EventGridLineStyle, value);
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridHeaderBackColorDescr))
        ]
        public Color HeaderBackColor
        {
            get
            {
                return headerBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "HeaderBackColor"));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentHeaderBackColorNotAllowed);
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "HeaderBackColor"));
                }

                if (!value.Equals(headerBackBrush.Color))
                {
                    headerBackBrush = new SolidBrush(value);

                    /*
                    if (layout.RowHeadersVisible)
                        Invalidate(layout.RowHeaders);
                    if (layout.ColumnHeadersVisible)
                        Invalidate(layout.ColumnHeaders);
                    Invalidate(layout.TopLeftHeader);
                    */
                    OnHeaderBackColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler HeaderBackColorChanged
        {
            add => Events.AddHandler(EventHeaderBackColor, value);
            remove => Events.RemoveHandler(EventHeaderBackColor, value);
        }

        internal SolidBrush HeaderBackBrush
        {
            get
            {
                return headerBackBrush;
            }
        }

        protected virtual bool ShouldSerializeHeaderBackColor()
        {
            return !HeaderBackBrush.Equals(DefaultHeaderBackBrush);
        }

        public void ResetHeaderBackColor()
        {
            if (ShouldSerializeHeaderBackColor())
            {
                HeaderBackColor = DefaultHeaderBackBrush.Color;
            }
        }

        [
         SRCategory(nameof(SR.CatAppearance)),
         Localizable(true),
         AmbientValue(null),
         SRDescription(nameof(SR.DataGridHeaderFontDescr))
        ]
        public Font HeaderFont
        {
            get
            {
                return (headerFont ?? (DataGrid == null ? Control.DefaultFont : DataGrid.Font));
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "HeaderFont"));
                }

                if (value == null && headerFont != null || (value != null && !value.Equals(headerFont)))
                {
                    headerFont = value;
                    /*
                    RecalculateFonts();
                    PerformLayout();
                    Invalidate(layout.Inside);
                    */
                    OnHeaderFontChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler HeaderFontChanged
        {
            add => Events.AddHandler(EventHeaderFont, value);
            remove => Events.RemoveHandler(EventHeaderFont, value);
        }

        private bool ShouldSerializeHeaderFont()
        {
            return (headerFont != null);
        }

        public void ResetHeaderFont()
        {
            if (headerFont != null)
            {
                headerFont = null;
                /*
                RecalculateFonts();
                PerformLayout();
                Invalidate(layout.Inside);
                */
                OnHeaderFontChanged(EventArgs.Empty);
            }
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridHeaderForeColorDescr))
        ]
        public Color HeaderForeColor
        {
            get
            {
                return headerForePen.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "HeaderForeColor"));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "HeaderForeColor"));
                }

                if (!value.Equals(headerForePen.Color))
                {
                    headerForePen = new Pen(value);
                    headerForeBrush = new SolidBrush(value);

                    /*
                    if (layout.RowHeadersVisible)
                        Invalidate(layout.RowHeaders);
                    if (layout.ColumnHeadersVisible)
                        Invalidate(layout.ColumnHeaders);
                    Invalidate(layout.TopLeftHeader);
                    */
                    OnHeaderForeColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler HeaderForeColorChanged
        {
            add => Events.AddHandler(EventHeaderForeColor, value);
            remove => Events.RemoveHandler(EventHeaderForeColor, value);
        }

        protected virtual bool ShouldSerializeHeaderForeColor()
        {
            return !HeaderForePen.Equals(DefaultHeaderForePen);
        }

        public void ResetHeaderForeColor()
        {
            if (ShouldSerializeHeaderForeColor())
            {
                HeaderForeColor = DefaultHeaderForeBrush.Color;
            }
        }

        internal SolidBrush HeaderForeBrush
        {
            get
            {
                return headerForeBrush;
            }
        }

        internal Pen HeaderForePen
        {
            get
            {
                return headerForePen;
            }
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridLinkColorDescr))
        ]
        public Color LinkColor
        {
            get
            {
                return linkBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "LinkColor"));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "LinkColor"));
                }

                if (!linkBrush.Color.Equals(value))
                {
                    linkBrush = new SolidBrush(value);
                    // Invalidate(layout.Data);
                    OnLinkColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler LinkColorChanged
        {
            add => Events.AddHandler(EventLinkColor, value);
            remove => Events.RemoveHandler(EventLinkColor, value);
        }

        protected virtual bool ShouldSerializeLinkColor()
        {
            return !LinkBrush.Equals(DefaultLinkBrush);
        }

        public void ResetLinkColor()
        {
            if (ShouldSerializeLinkColor())
            {
                LinkColor = DefaultLinkBrush.Color;
            }
        }

        internal Brush LinkBrush
        {
            get
            {
                return linkBrush;
            }
        }

        [
         SRDescription(nameof(SR.DataGridLinkHoverColorDescr)),
         SRCategory(nameof(SR.CatColors)),
         Browsable(false),
         EditorBrowsable(EditorBrowsableState.Never)
        ]
        public Color LinkHoverColor
        {
            get
            {
                return LinkColor;
            }
            [SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters")]
            set
            {
            }
        }

        public event EventHandler LinkHoverColorChanged
        {
            add => Events.AddHandler(EventLinkHoverColor, value);
            remove => Events.RemoveHandler(EventLinkHoverColor, value);
        }

        protected virtual bool ShouldSerializeLinkHoverColor()
        {
            return false;
            // return !LinkHoverBrush.Equals(defaultLinkHoverBrush);
        }

        internal Rectangle RelationshipRect
        {
            get
            {
                if (relationshipRect.IsEmpty)
                {
                    ComputeRelationshipRect();
                }
                return relationshipRect;
            }
        }

        private Rectangle ComputeRelationshipRect()
        {
            if (relationshipRect.IsEmpty && DataGrid.AllowNavigation)
            {
                Debug.WriteLineIf(CompModSwitches.DGRelationShpRowLayout.TraceVerbose, "GetRelationshipRect grinding away");
                Graphics g = DataGrid.CreateGraphicsInternal();
                relationshipRect = new Rectangle
                {
                    X = 0 //indentWidth;
                };
                // relationshipRect.Y = base.Height - BorderWidth;

                // Determine the width of the widest relationship name
                int longestRelationship = 0;
                for (int r = 0; r < RelationsList.Count; ++r)
                {
                    int rwidth = (int)Math.Ceiling(g.MeasureString(((string)RelationsList[r]), DataGrid.LinkFont).Width)
;
                    if (rwidth > longestRelationship)
                    {
                        longestRelationship = rwidth;
                    }
                }

                g.Dispose();

                relationshipRect.Width = longestRelationship + 5;
                relationshipRect.Width += 2; // relationshipRect border;
                relationshipRect.Height = BorderWidth + relationshipHeight * RelationsList.Count;
                relationshipRect.Height += 2; // relationship border
                if (RelationsList.Count > 0)
                {
                    relationshipRect.Height += 2 * relationshipSpacing;
                }
            }
            return relationshipRect;
        }

        internal void ResetRelationsUI()
        {
            relationshipRect = Rectangle.Empty;
            focusedRelation = -1;
            relationshipHeight = dataGrid.LinkFontHeight + relationshipSpacing;
        }

        internal int RelationshipHeight
        {
            get
            {
                return relationshipHeight;
            }
        }

        public void ResetLinkHoverColor()
        {
            /*if (ShouldSerializeLinkHoverColor())
                LinkHoverColor = defaultLinkHoverBrush.Color;*/
        }

        [
         DefaultValue(defaultPreferredColumnWidth),
         SRCategory(nameof(SR.CatLayout)),
         Localizable(true),
         SRDescription(nameof(SR.DataGridPreferredColumnWidthDescr)),
         TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))
        ]
        public int PreferredColumnWidth
        {
            get
            {
                return preferredColumnWidth;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "PreferredColumnWidth"));
                }

                if (value < 0)
                {
                    throw new ArgumentException(SR.DataGridColumnWidth, "PreferredColumnWidth");
                }

                if (preferredColumnWidth != value)
                {
                    preferredColumnWidth = value;

                    /*
                    // reset the dataGridRows
                    SetDataGridRows(null, this.DataGridRowsLength);
                    // layout the horizontal scroll bar
                    PerformLayout();
                    // invalidate everything
                    Invalidate();
                    */

                    OnPreferredColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler PreferredColumnWidthChanged
        {
            add => Events.AddHandler(EventPreferredColumnWidth, value);
            remove => Events.RemoveHandler(EventPreferredColumnWidth, value);
        }

        [
         SRCategory(nameof(SR.CatLayout)),
         Localizable(true),
         SRDescription(nameof(SR.DataGridPreferredRowHeightDescr))
        ]
        public int PreferredRowHeight
        {
            get
            {
                return prefferedRowHeight;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "PrefferedRowHeight"));
                }

                if (value < 0)
                {
                    throw new ArgumentException(SR.DataGridRowRowHeight);
                }

                prefferedRowHeight = value;

                /*
                bool needToRedraw = false;
                DataGridRow[] rows = DataGridRows;

                for (int i = 0; i < DataGridRowsLength; i++)
                {
                    if (rows[i].Height != value) needToRedraw = false;
                    rows[i].Height = value;
                }

                // if all rows' height was equal to "value" before setting it, then
                // there is no need to redraw.
                if (!needToRedraw)
                    return;

                // need to layout the scroll bars:
                PerformLayout();

                // invalidate the entire area...
                Rectangle rightArea = Rectangle.Union(layout.RowHeaders, layout.Data);
                Invalidate(rightArea);
                */
                OnPreferredRowHeightChanged(EventArgs.Empty);
            }
        }

        public event EventHandler PreferredRowHeightChanged
        {
            add => Events.AddHandler(EventPreferredRowHeight, value);
            remove => Events.RemoveHandler(EventPreferredRowHeight, value);
        }

        private void ResetPreferredRowHeight()
        {
            PreferredRowHeight = defaultFontHeight + 3;
        }

        protected bool ShouldSerializePreferredRowHeight()
        {
            return prefferedRowHeight != defaultFontHeight + 3;
        }

        [
         SRCategory(nameof(SR.CatDisplay)),
         DefaultValue(true),
         SRDescription(nameof(SR.DataGridColumnHeadersVisibleDescr))
        ]
        public bool ColumnHeadersVisible
        {
            get
            {
                return columnHeadersVisible;
            }
            set
            {
                if (columnHeadersVisible != value)
                {
                    columnHeadersVisible = value;
                    /*
                    PerformLayout();
                    InvalidateInside();
                    */
                    OnColumnHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler ColumnHeadersVisibleChanged
        {
            add => Events.AddHandler(EventColumnHeadersVisible, value);
            remove => Events.RemoveHandler(EventColumnHeadersVisible, value);
        }

        [
         SRCategory(nameof(SR.CatDisplay)),
         DefaultValue(true),
         SRDescription(nameof(SR.DataGridRowHeadersVisibleDescr))
        ]
        public bool RowHeadersVisible
        {
            get
            {
                return rowHeadersVisible;
            }
            set
            {
                if (rowHeadersVisible != value)
                {
                    rowHeadersVisible = value;
                    /*
                    PerformLayout();
                    InvalidateInside();
                    */
                    OnRowHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler RowHeadersVisibleChanged
        {
            add => Events.AddHandler(EventRowHeadersVisible, value);
            remove => Events.RemoveHandler(EventRowHeadersVisible, value);
        }

        [
         SRCategory(nameof(SR.CatLayout)),
         DefaultValue(defaultRowHeaderWidth),
         Localizable(true),
         SRDescription(nameof(SR.DataGridRowHeaderWidthDescr))
        ]
        public int RowHeaderWidth
        {
            get
            {
                return rowHeaderWidth;
            }
            set
            {
                if (DataGrid != null)
                {
                    value = Math.Max(DataGrid.MinimumRowHeaderWidth(), value);
                }

                if (rowHeaderWidth != value)
                {
                    rowHeaderWidth = value;
                    /*
                    if (layout.RowHeadersVisible)
                    {
                        PerformLayout();
                        InvalidateInside();
                    }
                    */
                    OnRowHeaderWidthChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler RowHeaderWidthChanged
        {
            add => Events.AddHandler(EventRowHeaderWidth, value);
            remove => Events.RemoveHandler(EventRowHeaderWidth, value);
        }

        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridSelectionBackColorDescr))
        ]
        public Color SelectionBackColor
        {
            get
            {
                return selectionBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "SelectionBackColor"));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentSelectionBackColorNotAllowed);
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "SelectionBackColor"));
                }

                if (!value.Equals(selectionBackBrush.Color))
                {
                    selectionBackBrush = new SolidBrush(value);

                    InvalidateInside();

                    OnSelectionBackColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectionBackColorChanged
        {
            add => Events.AddHandler(EventSelectionBackColor, value);
            remove => Events.RemoveHandler(EventSelectionBackColor, value);
        }

        internal SolidBrush SelectionBackBrush
        {
            get
            {
                return selectionBackBrush;
            }
        }

        internal SolidBrush SelectionForeBrush
        {
            get
            {
                return selectionForeBrush;
            }
        }

        protected bool ShouldSerializeSelectionBackColor()
        {
            return !DefaultSelectionBackBrush.Equals(selectionBackBrush);
        }

        public void ResetSelectionBackColor()
        {
            if (ShouldSerializeSelectionBackColor())
            {
                SelectionBackColor = DefaultSelectionBackBrush.Color;
            }
        }

        [
         Description("The foreground color for the current data grid row"),
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridSelectionForeColorDescr))
        ]
        public Color SelectionForeColor
        {
            get
            {
                return selectionForeBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, "SelectionForeColor"));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "SelectionForeColor"));
                }

                if (!value.Equals(selectionForeBrush.Color))
                {
                    selectionForeBrush = new SolidBrush(value);

                    InvalidateInside();

                    OnSelectionForeColorChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectionForeColorChanged
        {
            add => Events.AddHandler(EventSelectionForeColor, value);
            remove => Events.RemoveHandler(EventSelectionForeColor, value);
        }

        protected virtual bool ShouldSerializeSelectionForeColor()
        {
            return !SelectionForeBrush.Equals(DefaultSelectionForeBrush);
        }

        public void ResetSelectionForeColor()
        {
            if (ShouldSerializeSelectionForeColor())
            {
                SelectionForeColor = DefaultSelectionForeBrush.Color;
            }
        }

        // will need this function from the dataGrid
        //
        private void InvalidateInside()
        {
            if (DataGrid != null)
            {
                DataGrid.InvalidateInside();
            }
        }

        [
            SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")   // This has already shipped so we can't change it.
        ]
        public static readonly DataGridTableStyle DefaultTableStyle = new DataGridTableStyle(true);


        /// <summary>
        /// <para>Initializes a new instance of the <see cref='System.Windows.Forms.DataGridTableStyle'/> class.</para>
        /// </summary>
        public DataGridTableStyle(bool isDefaultTableStyle)
        {
            gridColumns = new GridColumnStylesCollection(this, isDefaultTableStyle);
            gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
            this.isDefaultTableStyle = isDefaultTableStyle;
        }

        public DataGridTableStyle() : this(false)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref='System.Windows.Forms.DataGridTableStyle'/> class with the specified
        /// <see cref='System.Windows.Forms.CurrencyManager'/>.</para>
        /// </summary>
        [
            SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")  // If the constructor does not set the GridColumnStyles
                                                                                                    // it would be a breaking change.
        ]
        public DataGridTableStyle(CurrencyManager listManager) : this()
        {
            Debug.Assert(listManager != null, "the DataGridTabel cannot use a null listManager");
            mappingName = listManager.GetListName();
            // set up the Relations and the columns
            SetGridColumnStylesCollection(listManager);
        }

        internal void SetRelationsList(CurrencyManager listManager)
        {
            PropertyDescriptorCollection propCollection = listManager.GetItemProperties();
            Debug.Assert(!IsDefault, "the grid can set the relations only on a table that was manually added by the user");
            int propCount = propCollection.Count;
            if (relationsList.Count > 0)
            {
                relationsList.Clear();
            }

            for (int i = 0; i < propCount; i++)
            {
                PropertyDescriptor prop = propCollection[i];
                Debug.Assert(prop != null, "prop is null: how that happened?");
                if (PropertyDescriptorIsARelation(prop))
                {
                    // relation
                    relationsList.Add(prop.Name);
                }
            }
        }

        internal void SetGridColumnStylesCollection(CurrencyManager listManager)
        {
            // when we are setting the gridColumnStyles, do not handle any gridColumnCollectionChanged events
            gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

            PropertyDescriptorCollection propCollection = listManager.GetItemProperties();

            // we need to clear the relations list
            if (relationsList.Count > 0)
            {
                relationsList.Clear();
            }

            Debug.Assert(propCollection != null, "propCollection is null: how that happened?");
            int propCount = propCollection.Count;
            for (int i = 0; i < propCount; i++)
            {
                PropertyDescriptor prop = propCollection[i];
                Debug.Assert(prop != null, "prop is null: how that happened?");
                // do not take into account the properties that are browsable.
                if (!prop.IsBrowsable)
                {
                    continue;
                }

                if (PropertyDescriptorIsARelation(prop))
                {
                    // relation
                    relationsList.Add(prop.Name);
                }
                else
                {
                    // column
                    DataGridColumnStyle col = CreateGridColumn(prop, isDefaultTableStyle);
                    if (isDefaultTableStyle)
                    {
                        gridColumns.AddDefaultColumn(col);
                    }
                    else
                    {
                        col.MappingName = prop.Name;
                        col.HeaderText = prop.Name;
                        gridColumns.Add(col);
                    }
                }
            }

            // now we are able to handle the collectionChangeEvents
            gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
        }

        private static bool PropertyDescriptorIsARelation(PropertyDescriptor prop)
        {
            return typeof(IList).IsAssignableFrom(prop.PropertyType) && !typeof(Array).IsAssignableFrom(prop.PropertyType);
        }

        internal protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop)
        {
            return CreateGridColumn(prop, false);
        }

        internal protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop, bool isDefault)
        {
            DataGridColumnStyle ret = null;
            Type dataType = prop.PropertyType;

            if (dataType.Equals(typeof(bool)))
            {
                ret = new DataGridBoolColumn(prop, isDefault);
            }
            else if (dataType.Equals(typeof(string)))
            {
                ret = new DataGridTextBoxColumn(prop, isDefault);
            }
            else if (dataType.Equals(typeof(DateTime)))
            {
                ret = new DataGridTextBoxColumn(prop, "d", isDefault);
            }
            else if (dataType.Equals(typeof(short)) ||
                     dataType.Equals(typeof(int)) ||
                     dataType.Equals(typeof(long)) ||
                     dataType.Equals(typeof(ushort)) ||
                     dataType.Equals(typeof(uint)) ||
                     dataType.Equals(typeof(ulong)) ||
                     dataType.Equals(typeof(decimal)) ||
                     dataType.Equals(typeof(double)) ||
                     dataType.Equals(typeof(float)) ||
                     dataType.Equals(typeof(byte)) ||
                     dataType.Equals(typeof(sbyte)))
            {
                ret = new DataGridTextBoxColumn(prop, "G", isDefault);
            }
            else
            {
                ret = new DataGridTextBoxColumn(prop, isDefault);
            }
            return ret;
        }

        internal void ResetRelationsList()
        {
            if (isDefaultTableStyle)
            {
                relationsList.Clear();
            }
        }

        // =------------------------------------------------------------------
        // =        Properties
        // =------------------------------------------------------------------

        /// <summary>
        ///    <para>Gets the name of this grid table.</para>
        /// </summary>
        [Editor("System.Windows.Forms.Design.DataGridTableStyleMappingNameEditor, " + AssemblyRef.SystemDesign, typeof(System.Drawing.Design.UITypeEditor)), DefaultValue("")]
        public string MappingName
        {
            get
            {
                return mappingName;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (value.Equals(mappingName))
                {
                    return;
                }

                string originalMappingName = MappingName;
                mappingName = value;

                // this could throw
                try
                {
                    if (DataGrid != null)
                    {
                        DataGrid.TableStyles.CheckForMappingNameDuplicates(this);
                    }
                }
                catch
                {
                    mappingName = originalMappingName;
                    throw;
                }
                OnMappingNameChanged(EventArgs.Empty);
            }
        }

        public event EventHandler MappingNameChanged
        {
            add => Events.AddHandler(EventMappingName, value);
            remove => Events.RemoveHandler(EventMappingName, value);
        }

        /// <summary>
        ///    <para>Gets the
        ///       list of relation objects for the grid table.</para>
        /// </summary>
        internal ArrayList RelationsList
        {
            get
            {
                return relationsList;
            }
        }

        /// <summary>
        ///    <para>Gets the collection of columns drawn for this table.</para>
        /// </summary>
        [
        Localizable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public virtual GridColumnStylesCollection GridColumnStyles
        {
            get
            {
                return gridColumns;
            }
        }

        /// <summary>
        ///    <para>
        ///       Gets or sets the <see cref='System.Windows.Forms.DataGrid'/>
        ///       control displaying the table.
        ///    </para>
        /// </summary>

        internal void SetInternalDataGrid(DataGrid dG, bool force)
        {
            if (dataGrid != null && dataGrid.Equals(dG) && !force)
            {
                return;
            }
            else
            {
                dataGrid = dG;
                if (dG != null && dG.Initializing)
                {
                    return;
                }

                int nCols = gridColumns.Count;
                for (int i = 0; i < nCols; i++)
                {
                    gridColumns[i].SetDataGridInternalInColumn(dG);
                }
            }
        }

        /// <summary>
        /// <para>Gets or sets the <see cref='System.Windows.Forms.DataGrid'/> control for the drawn table.</para>
        /// </summary>
        [Browsable(false)]
        public virtual DataGrid DataGrid
        {
            get
            {
                return dataGrid;
            }
            set
            {
                SetInternalDataGrid(value, true);
            }
        }

        /// <summary>
        ///    <para>Gets or sets a value indicating whether columns can be
        ///       edited.</para>
        /// </summary>
        [DefaultValue(false)]
        public virtual bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                if (readOnly != value)
                {
                    readOnly = value;
                    OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler ReadOnlyChanged
        {
            add => Events.AddHandler(EventReadOnly, value);
            remove => Events.RemoveHandler(EventReadOnly, value);
        }

        // =------------------------------------------------------------------
        // =        Methods
        // =------------------------------------------------------------------

        /// <summary>
        ///    <para>Requests an edit operation.</para>
        /// </summary>
        public bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber)
        {
            DataGrid grid = DataGrid;
            if (grid == null)
            {
                return false;
            }
            else
            {
                return grid.BeginEdit(gridColumn, rowNumber);
            }
        }

        /// <summary>
        ///    <para> Requests an end to an edit
        ///       operation.</para>
        /// </summary>
        public bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
        {
            DataGrid grid = DataGrid;
            if (grid == null)
            {
                return false;
            }
            else
            {
                return grid.EndEdit(gridColumn, rowNumber, shouldAbort);
            }
        }

        internal void InvalidateColumn(DataGridColumnStyle column)
        {
            int index = GridColumnStyles.IndexOf(column);
            if (index >= 0 && DataGrid != null)
            {
                DataGrid.InvalidateColumn(index);
            }
        }


        private void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

            try
            {
                DataGrid grid = DataGrid;
                DataGridColumnStyle col = e.Element as DataGridColumnStyle;
                if (e.Action == CollectionChangeAction.Add)
                {
                    if (col != null)
                    {
                        col.SetDataGridInternalInColumn(grid);
                    }
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    if (col != null)
                    {
                        col.SetDataGridInternalInColumn(null);
                    }
                }
                else
                {
                    // refresh
                    Debug.Assert(e.Action == CollectionChangeAction.Refresh, "there are only Add, Remove and Refresh in the CollectionChangeAction");
                    // if we get a column in this collectionChangeEventArgs it means
                    // that the propertyDescriptor in that column changed.
                    if (e.Element != null)
                    {
                        for (int i = 0; i < gridColumns.Count; i++)
                        {
                            gridColumns[i].SetDataGridInternalInColumn(null);
                        }
                    }
                }

                if (grid != null)
                {
                    grid.OnColumnCollectionChanged(this, e);
                }
            }
            finally
            {
                gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
            }
        }

#if false
        /// <summary>
        ///      The DataColumnCollection class actually wires up this
        ///      event handler to the PropertyChanged events of
        ///      a DataGridTable's columns.
        /// </summary>
        internal void OnColumnChanged(object sender, PropertyChangedEvent event) {
            if (event.PropertyName.Equals("Visible"))
                GenerateVisibleColumnsCache();
        }
#endif
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            if (Events[EventReadOnly] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected virtual void OnMappingNameChanged(EventArgs e)
        {
            if (Events[EventMappingName] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected virtual void OnAlternatingBackColorChanged(EventArgs e)
        {
            if (Events[EventAlternatingBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected virtual void OnForeColorChanged(EventArgs e)
        {
            if (Events[EventBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected virtual void OnBackColorChanged(EventArgs e)
        {
            if (Events[EventForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected virtual void OnAllowSortingChanged(EventArgs e)
        {
            if (Events[EventAllowSorting] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnGridLineColorChanged(EventArgs e)
        {
            if (Events[EventGridLineColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnGridLineStyleChanged(EventArgs e)
        {
            if (Events[EventGridLineStyle] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnHeaderBackColorChanged(EventArgs e)
        {
            if (Events[EventHeaderBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnHeaderFontChanged(EventArgs e)
        {
            if (Events[EventHeaderFont] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnHeaderForeColorChanged(EventArgs e)
        {
            if (Events[EventHeaderForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnLinkColorChanged(EventArgs e)
        {
            if (Events[EventLinkColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnLinkHoverColorChanged(EventArgs e)
        {
            if (Events[EventLinkHoverColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnPreferredRowHeightChanged(EventArgs e)
        {
            if (Events[EventPreferredRowHeight] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnPreferredColumnWidthChanged(EventArgs e)
        {
            if (Events[EventPreferredColumnWidth] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnColumnHeadersVisibleChanged(EventArgs e)
        {
            if (Events[EventColumnHeadersVisible] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnRowHeadersVisibleChanged(EventArgs e)
        {
            if (Events[EventRowHeadersVisible] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnRowHeaderWidthChanged(EventArgs e)
        {
            if (Events[EventRowHeaderWidth] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnSelectionForeColorChanged(EventArgs e)
        {
            if (Events[EventSelectionForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }
        protected virtual void OnSelectionBackColorChanged(EventArgs e)
        {
            if (Events[EventSelectionBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GridColumnStylesCollection cols = GridColumnStyles;
                if (cols != null)
                {
                    for (int i = 0; i < cols.Count; i++)
                    {
                        cols[i].Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        internal bool IsDefault
        {
            get
            {
                return isDefaultTableStyle;
            }
        }

    }
}
