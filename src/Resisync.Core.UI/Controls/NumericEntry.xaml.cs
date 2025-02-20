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

namespace Resisync.Core.UI.Controls;

public partial class NumericEntry : ContentView
{
    /// <summary>
    /// Current Numeric Value
    /// </summary>
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value),
            typeof(float),
            typeof(NumericEntry),
            0f);
    public float Value
    {
        get => (float)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Stepping Value
    /// </summary>
    public static readonly BindableProperty StepProperty =
        BindableProperty.Create(nameof(Step),
            typeof(float),
            typeof(NumericEntry),
            1f);
    public float Step
    {
        get => (float)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Minimum Value
    /// </summary>
    public static readonly BindableProperty ValueMinimumProperty =
        BindableProperty.Create(nameof(ValueMinimum),
            typeof(float),
            typeof(NumericEntry),
            float.MinValue);
    public float ValueMinimum
    {
        get => (float)GetValue(ValueMinimumProperty);
        set => SetValue(ValueMinimumProperty, value);
    }

    /// <summary>
    /// Maximum Value
    /// </summary>
    public static readonly BindableProperty ValueMaximumProperty =
        BindableProperty.Create(nameof(ValueMaximum),
            typeof(float),
            typeof(NumericEntry),
            float.MaxValue);
    public float ValueMaximum
    {
        get => (float)GetValue(ValueMaximumProperty);
        set => SetValue(ValueMaximumProperty, value);
    }

    /// <summary>
    /// Constructores
    /// </summary>
    public NumericEntry() =>
		InitializeComponent();

    public ICommand AddCommand => new Command(() => Value = Math.Clamp(Value + Step, ValueMinimum, ValueMaximum));
    public ICommand SubCommand => new Command(() => Value = Math.Clamp(Value - Step, ValueMinimum, ValueMaximum));
}