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

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using Resisync.Core.UI.Locale;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;

namespace Recisync.App.Pages;

public class SelectableItem : ObservableObject
{
    public SelectableItem(SelectionPopup popup, object @object)
    {
        Popup = popup;
        Object = @object;
        Refresh();
    }

    public void Refresh()
    {
        m_enabled = Popup.SelectedItems != null && Popup.SelectedItems.Contains(Object);
        Name = Popup.Converter == null ? Object.ToString() : Popup.Converter.Convert(Object, typeof(string), null, CultureInfo.CurrentCulture) as string;
    }

    public SelectionPopup Popup { get; init; }
    public object Object { get; init; }
    public string? Name { get; set; }
    public bool Enabled 
    { 
        get => m_enabled;
        set
        {
            if (m_enabled != value)
            {
                Popup.SelectItem(value, Object, out m_enabled);
                OnPropertyChanged();
            }
        }
    }

    bool m_enabled;
}

public partial class SelectionPopup : Popup, ICloneable
{
    /// <summary>
    /// Title Property for Popup Title
    /// </summary>
    public static readonly BindableProperty CustomContentProperty =
        BindableProperty.Create(nameof(CustomContent),
            typeof(View),
            typeof(SelectionPopup),
            default);
    public View CustomContent
    {
        get => (View)GetValue(CustomContentProperty);
        set => SetValue(CustomContentProperty, value);
    }

    /// <summary>
    /// Title Property for Popup Title
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title),
            typeof(string),
            typeof(SelectionPopup),
            default);
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // <summary>
    /// Title Property for Popup Title
    /// </summary>
    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText),
            typeof(string),
            typeof(SelectionPopup),
            default);
    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    /// <summary>
    /// Converter Property for Localization
    /// </summary>
    public static readonly BindableProperty ConverterProperty =
        BindableProperty.Create(nameof(Converter),
            typeof(IValueConverter),
            typeof(SelectionPopup),
            default,
            propertyChanged: OnItemsSourceChanged);
    public IValueConverter Converter
    {
        get => (IValueConverter)GetValue(ConverterProperty);
        set => SetValue(ConverterProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty =
        BindableProperty.Create(nameof(SelectionMode),
            typeof(SelectionMode),
            typeof(SelectionPopup),
            SelectionMode.Multiple,
            propertyChanged: OnItemsSourceChanged);
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// List of FilterElements Property
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SelectionPopup),
            default,
            propertyChanged: OnItemsSourceChanged);
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// List of Selected Items Property
    /// </summary>
    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(nameof(SelectedItems),
            typeof(IList<object>),
            typeof(SelectionPopup),
            default,
            propertyChanged: OnSelectedItemsChanged);
    public IList<object> SelectedItems
    {
        get => (IList<object>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }
    private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SelectionPopup popup && popup.SelectionChangedCommand != null)
        {
            if (popup.SelectionChangedCommand.CanExecute(null))
                popup.SelectionChangedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Calls this Command if a selection value has changed.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(SelectionPopup),
            default
        );
    public ICommand SelectionChangedCommand
    {
        get => (ICommand)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public ObservableCollection<SelectableItem> Items { get; set; }

    public SelectionPopup() =>
        InitializeComponent();

    private void OnClose(object sender, EventArgs e) =>
        Close();

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SelectionPopup popup && popup.ItemsSource != null)
        {
            popup.OnPropertyChanging(nameof(popup.Items));
            popup.Items = popup.Items ?? new ObservableCollection<SelectableItem>();
            popup.Items.Clear();
            var enumerator = popup.ItemsSource.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
                popup.Items.Add(new SelectableItem(popup, enumerator.Current));

            popup.OnPropertyChanged(nameof(popup.Items));
        }
    }

    public void SelectItem(bool enable, object item, out bool value)
    {
        value = enable;
        if (SelectionMode == SelectionMode.Single)
        {
            if(enable == true)
            {
                SelectedItems.Add(item);
                Items.First(s => SelectedItems[0].Equals(s.Object)).Enabled = false;

                if (SelectionChangedCommand.CanExecute(null))
                    SelectionChangedCommand.Execute(null);
            }
            else if (SelectedItems.Count > 1)
            {
                SelectedItems.Remove(item);
            }
            else
            {
                value = true;
            }
        }
        else
        {
            if (enable)
                SelectedItems.Add(item);
            else
                SelectedItems.Remove(item);

            if (SelectionChangedCommand.CanExecute(null))
                SelectionChangedCommand.Execute(null);
        }
    }

    public object Clone()
    {
        
        return new SelectionPopup
        {
            CustomContent           = CustomContent,
            Title                   = Title,
            IconText                = IconText,
            Converter               = Converter,
            SelectionMode           = SelectionMode,
            ItemsSource             = ItemsSource,
            SelectedItems           = SelectedItems,
            SelectionChangedCommand = SelectionChangedCommand,
        };
    }
}