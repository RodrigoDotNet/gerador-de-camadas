using System;
using System.Collections;
using System.Windows.Forms;
using DataDrain.Library.ExtensionMethods;

namespace DataDrain.UI.WinForm.Control
{
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            SortColumn = 0;

            // Initialize the sort order to 'none'
            Order = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            // Cast the objects to be compared to ListViewItem objects
            var listviewX = (ListViewItem)x;
            var listviewY = (ListViewItem)y;

            // Compare the two items

            int compareResult = ObjectCompare.Compare(
                    Convert.ChangeType(listviewX.SubItems[SortColumn].Text, listviewX.SubItems[SortColumn].Text.RetornaTipo()),
                    Convert.ChangeType(listviewY.SubItems[SortColumn].Text, listviewX.SubItems[SortColumn].Text.RetornaTipo())
                );

            //if (listviewX.SubItems[_columnToSort].Text.ToInt32()>0)
            //{
            //    compareResult = ObjectCompare.Compare(listviewX.SubItems[_columnToSort].Text.ToInt32(), listviewY.SubItems[_columnToSort].Text.ToInt32());
            //}
            //else
            //{
            //    compareResult = ObjectCompare.Compare(listviewX.SubItems[_columnToSort].Text, listviewY.SubItems[_columnToSort].Text);                
            //}

            // Calculate correct return value based on object comparison
            if (Order == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (Order == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn { set; get; }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order { set; get; }
    }
}
