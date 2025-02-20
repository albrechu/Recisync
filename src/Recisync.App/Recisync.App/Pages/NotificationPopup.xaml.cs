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
using Resisync.Core.Extensions;
using System.Windows.Input;

namespace Recisync.App.Pages;

public enum NotificationType
{
    Notification = 0,
    Warning      = 1,
    Error        = 2,
}

public enum PromptType
{
    OK    = 0,
    YesNo = 1,
}

public enum PromptResult
{
    OK  = 0,
    Yes = 1,
    No  = 2
}

public partial class NotificationPopup : Popup
{
    /// <summary>
    /// Titlebar Title Property
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title),
            typeof(string),
            typeof(NotificationPopup),
            default);
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Notification Description
    /// </summary>
    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description),
            typeof(string),
            typeof(NotificationPopup),
            default);
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Notification Description
    /// </summary>
    public static readonly BindableProperty NotificationTypeProperty =
        BindableProperty.Create(nameof(NotificationType),
            typeof(NotificationType),
            typeof(NotificationPopup),
            NotificationType.Notification
            );
    public NotificationType NotificationType
    {
        get => (NotificationType)GetValue(NotificationTypeProperty);
        set => SetValue(NotificationTypeProperty, value);
    }

    /// <summary>
    /// Calls this Command if:
    ///     - OK  pressed
    ///     - Yes pressed
    /// </summary>
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command),
            typeof(ICommand),
            typeof(NotificationPopup),
            default
        );
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Calls this Command if a selection value has changed.
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter),
            typeof(object),
            typeof(NotificationPopup),
            default
        );
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Notification Description
    /// </summary>
    public static readonly BindableProperty PromptTypeProperty =
        BindableProperty.Create(nameof(PromptType),
            typeof(PromptType),
            typeof(NotificationPopup),
            PromptType.OK
            );
    public PromptType PromptType
    {
        get => (PromptType)GetValue(PromptTypeProperty);
        set => SetValue(PromptTypeProperty, value);
    }

    /// <summary>
    /// Constructors
    /// </summary>
    public NotificationPopup() =>
		InitializeComponent();

    public ICommand PressedCommand => new Command<PromptResult>(result =>
    {
        switch (PromptType)
        {
            case PromptType.OK:
                Command.TryExecute(CommandParameter);
                break;
            case PromptType.YesNo:
                if(result == PromptResult.Yes)
                    Command.TryExecute(CommandParameter);
                break;
        }
        Close();
    });
}