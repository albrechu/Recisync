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

using CommunityToolkit.Mvvm.Messaging;
using Recisync.App.Messages;
using Resisync.Core.Interfaces;
using System.Windows.Input;

namespace Recisync.App.Pages;

public partial class CloudPopup : CommunityToolkit.Maui.Views.Popup
{
    public CloudPopup(IRecipeService recipeService)
    {
        InitializeComponent();
        BindingContext = this;
        m_recipeService = recipeService;
        m_isConnected = recipeService.IsConnected;

        Task.Run(async () =>
        {
            string? endpoint = await SecureStorage.GetAsync("user-endpoint");
            if (!string.IsNullOrEmpty(endpoint))
                Endpoint = endpoint;
        });
    }

    public string Endpoint          
    {  
        get => m_endpoint;
        set
        {
            if (m_endpoint != value)
            {
                m_endpoint = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginCommand));
            }
        }
    }
    public string AuthenticationKey 
    {  
        get => m_authenticationKey;
        set
        {
            if (m_authenticationKey != value)
            {
                m_authenticationKey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginCommand));
            }
        }
    }
    
    public bool HideKey 
    {  
        get => m_hideKey;
        set 
        {
            if (m_hideKey != value)
            {
                m_hideKey = value;
                OnPropertyChanged();
            }
        } 
    }
    
    public ICommand ToggleKeyVisibility => new Command(() => HideKey = !HideKey);
    public ICommand LoginCommand => new Command(async () => {
        IsConnecting = true;
        bool success = await m_recipeService.LoginAsync(Endpoint, AuthenticationKey);
        IsConnecting = false;
        IsConnected  = success;
        if (success) 
        {
            await SecureStorage.SetAsync("user-endpoint", Endpoint);
            await SecureStorage.SetAsync("user-auth", AuthenticationKey);
        }
        WeakReferenceMessenger.Default.Send(new ConnectionMessage(success));
    }, () => !string.IsNullOrEmpty(AuthenticationKey) && !string.IsNullOrEmpty(Endpoint));

    public bool IsConnecting
    {
        get => m_isConnecting;
        set
        {
            if (m_isConnecting != value)
            {
                m_isConnecting = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsConnected
    {
        get => m_isConnected;
        set
        {
            if (m_isConnected != value)
            {
                m_isConnected = value;
                OnPropertyChanged();
            }
        }
    }

    bool m_isConnecting = false;
    bool m_isConnected = false;

    string m_endpoint;
    string m_authenticationKey;
    bool   m_hideKey           = true;
    IRecipeService m_recipeService;

    private void OnClose(object sender, EventArgs e)
    {
        Close();
    }
}