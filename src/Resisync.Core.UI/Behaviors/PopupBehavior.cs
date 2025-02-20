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

using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Resisync.Core.UI.Behaviors;

public partial class PopupBehavior : Behavior<View>
{
    protected override void AfterAttached()
    {
        OnEventNameChanged(this, default, EventName);
        ActivationCommand = new Command(ShowPopup);
    }

    /// <summary>
    /// Event Name Property
    /// </summary>
    public static readonly BindableProperty ActivationCommandProperty =
        BindableProperty.CreateAttached(nameof(ActivationCommand),
            typeof(ICommand),
            typeof(PopupBehavior),
            default
        );
    public ICommand ActivationCommand
    {
        get => (ICommand)GetValue(ActivationCommandProperty);
        set => SetValue(ActivationCommandProperty, value);
    }

    /// <summary>
    /// Event Name Property
    /// </summary>
    public static readonly BindableProperty EventNameProperty =
        BindableProperty.CreateAttached(nameof(EventName),
            typeof(string),
            typeof(PopupBehavior),
            default,
            propertyChanged: OnEventNameChanged
        );
    public string EventName
    {
        get => (string)GetValue(EventNameProperty);  
        set => SetValue(EventNameProperty, value);
    }
    private static void OnEventNameChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not PopupBehavior behavior)
            return;

        if (behavior.Associated == null)
            return;
        
        Type type = behavior.Associated!.GetType();
        if (oldValue != null)
        {
            string oldEvent = (string)oldValue;
            EventInfo info = type.GetEvent(oldEvent)!;
            info.RemoveEventHandler(behavior.Associated, behavior.m_eventHandler);
            behavior.m_eventHandler = null;
        }

        if(newValue != null)
        {
            string newEvent = (string)newValue;
            EventInfo info = type.GetEvent(newEvent) ?? throw new InvalidOperationException($"Type '{type}' does not have an event named '{newEvent}'.");
            behavior.m_eventHandler = (o, args) => behavior.ShowPopup();
            info.AddEventHandler(behavior.Associated, behavior.m_eventHandler);
        }
    }

    /// <summary>
    /// Popup Type Property
    /// </summary>
    public static readonly BindableProperty PopupTypeProperty =
        BindableProperty.CreateAttached(nameof(PopupType), 
            typeof(Type), 
            typeof(PopupBehavior),
            default);
    public Type PopupType
    {
        get => (Type)GetValue(PopupTypeProperty);
        set => SetValue(PopupTypeProperty, value);
    }

    /// <summary>
    /// Popup Type Property
    /// </summary>
    public static readonly BindableProperty PopupInstanceFetcherProperty =
        BindableProperty.CreateAttached(nameof(PopupInstanceFetcher),
            typeof(Func<object?, Popup>),
            typeof(PopupBehavior),
            default);
    public Func<object?, Popup> PopupInstanceFetcher
    {
        get => (Func<object?, Popup>)GetValue(PopupInstanceFetcherProperty);
        set => SetValue(PopupInstanceFetcherProperty, value);
    }

    /// <summary>
    /// Anchor Property
    /// </summary>
    public static readonly BindableProperty AnchorProperty =
        BindableProperty.CreateAttached(nameof(Popup.Anchor),
            typeof(View),
            typeof(PopupBehavior),
            default);
    public View Anchor
    {
        get => (View)GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }

    /// <summary>
    /// Parameter Property
    /// </summary>
    public static readonly BindableProperty ParameterProperty =
        BindableProperty.Create(nameof(Parameter),
            typeof(object),
            typeof(PopupBehavior),
            null);

    public object Parameter
    {
        get => GetValue(ParameterProperty);
        set => SetValue(ParameterProperty, value);
    }

    void ShowPopup()
    {
        Debug.Assert(PopupInstanceFetcher != null || PopupType.IsSubclassOf(typeof(Popup)), $"{PopupType} has to be a subclass of Popup.");

        Popup? popup = null;
        if (PopupInstanceFetcher == null)
        {
            if (Parameter == null)
            {
                popup = Ioc.Default.GetService(PopupType) as Popup;
            }

            popup ??= (Popup)Activator.CreateInstance(PopupType, Parameter == null ? [] : [Parameter])!;
        }
        else
        {
            popup = PopupInstanceFetcher(Parameter);
        }

        popup.Anchor = Anchor;

        Application.Current?.MainPage?.ShowPopupAsync(popup);
    }

    private EventHandler? m_eventHandler = null;
}
