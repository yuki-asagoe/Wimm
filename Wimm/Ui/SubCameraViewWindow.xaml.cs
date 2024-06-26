﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    /// <summary>
    /// SubCameraViewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubCameraViewWindow : MetroWindow
    {
        internal SubCameraViewWindow(MachineControlViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
