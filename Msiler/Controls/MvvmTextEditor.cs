﻿using ICSharpCode.AvalonEdit;
using System;
using System.ComponentModel;
using System.Windows;
// ReSharper disable UnusedMember.Global

namespace Msiler.Controls
{
    public class MvvmTextEditor : TextEditor, INotifyPropertyChanged
    {
        public new string Text {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MvvmTextEditor), new PropertyMetadata((obj, args) => {
                var target = (MvvmTextEditor)obj;
                target.Text = (string)args.NewValue;
            }));

        protected override void OnTextChanged(EventArgs e) {
            this.RaisePropertyChanged("Text");
            base.OnTextChanged(e);
        }

        public void RaisePropertyChanged(string property) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
