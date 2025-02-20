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

using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Recisync.App.Messages;
using Resisync.Core.Data;
using Resisync.Core.Extensions;
using Resisync.Core.UI.Locale;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Recisync.App.ViewModels;

internal partial class EditRecipeViewModel : ObservableObject
{
    public EditRecipeViewModel(Recipe? recipe)
    {
        if(recipe == null)
        {
            Recipe = new Recipe()
            {
                Id               = Guid.NewGuid().ToString(),
                Name             = Strings.Recipe,
                People           = 1,
                Date             = DateTime.Now,
                Duration         = TimeSpan.FromMinutes(15),
                Description      = string.Empty,
                FoodCategory     = FoodCategory.Breakfast,
                Keywords         = "Rice;Vegetables",
                ShortDescription = "This is my rice dish...",
                Steps            = new ObservableCollection<RecipeStep>
                {
                    new RecipeStep
                    { 
                        Step        = 1,
                        Name        = "Preparations",
                        Description = "Cook Brown Rice for about 35 min (18 min for long grained white rice). Meanwhile you can...",
                    },
                },
                Ingredients = new ObservableCollection<Ingredient>
                {
                    new Ingredient
                    {
                        Amount = "100g",
                        Name   = "Brown Rice",
                        Note   = "Preferably but not mandatory.",
                    },
                },
            };
        }
        else
        {
            Recipe = Recipe.FromString(recipe.ToString()) ?? throw new InvalidDataException("Any recipe should be de-/serializable.");
            Recipe.Date = DateTime.Now;
        }
    }

    [RelayCommand]
    void SubmitRecipe()
    {
        WeakReferenceMessenger.Default.Send(new RecipeAddedMessage(Recipe));
        Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    async Task OpenImage(RecipeStep? step = null)
    {
        try
        {
            var result = await FilePicker.PickAsync(PickOptions.Images);
            if(result != null)
            {
                var bytes = File.ReadAllBytes(result.FullPath);
                if (step == null)
                {
                    CoverImage = bytes;
                }
                else
                {
                    int index = Steps.IndexOf(step);
                    Steps[index] = Steps[index] with { Image = bytes };
                }
            }
        }
        catch (Exception e) // User canceled or error
        {
        }
    }
    [RelayCommand]
    async Task CapturePhoto(RecipeStep? step = null)
    {
        try
        {
            FileResult? result = await MediaPicker.CapturePhotoAsync();
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                var bytes = stream.ToByteArray();
                if (step == null)
                {
                    CoverImage = bytes;
                }
                else
                {
                    int index = Steps.IndexOf(step);
                    Steps[index] = Steps[index] with { Image = bytes };
                }
            }
        }
        catch (Exception) // User canceled or error
        {
        }
    }

    [RelayCommand]
    async Task Import(string? text = null)
    {
        bool isTextParsed = text != null;
        if (!isTextParsed && Clipboard.HasText) // Text to parse should be taken from clipboard
            text = await Clipboard.GetTextAsync();

        if (text != null) // There is text available
        {
            Recipe? recipe = Recipe.FromString(File.Exists(text) ? File.ReadAllText(Path.GetFullPath(text)) : text);
            if (recipe != null)
            {
                Recipe             = recipe;
                Recipe.Ingredients = Recipe.Ingredients.ToObservableCollection();
                Recipe.Steps       = Recipe.Steps.ToObservableCollection();
                if(isTextParsed)
                    Recipe.Id = Guid.NewGuid().ToString();
                // Refresh page
                GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .ForEach(p     => OnPropertyChanged(p.Name));
            }
            else // String could not be converted.
            {
                ImportErrorCommand.TryExecute();
            }
        }
        else // No text available to import
        {
            ImportErrorCommand.TryExecute();
        }
    }
    public ICommand ImportErrorCommand { get; set; }

    [RelayCommand]
    async Task Export() =>
        await Clipboard.SetTextAsync(Recipe.ToString());

