﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2FCC330E933F68A51727B4DF0EB7C683"
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


namespace Parallel_Advisor {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 34 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBox browseTextBox;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button browseButton;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button analyzeButton;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.CheckBox detailsCheckbox;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.ListBox outputListBox;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.TextBox convertTextBox;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button browseButtonBottom;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button convertButton;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\MainWindow.xaml"
        internal System.Windows.Controls.CheckBox comparisonCheckbox;
        
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
            System.Uri resourceLocater = new System.Uri("/Parallel_Advisor;component/mainwindow.xaml", System.UriKind.Relative);
            
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
            this.browseTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.browseButton = ((System.Windows.Controls.Button)(target));
            
            #line 35 "..\..\MainWindow.xaml"
            this.browseButton.Click += new System.Windows.RoutedEventHandler(this.browseButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.analyzeButton = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\MainWindow.xaml"
            this.analyzeButton.Click += new System.Windows.RoutedEventHandler(this.analyzeButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.detailsCheckbox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 5:
            this.outputListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            case 6:
            
            #line 55 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.convertTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.browseButtonBottom = ((System.Windows.Controls.Button)(target));
            
            #line 73 "..\..\MainWindow.xaml"
            this.browseButtonBottom.Click += new System.Windows.RoutedEventHandler(this.browseButtonBottom_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.convertButton = ((System.Windows.Controls.Button)(target));
            
            #line 74 "..\..\MainWindow.xaml"
            this.convertButton.Click += new System.Windows.RoutedEventHandler(this.convertButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.comparisonCheckbox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
