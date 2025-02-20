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
using Recisync.App.Converters;
using Recisync.App.ViewModels;
using Resisync.Core.Extensions;
using Resisync.Core.UI.Controls;
using Resisync.Core.UI.Locale;

namespace Recisync.App.Pages;

public partial class RecipesPage : ContentPage
{
	public RecipesPage(RecipesViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnFilterChanged(object sender, ToggledEventArgs e) =>
        ViewModel?.RefreshSearchCommand.TryExecute();

    private RecipesViewModel? ViewModel => BindingContext as RecipesViewModel;

    public Func<object?, Popup> DeletePopupFetcher => o =>
    {
        var vm = ViewModel;
        NotificationPopup popup = new()
        {
            Title            = Strings.DeleteRecipe,
            Description      = Strings.DeleteRecipeDescription,
            NotificationType = NotificationType.Notification,
            PromptType       = PromptType.YesNo,
            CommandParameter = o,
        };
        popup.SetBinding(NotificationPopup.CommandProperty, new Binding(nameof(vm.DeleteCommand), source: vm));
        return popup;
    };

    public Func<object?, Popup> FilterPopupFetcher => o =>
    {
        var vm = ViewModel;
        SelectionPopup popup = new()
        {
            Converter = new FoodCategoryToStringConverter(),
            Title     = Strings.Filter,
            IconText  = Icons.FilterAlt,
        };
        popup.SetBinding(SelectionPopup.SelectedItemsProperty,           nameof(vm.SelectedCategories));
        popup.SetBinding(SelectionPopup.ItemsSourceProperty,             nameof(vm.Categories));
        popup.SetBinding(SelectionPopup.SelectionChangedCommandProperty, nameof(vm.FilterCommand));
        return popup;
    };

    public Func<object?, Popup> SortPopupFetcher => o =>
    {
        var vm = ViewModel;
        // Create Order Polarity Button
        IconButton sortOrder = new()
        {
            BindingContext = vm,
            IconText = Icons.North,
            Text = Strings.Ascending,
            Margin = new Thickness(20),
        };
        sortOrder.SetBinding(IconButton.CommandProperty, nameof(vm.SwapOrderPolarityCommand));
        // Create Trigger
        DataTrigger orderTrigger = new DataTrigger(typeof(IconButton))
        {
            Setters =
            {
                new()
                {
                    Property = IconButton.IconTextProperty,
                    Value    = Icons.South,
                },
                new()
                {
                    Property = IconButton.TextProperty,
                    Value    = Strings.Descending,
                },
                new()
                {
                    Property = IconButton.IconColorProperty,
                    Value    = "#D0312D",
                }
            }
        };
        orderTrigger.BindingContext = vm;
        orderTrigger.Binding = new Binding(nameof(vm.Descending));
        orderTrigger.Value = true;
        sortOrder.Triggers.Add(orderTrigger);
        // Create Popup
        SelectionPopup popup = new SelectionPopup
        {
            Title = Strings.SortBy,
            IconText = Icons.Sort,
            BindingContext = vm,
            SelectionMode = SelectionMode.Single,
            Converter = new SortRecipeByToStringConverter(),
            CustomContent = sortOrder,
        };
        popup.SetBinding(SelectionPopup.SelectedItemsProperty, nameof(vm.SelectedSortBy));
        popup.SetBinding(SelectionPopup.ItemsSourceProperty, nameof(vm.Sorts));
        popup.SetBinding(SelectionPopup.SelectionChangedCommandProperty, nameof(vm.FilterCommand));
        return popup;
    };
}

