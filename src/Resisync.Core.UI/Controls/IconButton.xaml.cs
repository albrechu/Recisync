/*
 * MIT License
 * 
 * Copyright (c) 2025 Julian Albrecht
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Microsoft.Maui.Controls.Shapes;
using System.Windows.Input;

namespace Resisync.Core.UI.Controls;

public partial class IconButton : ContentView
{
    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText),
            typeof(string),
            typeof(IconButton),
            default);
    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text),
            typeof(string),
            typeof(IconButton),
            default);
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily),
            typeof(string),
            typeof(IconButton),
            default);
    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize),
            typeof(double),
            typeof(IconButton),
            default);
    [System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty IconSizeProperty =
        BindableProperty.Create(nameof(IconSize),
            typeof(double),
            typeof(IconButton),
            default);
    [System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly BindableProperty IconFontFamilyProperty =
        BindableProperty.Create(nameof(IconFontFamily),
            typeof(string),
            typeof(IconButton),
            default);
    public string IconFontFamily
    {
        get => (string)GetValue(IconFontFamilyProperty);
        set => SetValue(IconFontFamilyProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command),
            typeof(ICommand),
            typeof(IconButton),
            default,
            propertyChanged: OnCommandChanged);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter),
            typeof(object),
            typeof(IconButton),
            default);
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor),
            typeof(Color),
            typeof(IconButton),
            default);
    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty); 
        set => SetValue(TextColorProperty, value); 
    }

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor),
            typeof(Color),
            typeof(IconButton),
            default);
    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public event EventHandler? Pressed;


    public static readonly BindableProperty StrokeShapeProperty =
        BindableProperty.Create(nameof(StrokeShape),
            typeof(IShape),
            typeof(IconButton),
            default);
    [System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
    public IShape? StrokeShape
    {
        get => (IShape?)GetValue(StrokeShapeProperty); 
        set => SetValue(StrokeShapeProperty, value); 
    }
    public IconButton() => 
        InitializeComponent();

    private void OnTapped(object sender, TappedEventArgs e) =>
        Pressed?.Invoke(this, new EventArgs());

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if(bindable is IconButton btn && btn.Command != null)
            btn.IsEnabled = btn.Command.CanExecute(btn.CommandParameter);
    }
}