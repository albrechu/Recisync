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

using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace Resisync.Core.UI.Behaviors;

public partial class NavigationBehavior : Behavior<View>
{
    protected override void AfterAttached() => OnEventNameChanged(this, default, EventName);

    /// <summary>
    /// Statically attacheable property to pop the page
    /// </summary>
    public static readonly BindableProperty PopEventNameProperty =
        BindableProperty.Create("PopEventName", 
            typeof(string), 
            typeof(NavigationBehavior), 
            default, 
            propertyChanged: OnPopEventNameChanged);
    public static string GetPopEventName(BindableObject page) =>
        (string)page.GetValue(PopEventNameProperty);
    public static void SetPopEventName(BindableObject page, string eventName) =>
        page.SetValue(PopEventNameProperty, eventName);
    static void OnPopEventNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if(bindable != null && bindable is not NavigationBehavior)
        {
            Type type = bindable.GetType();
            string newEvent = (string)newValue;
            EventInfo info = type.GetEvent(newEvent) ?? throw new InvalidOperationException($"Type '{type}' does not have an event named '{newEvent}'.");
            EventHandler? eventHandler = (o, args) => Shell.Current.Navigation.PopAsync();
            info.AddEventHandler(bindable, eventHandler);
        }

    }

    /// <summary>
    /// Type to push.
    /// </summary>
    public static readonly BindableProperty PageTypeProperty =
        BindableProperty.CreateAttached(nameof(PageType), 
            typeof(Type), 
            typeof(NavigationBehavior), 
            default
        );
    public Type PageType
    {
        get => (Type)GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }

    /// <summary>
    /// Parameter for if the type is custom constructed without retrieving it as a Service.
    /// </summary>
    public static readonly BindableProperty CustomPageTypeParameterProperty =
        BindableProperty.CreateAttached(nameof(CustomPageTypeParameter),
            typeof(object),
            typeof(NavigationBehavior),
            default
        );
    public object CustomPageTypeParameter
    {
        get => GetValue(CustomPageTypeParameterProperty);
        set => SetValue(CustomPageTypeParameterProperty, value);
    }

    /// <summary>
    /// Root page, which is required when using NavigationPage for example.
    /// </summary>
    public static readonly BindableProperty RootPageTypeProperty =
        BindableProperty.CreateAttached(nameof(RootPageType),
            typeof(Type),
            typeof(NavigationBehavior),
            default
        );
    public Type RootPageType
    {
        get => (Type)GetValue(RootPageTypeProperty);
        set => SetValue(RootPageTypeProperty, value);
    }

    /// <summary>
    /// Name of the event that is fired by the element this behavior is attached to.
    /// </summary>
    public static readonly BindableProperty EventNameProperty =
        BindableProperty.CreateAttached(nameof(EventName), 
            typeof(string), 
            typeof(NavigationBehavior), 
            default,
            propertyChanged: OnEventNameChanged);
    public string EventName
    {
        get => (string)GetValue(EventNameProperty); 
        set => SetValue(EventNameProperty, value); 
    }

    static void OnEventNameChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not NavigationBehavior behavior)
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

        if (newValue != null)
        {
            string newEvent = (string)newValue;
            EventInfo info = type.GetEvent(newEvent) ?? throw new InvalidOperationException($"Type '{type}' does not have an event named '{newEvent}'.");
            behavior.m_eventHandler = (o, args) => behavior.Push();
            info.AddEventHandler(behavior.Associated, behavior.m_eventHandler);
        }
    }

    void Push()
    {
        Debug.Assert(PageType.IsSubclassOf(typeof(Page)), $"{PageType} has to be a subclass of Page.");
        // Get page instance either through service or by constructing with a parameter
        Page? page = CustomPageTypeParameter == default ?
            Ioc.Default.GetRequiredService(PageType) as Page ?? throw new InvalidOperationException("It is required that PageType of NavigationBehavior is either set or a custom constructor parameter is chosen.") :
            Activator.CreateInstance(PageType, [CustomPageTypeParameter]) as Page ?? throw new InvalidOperationException("No constructor that takes Page exists for this type.");
        if (RootPageType != null)
        {
            Debug.Assert(RootPageType.IsSubclassOf(typeof(Page)), $"{RootPageType} has to be a subclass of Page.");
            Page root = Activator.CreateInstance(RootPageType, [page]) as Page ?? throw new InvalidOperationException("No constructor that takes Page exists for this type.");
            Shell.Current.Navigation.PushAsync(root);
        }
        else
        {
            Shell.Current.Navigation.PushAsync(page);
        }
    }

    private EventHandler? m_eventHandler = null;
}
