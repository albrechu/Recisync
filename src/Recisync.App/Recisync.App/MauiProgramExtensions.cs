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

using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using Recisync.App.Pages;
using Recisync.App.ViewModels;
using Resisync.Core.Data;
using Resisync.Core.Interfaces;

namespace Recisync.App;

public static class MauiProgramExtensions
{
	public static MauiApp BuildSharedMauiApp(this MauiAppBuilder builder)
	{
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .AddServices()
            .ConfigureFonts(fonts =>
			{
				fonts
                .AddFont("SpaceGrotesk-Light.ttf",    "SpaceGroteskLight")
                .AddFont("SpaceGrotesk-Regular.ttf",  "SpaceGroteskRegular")
                .AddFont("SpaceGrotesk-Medium.ttf",   "SpaceGroteskMedium")
				.AddFont("SpaceGrotesk-Bold.ttf",     "SpaceGroteskBold")
                .AddFont("SpaceGrotesk-SemiBold.ttf", "SpaceGroteskSemiBold")
				.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular")
                ;
			});
#if DEBUG
        builder.Logging.AddDebug();
#endif
        MauiApp mauiApp = builder.Build();
		Ioc.Default.ConfigureServices(mauiApp.Services);

        return mauiApp;
	}

	private static MauiAppBuilder AddServices(this MauiAppBuilder builder)
	{
		builder.Services
			// Define ViewModels
			.AddSingleton<RecipesViewModel>()
			// Declare Pages
			.AddTransient<EditRecipePage>()
			.AddTransient<RecipeDetailPage>()
			.AddTransient<RecipesPage>()
			// Declare Popups
			.AddTransient<CloudPopup>()
			// Define Services
			.AddSingleton<IRecipeService, RecipeAzureStorage>()
			;
        return builder;
	}
}
