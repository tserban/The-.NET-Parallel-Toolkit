﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "25E30E5ADE332E13463E1299E2BC9832"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Thread_Profiler {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 26 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBlock processorsLabel;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBlock coresLabel;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBlock logicalProcessorsLabel;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBlock htLabel;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBlock nameLabel;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBox browseTextBox;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button browseButton;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBox argsTextBox;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button samplingButton;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBox durationTextBox;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.ListView threadListView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Thread_Profiler;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.processorsLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.coresLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.logicalProcessorsLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.htLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.nameLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.browseTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.browseButton = ((System.Windows.Controls.Button)(target));
            
            #line 51 "..\..\MainWindow.xaml"
            this.browseButton.Click += new System.Windows.RoutedEventHandler(this.browseButton_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.argsTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.samplingButton = ((System.Windows.Controls.Button)(target));
            
            #line 64 "..\..\MainWindow.xaml"
            this.samplingButton.Click += new System.Windows.RoutedEventHandler(this.samplingButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.durationTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.threadListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 12:
            
            #line 84 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}