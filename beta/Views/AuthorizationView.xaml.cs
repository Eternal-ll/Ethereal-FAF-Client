﻿using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : UserControl
    {
        public AuthorizationView(AuthorizationViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
