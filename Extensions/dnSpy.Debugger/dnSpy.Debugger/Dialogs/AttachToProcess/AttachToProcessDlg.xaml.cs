﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Utilities;

namespace dnSpy.Debugger.Dialogs.AttachToProcess {
	sealed partial class AttachToProcessDlg : WindowBase {
		public AttachToProcessDlg() {
			InitializeComponent();
			DataContextChanged += (s, e) => {
				if (DataContext is AttachToProcessVM vm) {
					vm.PropertyChanged += AttachToProcessVM_PropertyChanged;
					vm.AllItems.CollectionChanged += AllItems_CollectionChanged;
				}
			};
			Loaded += OnLoaded;
			InputBindings.Add(new KeyBinding(new RelayCommand(a => FocusSearchtextBox()), Key.F, ModifierKeys.Control));
			InputBindings.Add(new KeyBinding(new RelayCommand(a => FocusSearchtextBox()), Key.E, ModifierKeys.Control));
		}

		void FocusSearchtextBox() {
			searchTextBox.Focus();
			searchTextBox.SelectAll();
		}

		void AllItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems == null)
				return;
			var vm = DataContext as AttachToProcessVM;
			if (vm == null || vm.AllItems.Count != 1)
				return;
			FocusListViewElement();
		}

		void FocusListViewElement() {
			var vm = DataContext as AttachToProcessVM;
			if (vm == null || vm.AllItems.Count == 0)
				return;
			var obj = vm.AllItems[0];
			listView.SelectedItem = obj;
			SetFocus(listView, obj, DispatcherPriority.Background);
		}

		void OnLoaded(object sender, RoutedEventArgs e) {
			listView.Focus();
			var vm = DataContext as AttachToProcessVM;
			if (listView.SelectedItem == null && vm != null && vm.AllItems.Count > 0)
				FocusListViewElement();
		}

		void AttachToProcessVM_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(AttachToProcessVM.IsRefreshing)) {
				// Make sure Refresh button gets updated
				CommandManager.InvalidateRequerySuggested();
			}
		}

		void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (!UIUtilities.IsLeftDoubleClick<ListViewItem>(listView, e))
				return;

			var vm = DataContext as ViewModelBase;
			if (vm == null || vm.HasError)
				return;
			okButton_Click(this, e);
		}

		static void SetFocus(Selector selector, object obj, DispatcherPriority prio) => selector.Dispatcher.BeginInvoke(prio, new Action(() => {
			if (selector.SelectedItem == obj) {
				if (selector.ItemContainerGenerator.ContainerFromItem(obj) is IInputElement item)
					item.Focus();
			}
		}));
	}
}