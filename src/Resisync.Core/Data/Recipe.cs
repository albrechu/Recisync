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

using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace Resisync.Core.Data;

public class Recipe
{
    [JsonProperty(PropertyName = "id")]
    public string            Id { get; set; }
    public string            Name { get; set; }
    public DateTime          Date {  get; set; }
    public uint              People {  get; set; }
    public TimeSpan          Duration { get; set; }
    public FoodCategory      FoodCategory {  get; set; }
    public string            ShortDescription { get; set; }
    public string            Description { get; set; }
    public byte[]            CoverImage { get; set; }
    public string            Keywords { get; set; }
    public IList<Ingredient> Ingredients { get; set; }
    public IList<RecipeStep> Steps { get; set; }

    public override string ToString() =>
        JsonConvert.SerializeObject(this);
    public string CreateFileName() => Id + ".recipe";

    public static Recipe? FromString(string serialized)
    {
        Recipe? recipe = null;
        try
        {
            recipe = JsonConvert.DeserializeObject<Recipe>(serialized) as Recipe;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return recipe; 
    }
    public static Recipe? FromBytes(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }
        return Recipe.FromString(Encoding.UTF8.GetString(mso.ToArray()));
    }
    public static byte[] ToBytes(Recipe recipe) 
    {
        var bytes = Encoding.UTF8.GetBytes(recipe.ToString());
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }
        return mso.ToArray();
    }
    public static void ToFile(Recipe recipe, string fileWithPath)
    {
        File.WriteAllBytes(fileWithPath, ToBytes(recipe));
    }

    public static Recipe? FromFile(string fileWithPath)
    {
        var bytes = File.ReadAllBytes(fileWithPath);
        return FromBytes(bytes);
    }
}
