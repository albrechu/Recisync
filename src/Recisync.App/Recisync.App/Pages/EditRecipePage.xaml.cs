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
using Recisync.App.ViewModels;
using Resisync.Core.Data;
using Resisync.Core.UI.Locale;

namespace Recisync.App.Pages;

public partial class EditRecipePage : ContentPage
{
	public EditRecipePage()
        : this(null)
	{
    }

	public EditRecipePage(Recipe? recipe)
	{
        InitializeComponent();
        BindingContext = new EditRecipeViewModel(recipe);
    }

    public Func<object?, Popup> FailedImportFetcher => o =>
    {
        return new NotificationPopup()
        {
            NotificationType = NotificationType.Error,
            PromptType       = PromptType.OK,
            Title            = Strings.FailedImportTitle, 
            Description      = Strings.FailedImportDescription,
        };
    };
}