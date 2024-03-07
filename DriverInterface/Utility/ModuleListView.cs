using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KsDumper11.Utility
{
    public class ModuleListView : ListView
    {

        public ModuleListView()
        {
            base.OwnerDraw = true;
            this.DoubleBuffered = true;
            base.Sorting = SortOrder.Ascending;
        }

        public void LoadModules(ModuleSummary[] moduleSummaries)
        {
            this.moduleCache = moduleSummaries;
            this.ReloadItems();
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                using (Font headerFont = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold))
                {
                    e.Graphics.FillRectangle(new SolidBrush(this.BackColor), e.Bounds);
                    e.Graphics.DrawString(e.Header.Text, headerFont, new SolidBrush(this.ForeColor), e.Bounds, sf);
                }
            }
        }

        private void ReloadItems()
        {
            base.BeginUpdate();
            int idx = 0;
            bool flag = base.SelectedIndices.Count > 0;
            if (flag)
            {
                idx = base.SelectedIndices[0];
                bool flag2 = idx == -1;
                if (flag2)
                {
                    idx = 0;
                }
            }
            base.Items.Clear();
            string systemRootFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToLower();
            foreach (ModuleSummary moduleSummary in this.moduleCache)
            {
                    ListViewItem lvi = new ListViewItem(string.Format("0x{0:x8}", moduleSummary.ModuleBase));
                    lvi.BackColor = this.BackColor;
                    lvi.ForeColor = this.ForeColor;
                    lvi.SubItems.Add(moduleSummary.ModuleFileName);
                    lvi.SubItems.Add(string.Format("0x{0:x8}", moduleSummary.ModuleEntryPoint));
                    lvi.SubItems.Add(string.Format("0x{0:x4}", moduleSummary.ModuleImageSize));
                    lvi.SubItems.Add(moduleSummary.IsWOW64 ? "x86" : "x64");
                    lvi.Tag = moduleSummary;
                    base.Items.Add(lvi);
            }
            base.ListViewItemSorter = new ModuleListViewItemComparer(this.sortColumnIndex, base.Sorting);
            base.Sort();
            base.Items[idx].Selected = true;
            base.EndUpdate();
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            bool flag = e.Column != this.sortColumnIndex;
            if (flag)
            {
                this.sortColumnIndex = e.Column;
                base.Sorting = SortOrder.Ascending;
            }
            else
            {
                bool flag2 = base.Sorting == SortOrder.Ascending;
                if (flag2)
                {
                    base.Sorting = SortOrder.Descending;
                }
                else
                {
                    base.Sorting = SortOrder.Ascending;
                }
            }
            base.ListViewItemSorter = new ModuleListView.ModuleListViewItemComparer(e.Column, base.Sorting);
            base.Sort();
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = m.Msg == 1;
            if (flag)
            {
            }
            base.WndProc(ref m);
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private int sortColumnIndex = 1;

        private ModuleSummary[] moduleCache;

        private class ModuleListViewItemComparer : IComparer
        {
            public ModuleListViewItemComparer(int columnIndex, SortOrder sortOrder)
            {
                this.columnIndex = columnIndex;
                this.sortOrder = sortOrder;
            }

            public int Compare(object x, object y)
            {
                if ((x is ListViewItem) && (y is ListViewItem))
                {
                    ModuleSummary p1 = ((ListViewItem)x).Tag as ModuleSummary;
                    ModuleSummary p2 = ((ListViewItem)y).Tag as ModuleSummary;

                    if (!(p1 == null || p2 == null))
                    {
                        int result = 0;

                        switch (columnIndex)
                        {
                            case 0:
                                result = p1.ModuleBase.CompareTo(p2.ModuleBase);
                                break;
                            case 1:
                                result = p1.ModuleFileName.CompareTo(p2.ModuleFileName);
                                break;
                            case 2:
                                result = p1.ModuleEntryPoint.CompareTo(p2.ModuleEntryPoint);
                                break;
                            case 3:
                                result = p1.ModuleImageSize.CompareTo(p2.ModuleImageSize);
                                break;
                            case 4:
                                result = p1.IsWOW64.CompareTo(p2.IsWOW64);
                                break;
                        }

                        if (sortOrder == SortOrder.Descending)
                        {
                            result = -result;
                        }
                        return result;
                    }
                }
                return 0;
            }

            private readonly int columnIndex;

            private readonly SortOrder sortOrder;
        }
    }
}
