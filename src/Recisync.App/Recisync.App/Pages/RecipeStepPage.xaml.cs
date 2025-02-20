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

using Resisync.Core.Data;
using System.Windows.Input;

namespace Recisync.App.Pages;

public partial class RecipeStepPage : ContentPage
{
	public RecipeStepPage(IList<RecipeStep> steps)
	{
		InitializeComponent();
		m_steps = steps;
        
        BindingContext = this;
    }

	public RecipeStep Step => m_steps[m_stepIndex];

	public ICommand NextCommand => new Command(_ =>
	{
		++m_stepIndex;
		OnPropertyChanged(nameof(Step));
		OnPropertyChanged(nameof(NextCommand));
    }, _ => m_steps.Count > (m_stepIndex + 1));

    public ICommand PreviousCommand => new Command(_ =>
    {
        if (m_stepIndex == 0)
        {
            Navigation.PopAsync();
            return;
        }
        --m_stepIndex;
        OnPropertyChanged(nameof(Step));
        OnPropertyChanged(nameof(NextCommand));
    });

    int m_stepIndex = 0;
	IList<RecipeStep> m_steps;
}