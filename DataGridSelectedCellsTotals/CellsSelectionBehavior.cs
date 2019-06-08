using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace DataGridSelectedCellsTotals
{
    public class CellsSelectionBehavior : Behavior<DataGrid>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICollection<object> Values
        {
            get { return (ICollection<object>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(ICollection<object>), typeof(CellsSelectionBehavior), new PropertyMetadata(Array.Empty<object>()));

        public double? Sum => Values?.Sum(value => ExtractDouble(value));
        public double? Max => Values?.Max(value => ExtractDouble(value));
        public double? Min => Values?.Min(value => ExtractDouble(value));
        public double? Average => Values?.Average(value => ExtractDouble(value));
        public int? Count => Values?.Count();
        public int? CountNotNull => Values?.Count(v=>v != null);


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedCellsChanged += this.AssociatedObject_SelectedCellsChanged;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectedCellsChanged -= this.AssociatedObject_SelectedCellsChanged;
        }
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void AssociatedObject_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            var Values = dg.SelectedCells.Select(cell => GetValueFromClipBoard(cell));
            this.SetValue(ValuesProperty, Values.ToArray());
            OnPropertyChanged("Sum");
            OnPropertyChanged("Max");
            OnPropertyChanged("Min");
            OnPropertyChanged("Average");
            OnPropertyChanged("Count");
            OnPropertyChanged("CountNotNull");

        }
        private static object GetValueFromClipBoard(DataGridCellInfo cellInfo)
        {
            string path = (cellInfo.Column.ClipboardContentBinding as Binding)?.Path?.Path;
            object item = cellInfo.Item;
            var pathParts = path?.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            pathParts?.ForEach(p =>
            {
                var itype = item.GetType();
                var property = itype.GetProperty(p);
                item = property.GetValue(item);

            });
            return item;
        }

        private static double? ExtractDouble(object o)
        {
            if (o is double || o is double? || o == null)
                return (double?)o;
            double temp = 0; 
            bool isSuccess = double.TryParse(o.ToString(), out temp);
            return isSuccess ? temp : (double?)null;
        }
    }
}
