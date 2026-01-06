// Copyright (C)2026 Nick Kastellanos

using System.Collections;
using System.Windows.Forms;

namespace Content.Pipeline.Editor
{
    internal class TreeViewNodeSorter : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            TreeNode a = (TreeNode)x;
            TreeNode b = (TreeNode)y;

            if (a.Tag != null && b.Tag != null)
            {
                if ((a.Tag is FolderItem) && !(b.Tag is FolderItem))
                    return -1;
                if (!(a.Tag is FolderItem) && (b.Tag is FolderItem))
                    return +1;
            }

            return a.Text.CompareTo(b.Text);
        }
    }
}
