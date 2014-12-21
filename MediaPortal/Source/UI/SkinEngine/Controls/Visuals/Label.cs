#region Copyright (C) 2007-2014 Team MediaPortal

/*
    Copyright (C) 2007-2014 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.UI.SkinEngine.DirectX11;
using MediaPortal.UI.SkinEngine.Rendering;
using SharpDX;
using MediaPortal.Utilities.DeepCopy;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Size = SharpDX.Size2;
using SizeF = SharpDX.Size2F;
using PointF = SharpDX.Vector2;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  // TODO: We don't notice font changes if font is declared on a parent element, so add a virtual font change handler in parent
  public class Label : Control
  {
    public const double DEFAULT_SCROLL_SPEED = 20.0;
    public const double DEFAULT_SCROLL_DELAY = 2.0;

    #region Protected fields

    protected AbstractProperty _contentProperty;
    protected AbstractProperty _colorProperty;
    protected AbstractProperty _scrollProperty;
    protected AbstractProperty _scrollSpeedProperty;
    protected AbstractProperty _scrollDelayProperty;
    protected AbstractProperty _wrapProperty;
    //protected TextBuffer _asset = null;
    protected string _resourceString;
    private Size2F _totalSize;
    private TextFormat _textFormat;
    private TextLayout _textLayout;
    private SolidColorBrush _textBrush;

    #endregion

    #region Ctor

    public Label()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _contentProperty = new SProperty(typeof(string), null);
      _colorProperty = new SProperty(typeof(Color), Color.DarkViolet);
      _scrollProperty = new SProperty(typeof(TextScrollEnum), TextScrollEnum.None);
      _scrollSpeedProperty = new SProperty(typeof(double), DEFAULT_SCROLL_SPEED);
      _scrollDelayProperty = new SProperty(typeof(double), DEFAULT_SCROLL_DELAY);
      _wrapProperty = new SProperty(typeof(bool), false);

      HorizontalAlignment = HorizontalAlignmentEnum.Left;
      InitializeResourceString();
    }

    void Attach()
    {
      _contentProperty.Attach(OnContentChanged);
      _wrapProperty.Attach(OnLayoutPropertyChanged);
      _scrollProperty.Attach(OnLayoutPropertyChanged);
      _scrollSpeedProperty.Attach(OnLayoutPropertyChanged);
      _scrollDelayProperty.Attach(OnLayoutPropertyChanged);
      _colorProperty.Attach(OnColorPropertyChanged);

      HorizontalAlignmentProperty.Attach(OnLayoutPropertyChanged);
      VerticalAlignmentProperty.Attach(OnLayoutPropertyChanged);
      FontFamilyProperty.Attach(OnFontChanged);
      FontSizeProperty.Attach(OnFontChanged);

      HorizontalContentAlignmentProperty.Attach(OnLayoutPropertyChanged);
      VerticalContentAlignmentProperty.Attach(OnLayoutPropertyChanged);
    }

    void Detach()
    {
      _contentProperty.Detach(OnContentChanged);
      _wrapProperty.Detach(OnLayoutPropertyChanged);
      _scrollProperty.Detach(OnLayoutPropertyChanged);
      _scrollSpeedProperty.Detach(OnLayoutPropertyChanged);
      _scrollDelayProperty.Detach(OnLayoutPropertyChanged);
      _colorProperty.Detach(OnColorPropertyChanged);

      HorizontalAlignmentProperty.Detach(OnLayoutPropertyChanged);
      VerticalAlignmentProperty.Detach(OnLayoutPropertyChanged);
      FontFamilyProperty.Detach(OnFontChanged);
      FontSizeProperty.Detach(OnFontChanged);

      HorizontalContentAlignmentProperty.Detach(OnLayoutPropertyChanged);
      VerticalContentAlignmentProperty.Detach(OnLayoutPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Label l = (Label)source;
      Content = l.Content;
      HorizontalAlignment = l.HorizontalAlignment;
      VerticalAlignment = l.VerticalAlignment;
      Color = l.Color;
      Scroll = l.Scroll;
      ScrollDelay = l.ScrollDelay;
      ScrollSpeed = l.ScrollSpeed;
      Wrap = l.Wrap;

      InitializeResourceString();
      Attach();
    }

    #endregion

    #region Property change handlers

    void OnContentChanged(AbstractProperty prop, object oldValue)
    {
      InitializeResourceString();
      ReAllocFont();
      InvalidateLayout(true, false);
    }

    void OnLayoutPropertyChanged(AbstractProperty prop, object oldValue)
    {
      ReAllocFont();
      InvalidateLayout(true, false);
    }

    protected void OnFontChanged(AbstractProperty prop, object oldValue)
    {
      InvalidateLayout(true, false);
      ReAllocFont();
    }

    private void OnColorPropertyChanged(AbstractProperty property, object oldvalue)
    {
      if (_textBrush != null)
      {
        _textBrush.Color = Color;
      }
    }

    #endregion

    protected void InitializeResourceString()
    {
      string content = Content;
      _resourceString = string.IsNullOrEmpty(content) ? string.Empty :
          LocalizationHelper.CreateResourceString(content).Evaluate();
    }

    #region Public properties

    public AbstractProperty ContentProperty
    {
      get { return _contentProperty; }
    }

    public string Content
    {
      get { return (string)_contentProperty.GetValue(); }
      set { _contentProperty.SetValue(value); }
    }

    /// <summary>
    /// Gets or sets the color of the label.
    /// </summary>
    public Color Color
    {
      get { return (Color)_colorProperty.GetValue(); }
      set { _colorProperty.SetValue(value); }
    }

    public AbstractProperty ColorProperty
    {
      get { return _colorProperty; }
    }

    /// <summary>
    /// Determines how to scroll text.
    /// </summary>
    public TextScrollEnum Scroll
    {
      get { return (TextScrollEnum)_scrollProperty.GetValue(); }
      set { _scrollProperty.SetValue(value); }
    }

    public AbstractProperty ScrollProperty
    {
      get { return _scrollProperty; }
    }

    /// <summary>
    /// Sets the delay in seconds before scrolling starts.
    /// </summary>
    public Double ScrollDelay
    {
      get { return (double)_scrollDelayProperty.GetValue(); }
      set { _scrollDelayProperty.SetValue(value); }
    }

    public AbstractProperty ScrollDelayProperty
    {
      get { return _scrollDelayProperty; }
    }

    /// <summary>
    /// Gets or sets the scroll speed for text in skin units per second (1 unit = 1 pixel at native skin resolution).
    /// <see cref="Scroll"/> must also be set for this to have an effect.
    /// </summary>
    public double ScrollSpeed
    {
      get { return (double)_scrollSpeedProperty.GetValue(); }
      set { _scrollSpeedProperty.SetValue(value); }
    }

    public AbstractProperty ScrollSpeedProperty
    {
      get { return _scrollSpeedProperty; }
    }

    /// <summary>
    /// Gets or sets whether content text should be horizontally wrapped when it longer than can fit on a single line.
    /// </summary>
    public bool Wrap
    {
      get { return (bool)_wrapProperty.GetValue(); }
      set { _wrapProperty.SetValue(value); }
    }

    public AbstractProperty WrapProperty
    {
      get { return _wrapProperty; }
    }

    #endregion

    void ReAllocFont()
    {
      DeAllocFont();
      AllocFont();
    }

    private void DeAllocFont()
    {
      TryDispose(ref _textFormat);
      TryDispose(ref _textLayout);
      TryDispose(ref _textBrush);
    }

    void AllocFont()
    {
      // HACK: avoid NREs during style load time
      if (GraphicsDevice11.Instance.FactoryDW == null)
        return;

      if (_textLayout == null)
      {
        _textFormat = new TextFormat(GraphicsDevice11.Instance.FactoryDW, GetFontFamilyOrInherited(), FontWeight.Normal, FontStyle.Normal, GetFontSizeOrInherited()); // create the text format of specified font configuration

        // create the text layout - this improves the drawing performance for static text
        // as the glyph positions are precalculated
        float layoutWidth = _totalSize.Width;
        if (float.IsNaN(layoutWidth) || layoutWidth == 0.0f)
          layoutWidth = 2048;
        float layoutHeight = _totalSize.Height;
        if (float.IsNaN(layoutHeight) || layoutHeight == 0.0f)
          layoutHeight = 2048;
        _textFormat = new TextFormat(GraphicsDevice11.Instance.FactoryDW, GetFontFamilyOrInherited(), FontWeight.Normal, FontStyle.Normal, GetFontSizeOrInherited()); // create the text format of specified font configuration
        _textLayout = new TextLayout(GraphicsDevice11.Instance.FactoryDW, _resourceString, _textFormat, layoutWidth, layoutHeight);
        // _asset = new TextBuffer(GetFontFamilyOrInherited(), GetFontSizeOrInherited()) { Text = _resourceString };
      }
      if (_textBrush == null)
      {
        _textBrush = new SolidColorBrush(GraphicsDevice11.Instance.Context2D1, Color);
      }
    }

    protected override SizeF CalculateInnerDesiredSize(SizeF totalSize)
    {
      base.CalculateInnerDesiredSize(totalSize); // Needs to be called in each sub class of Control, see comment in Control.CalculateInnerDesiredSize()
      // Measure the text
      float totalWidth = totalSize.Width; // Attention: totalWidth is cleaned up by SkinContext.Zoom
      if (!double.IsNaN(Width))
        totalWidth = (float)Width;
      SizeF size = new SizeF();
      size.Width = 0;
      string[] lines = _resourceString.Split(Environment.NewLine.ToCharArray());
      using (var textFormat = new TextFormat(GraphicsDevice11.Instance.FactoryDW, GetFontFamilyOrInherited(), FontWeight.Normal, FontStyle.Normal, GetFontSizeOrInherited()))
      {
        foreach (string line in lines)
        {
          using (var textLayout = new TextLayout(GraphicsDevice11.Instance.FactoryDW, line, textFormat, totalWidth, 4096))
          {
            size.Width = Math.Max(size.Width, textLayout.Metrics.WidthIncludingTrailingWhitespace);
            size.Height = textLayout.Metrics.Height;
          }
        }
      }
      // Add one pixel to compensate rounding errors. Stops the label scrolling even though there is enough space.
      size.Width += 1;
      size.Height += 1;
      _totalSize = size;
      return size;

      //AllocFont();
      //return totalSize;
      //// Measure the text
      //float totalWidth = totalSize.Width; // Attention: totalWidth is cleaned up by SkinContext.Zoom
      //if (!double.IsNaN(Width))
      //  totalWidth = (float)Width;

      //SizeF size = new SizeF();
      //var textBuffer = _asset;
      //if (textBuffer == null)
      //  return size;
      //string[] lines = textBuffer.GetLines(totalWidth, Wrap);
      //size.Width = 0;
      //foreach (string line in lines)
      //  size.Width = Math.Max(size.Width, textBuffer.TextWidth(line));
      //size.Height = textBuffer.TextHeight(Math.Max(lines.Length, 1));

      //// Add one pixel to compensate rounding errors. Stops the label scrolling even though there is enough space.
      //size.Width += 1;
      //size.Height += 1;
      //return size;
    }

    public override void RenderOverride(RenderContext localRenderContext)
    {
      base.RenderOverride(localRenderContext);

      AllocFont();

      HorizontalTextAlignEnum horzAlign = HorizontalTextAlignEnum.Left;
      if (HorizontalContentAlignment == HorizontalAlignmentEnum.Right)
        horzAlign = HorizontalTextAlignEnum.Right;
      else if (HorizontalContentAlignment == HorizontalAlignmentEnum.Center)
        horzAlign = HorizontalTextAlignEnum.Center;

      VerticalTextAlignEnum vertAlign = VerticalTextAlignEnum.Top;
      if (VerticalContentAlignment == VerticalAlignmentEnum.Bottom)
        vertAlign = VerticalTextAlignEnum.Bottom;
      else if (VerticalContentAlignment == VerticalAlignmentEnum.Center)
        vertAlign = VerticalTextAlignEnum.Center;

      Color4 color = ColorConverter.FromColor(Color);
      color.Alpha *= (float)localRenderContext.Opacity;

      var oldOpacity = _textBrush.Opacity;
      _textBrush.Opacity *= (float)localRenderContext.Opacity;
      GraphicsDevice11.Instance.Context2D1.DrawTextLayout(localRenderContext.OccupiedTransformedBounds.TopLeft, _textLayout, _textBrush);
      _textBrush.Opacity = oldOpacity;

      //_asset.Render(_innerRect, horzAlign, vertAlign, color, Wrap, true, localRenderContext.ZOrder, 
      //  Scroll, (float) ScrollSpeed, (float) ScrollDelay, localRenderContext.Transform);
    }

    public override void Deallocate()
    {
      base.Deallocate();
      DeAllocFont();
    }

    public override void Dispose()
    {
      Deallocate();
      base.Dispose();
    }
  }
}

