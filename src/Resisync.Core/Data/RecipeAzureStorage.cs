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

using Azure.Storage.Blobs;
using Resisync.Core.Interfaces;
using Azure.Storage.Blobs.Models;
using Azure;
using Resisync.Core.Extensions;
using System.Diagnostics;

namespace Resisync.Core.Data;

public class RecipeAzureStorage : IRecipeService
{
    public bool IsConnected { get; private set; }

    public async Task<bool> LoginAsync(string user, string key)
    {
        try
        {
            m_client = new BlobContainerClient(
                new Uri($"https://rsrecipes.blob.core.windows.net/{user}?{key}")
            );
            IsConnected = await m_client.ExistsAsync();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            IsConnected = false;
        }
        return IsConnected;
    }

    public async Task<bool> DeleteAsync(Recipe recipe)
    {
        if (m_client != null) try
        {
            var response = await m_client.DeleteBlobIfExistsAsync(recipe.CreateFileName());
            return response.HasValue && response.Value;
        }
        catch(Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return false;
    }

    public async Task<IList<Recipe>?> GetMissingRecipesAsync(string downloadFolder, IList<Recipe>? localRecipes)
    {
        if (m_client == null) // Not connected
            return null;

        var recipes = new List<Recipe>(localRecipes ?? []);
        Dictionary<int, string> indexDestinationPair = [];
        IList<string> unknownRecipeDestinations = [];
        try // Download all recipes that are not available locally, changed or upload newer ones.
        {
            await foreach (BlobItem blob in m_client.GetBlobsAsync(BlobTraits.Metadata)) 
            {
                int ri = recipes.IndexOf(r => r.CreateFileName() == blob.Name, out Recipe? r);
                if (!blob.Metadata.TryGetValue(TimestampKey, out string? timestamp))
                    continue;

                string destination = Path.Combine(downloadFolder, blob.Name);
                if (r != null) // Already known
                {
                    bool parsed = DateTime.TryParse(timestamp, out DateTime dt);
                    if (!parsed || dt < r.Date) // Something is wrong with the remote file or Local is newer
                    {
                        await AddOrChangeRecipeAsync(recipes[ri]);
                        continue;
                    }
                    else if (dt == r.Date) // Local and Remote are the same    
                    {
                        continue;
                    }
                    // else if (dt > r.Date) // Remote is newer

                    indexDestinationPair.Add(ri, destination);
                }
                else // Not known and has to be downloaded
                { 
                    unknownRecipeDestinations.Add(destination); 
                }
                // Start downloading to destination folder
                await m_client.GetBlobClient(blob.Name).DownloadToAsync(destination);
            }
            // Add or replace elements in recipe set
            foreach (var item in indexDestinationPair.OrderBy(o => o.Key)) // Replace 
            {
                Recipe r = Recipe.FromFile(item.Value) ?? throw new InvalidDataException("A recipe on the cloud must be deserializable.");
                recipes[item.Key] = r;
            }
            foreach (string dst in unknownRecipeDestinations)  // Add
            {
                Recipe r = Recipe.FromFile(dst) ?? throw new InvalidDataException("A recipe on the cloud must be deserializable.");
                recipes.Add(r);
            }
        }
        catch (RequestFailedException ex)
        {
            Trace.WriteLine($"Azure request failed {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Trace.WriteLine($"Error parsing files in the directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception: {ex.Message}");
        }

        if(localRecipes != null) // Upload recipes that are not found remotely but are available locally
        {
            foreach (Recipe recipe in recipes.Except(localRecipes, new RecipeComparer()))
                await AddOrChangeRecipeAsync(recipe);
        }

        return recipes;
    }

    public async Task<bool> AddOrChangeRecipeAsync(Recipe recipe)
    {
        if (m_client == null)
            return false;

        var client = m_client.GetBlobClient(recipe.CreateFileName());
        if (client == null)
            return false;

        try
        {
            await client.UploadAsync(BinaryData.FromBytes(Recipe.ToBytes(recipe)), overwrite: true);
            var metadata = new Dictionary<string, string>
            {
                { TimestampKey, recipe.Date.ToString("O") }
            };
            client.SetMetadata(metadata);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return true;
    }


    const string TimestampKey = "Timestamp";

    BlobContainerClient? m_client;
}