    [RelayCommand]
    void AddNewIngredient() 
    {
        Ingredients.Add(new Ingredient
        {
            Amount = "2 tbsp",
            Name   = "Salt",
            Note   = ""
        });
    }

    [RelayCommand]
    async Task AddNewStep()
    {
        Debug.Assert(Steps != null);
        int step = Steps.Count + 1;
        Steps.Add(new RecipeStep{
            Step        = step,
            Name        = Strings.Step,
            Description = "",
        });
    }

    [RelayCommand]
    void RemoveIngredient(Ingredient ingredient) => 
        Ingredients.Remove(ingredient);
    [RelayCommand]
    void RemoveStep(RecipeStep step) =>
        Steps.Remove(step);

    [RelayCommand]
    void StepsReordered()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            if (Steps[i].Step != (i + 1))
            {
                Steps[i] = Steps[i] with { Step = i + 1 };
            }
        }
    }

    public List<int> Hours
    {
        get
        {
            var list = new List<int>(24);
            for (int i = 0; i < 24; ++i)
                list.Add(i);
            return list;
        }
    }
    public List<int> Minutes
    {
        get
        {
            var list = new List<int>(60);
            for (int i = 0; i < 60; ++i)
                list.Add(i);
            return list;
        }
    }

    public int DurationHours { get => Duration.Hours; set => Duration = new TimeSpan(value, Duration.Minutes, 0); }
    public int DurationMinutes { get => Duration.Minutes; set => Duration = new TimeSpan(Duration.Hours, value, 0); }

    public FoodCategory[] FoodCategories => Enum.GetValues<FoodCategory>();
    public Recipe Recipe { get; set; }

    public string Name 
    { 
        get => Recipe.Name; 
        set
        {
            if(Recipe.Name != value)
            {
                Recipe.Name = value;
                OnPropertyChanged();
            }
        }
    }
    public TimeSpan Duration
    {
        get => Recipe.Duration;
        set
        {
            if (Recipe.Duration != value)
            {
                Recipe.Duration = value;
                OnPropertyChanged();
            }
        }
    }
    public uint People
    {
        get => Recipe.People;
        set
        {
            if (Recipe.People != value)
            {
                Recipe.People = value;
                OnPropertyChanged();
            }
        }
    }
    public string ShortDescription
    {
        get => Recipe.ShortDescription;
        set
        {
            if (Recipe.ShortDescription != value)
            {
                Recipe.ShortDescription = value;
                OnPropertyChanged();
            }
        }
    }
    public string Description
    {
        get => Recipe.Description;
        set
        {
            if (Recipe.Description != value)
            {
                Recipe.Description = value;
                OnPropertyChanged();
            }
        }
    }
    public string Keywords
    {
        get => Recipe.Keywords;
        set
        {
            if (Recipe.Keywords != value)
            {
                Recipe.Keywords = value;
                OnPropertyChanged();
            }
        }
    }
    public byte[] CoverImage
    {
        get => Recipe.CoverImage;
        set
        {
            if (Recipe.CoverImage != value)
            {
                Recipe.CoverImage = value;
                OnPropertyChanged();
            }
        }
    }
    public FoodCategory FoodCategory
    {
        get => Recipe.FoodCategory;
        set
        {
            if (Recipe.FoodCategory != value)
            {
                Recipe.FoodCategory = value;
                OnPropertyChanged();
            }
        }
    }
    public IList<RecipeStep> Steps
    {
        get => Recipe.Steps;
        set
        {
            if (Recipe.Steps != value)
            {
                Recipe.Steps = value;
                OnPropertyChanged();
            }
        }
    }
    public IList<Ingredient> Ingredients
    {
        get => Recipe.Ingredients;
        set
        {
            if (Recipe.Ingredients != value)
            {
                Recipe.Ingredients = value;
                OnPropertyChanged();
            }
        }
    }
}
