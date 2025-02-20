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

using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Recisync.App.Messages;
using Resisync.Core.Data;
using Resisync.Core.Extensions;
using Resisync.Core.Interfaces;

namespace Recisync.App.ViewModels;

[ObservableObject]
public partial class RecipesViewModel
{
    public RecipesViewModel(IRecipeService service)
    {
        m_service          = service;
        Categories         = Enum.GetValues<FoodCategory>();
        SelectedCategories = Categories.Cast<object>().ToList();
        Sorts              = Enum.GetValues<SortRecipeBy>();
        SelectedSortBy     = [SortRecipeBy.Date];
        m_searchResults    = [];
        StartRecipeCollection();
    }

    public FoodCategory[]      Categories { get; set; }
    public IList<object> SelectedCategories { get; set; } 
    public SortRecipeBy[]      Sorts { get; set; } 
    public IList<object> SelectedSortBy { get; set; }


    void StartRecipeCollection()
    {
        WeakReferenceMessenger.Default.Register<ConnectionMessage>(this, async (r, m) =>
        {
            if (m.Value)
            {
                Recipes = await m_service.GetMissingRecipesAsync(Path.Combine(FileSystem.AppDataDirectory, "recipes/"), Recipes) ?? Recipes;
                await RefreshSearch();
            }
            IsConnected = m.Value;
            m_canSync = true;
            OnPropertyChanged(nameof(SyncCommand));
        });

        WeakReferenceMessenger.Default.Register<RecipeAddedMessage>(this, async (r, m) =>
        {
            // Update UI
            int i = Recipes.IndexOf(item => m.Value.Id == item.Id);
            if (i >= 0) 
                Recipes[i] = m.Value;
            else
                Recipes.Add(m.Value);
            await RefreshSearch();

            // Update Cloud
            if (m_service is IRecipeService service && service.IsConnected)
                await service.AddOrChangeRecipeAsync(m.Value);
            // Update File System
            string file = Path.Combine(FileSystem.AppDataDirectory, "recipes/", m.Value.CreateFileName());
            Recipe.ToFile(m.Value, file);
        });

        // Get local and remote recipes
        Task.Run(async () => 
        {
            string recipes = Path.Combine(FileSystem.AppDataDirectory, "recipes/");
            if(!Path.Exists(recipes)) 
                Directory.CreateDirectory(recipes);
            // Find recipes in file system
            Directory.EnumerateFiles(recipes)
                .Where(f => f.EndsWith(".recipe"))
                .ForEach(f =>
                {
                    if(Recipe.FromFile(f) is Recipe recipe)
                        Recipes.Add(recipe);
                });
            await RefreshSearch();
            // Retrieve remote recipes
            string? endpoint = await SecureStorage.GetAsync("user-endpoint");
            string? auth     = await SecureStorage.GetAsync("user-auth");
            if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(auth))
            {
                WeakReferenceMessenger.Default.Send(new ConnectionMessage(await m_service.LoginAsync(endpoint, auth)));
            }
        });
    }

    [RelayCommand]
    async Task RefreshSearch()
    {
        if (string.IsNullOrEmpty(Query))
        {
            SearchResults = new List<Recipe>(Recipes);
        }
        else
        {
            IList<Recipe> recipes = [];
            foreach (Recipe recipe in Recipes)
            {
                if (recipe.Name.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ||
                    recipe.Keywords.Contains(Query, StringComparison.InvariantCultureIgnoreCase))
                {
                    recipes.Add(recipe);
                }
            }
            SearchResults = recipes;
        }
        await Filter();
    }

    [RelayCommand]
    async Task Filter() 
    {
        await Task.Run(() =>
        {
            IEnumerable<Recipe> recipes;
            if (SelectedCategories == null)
            {
                recipes = SearchResults.AsEnumerable();
            }
            else if (SelectedCategories.Count > 0)
            {
                recipes = SearchResults.Where(r => SelectedCategories.Contains(r.FoodCategory));
            }
            else
            {
                Results = [];
                return;
            }

            switch ((SortRecipeBy)SelectedSortBy[0])
            {
                case SortRecipeBy.Name:         Results = [.. recipes.Sort(x => x.Name, Descending)];         break;
                case SortRecipeBy.FoodCategory: Results = [.. recipes.Sort(x => x.FoodCategory, Descending)]; break;
                case SortRecipeBy.People:       Results = [.. recipes.Sort(x => x.People, Descending)];       break;
                case SortRecipeBy.Duration:     Results = [.. recipes.Sort(x => x.Duration, Descending)];     break;
                case SortRecipeBy.Date:         Results = [.. recipes.Sort(x => x.Date, Descending)];         break;
                default: break;
            }
        });
    }

    [RelayCommand]
    async Task SwapOrderPolarity()
    {
        Descending = !Descending;
        await Filter();
    }

    [RelayCommand]
    async Task Tag(string tag)
    {
        Query = tag;
        await RefreshSearch();
    }

    [RelayCommand]
    async Task Delete(Recipe? recipe)
    {
        if (recipe != null)
        {
            // UI Refresh
            Recipes.Remove(recipe);
            await RefreshSearch();
            // Delete Locally
            string path = Path.Combine(FileSystem.AppDataDirectory, "recipes/", recipe.CreateFileName());
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            // Delete Remotely
            if (m_service.IsConnected)
            {
                await m_service.DeleteAsync(recipe);
            }
        }
    }

    [RelayCommand]
    void SwapTheme() =>
        Application.Current!.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;

    public ICommand SyncCommand => new Command(async () =>
    {
        m_canSync = false;
        OnPropertyChanged(nameof(SyncCommand));
        Recipes = await m_service.GetMissingRecipesAsync(Path.Combine(FileSystem.AppDataDirectory, "recipes/"), Recipes) ?? Recipes;
        await RefreshSearch();
        m_canSync = true;
        OnPropertyChanged(nameof(SyncCommand));
    }, () => m_canSync);

    public string Query
    {
        get => m_query;
        set
        {
            if (m_query != value)
            {
                m_query = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(m_query))
                    Task.Run(RefreshSearch);    
            }
        }
    }

    [ObservableProperty]
    bool m_isConnected = false;
    [ObservableProperty]
    IList<Recipe> m_searchResults;
    [ObservableProperty]
    IList<Recipe> m_results;
    [ObservableProperty]
    bool m_descending = false;
    [ObservableProperty]
    IList<Recipe> m_recipes = [];

    bool   m_canSync = false;
    string m_query = string.Empty;
    IRecipeService m_service;
}
