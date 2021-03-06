
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Win32.TaskScheduler
{
	/// <summary>
	/// Displays a <see cref="TaskCollection"/> in a <see cref="ListView"/> control. Mimics list in MMC.
	/// </summary>
	public partial class TaskListView : UserControl
	{
		private TaskCollection coll;

		/// <summary>
		/// Initializes a new instance of the <see cref="TaskListView"/> class.
		/// </summary>
		public TaskListView()
		{
			InitializeComponent();
			smallImageList.Images.Add(new System.Drawing.Icon(EditorProperties.Resources.ts, 0x10, 0x10));
		}

		/// <summary>
		/// Occurs when task selected in the list.
		/// </summary>
		public event EventHandler<TaskSelectedEventArgs> TaskSelected;

		/// <summary>
		/// Gets or sets the <see cref="T:System.Windows.Forms.ContextMenuStrip" /> associated with this control.
		/// </summary>
		/// <returns>The <see cref="T:System.Windows.Forms.ContextMenuStrip" /> for this control, or null if there is no <see cref="T:System.Windows.Forms.ContextMenuStrip" />. The default is null.</returns>
		public override ContextMenuStrip ContextMenuStrip
		{
			get { return listView1.ContextMenuStrip; }
			set { listView1.ContextMenuStrip = value; listView1.ContextMenuStrip.Opening += listView1ContextMenuStrip_Opening; }
		}

		/// <summary>
		/// Gets or sets the zero-based index of the currently selected item in a <see cref="TaskListView"/>.
		/// </summary>
		/// <value>
		/// A zero-based index of the currently selected item. A value of negative one (-1) is returned if no item is selected.
		/// </value>
		public int SelectedIndex
		{
			get
			{
				return listView1.SelectedIndices.Count == 0 ? -1 : listView1.SelectedIndices[0];
			}
			set
			{
				foreach (int i in listView1.SelectedIndices)
					listView1.Items[i].Selected = false;
				if (value != -1)
					listView1.Items[value].Selected = true;
			}
		}

		/// <summary>
		/// Gets or sets the tasks.
		/// </summary>
		/// <value>The tasks.</value>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), RefreshProperties(RefreshProperties.All)]
		public TaskCollection Tasks
		{
			get { return coll; }
			set
			{
				coll = value;
				listView1.Items.Clear();
				if (coll != null)
					foreach (var item in coll)
						listView1.Items.Add(LVIFromTask(item));
			}
		}

		/// <summary>
		/// Raises the <see cref="TaskSelected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Microsoft.Win32.TaskScheduler.TaskListView.TaskSelectedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnTaskSelected(TaskSelectedEventArgs e)
		{
			EventHandler<TaskSelectedEventArgs> handler = TaskSelected;
			if (handler != null)
				handler(this, e);
		}

		private void listView1ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			if (listView1.SelectedItems.Count <= 0)
				e.Cancel = true;
		}

		private void listView1_MouseClick(object sender, MouseEventArgs e)
		{
			if (this.ContextMenuStrip != null && e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				ListViewItem item = listView1.GetItemAt(e.X, e.Y);
				if (item != null)
				{
					item.Selected = true;
					this.ContextMenuStrip.Show(listView1, e.Location);
				}
			}
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Task t = null;
			if (listView1.SelectedIndices.Count == 1)
				t = coll[listView1.SelectedItems[0].Text];
			OnTaskSelected(new TaskSelectedEventArgs(t));
		}

		private ListViewItem LVIFromTask(Task task)
		{
			bool disabled = task.State == TaskState.Disabled;
			ListViewItem lvi = new ListViewItem(new string[] {
				task.Name,
				TaskEnumGlobalizer.GetString(task.State),
				task.Definition.Triggers.ToString(),
				disabled || task.NextRunTime < DateTime.Now ? string.Empty : task.NextRunTime.ToString("G"),
				task.LastRunTime == DateTime.MinValue ? EditorProperties.Resources.Never :  task.LastRunTime.ToString("G"),
				task.LastTaskResult == 0 ? EditorProperties.Resources.LastResultSuccessful : string.Format("(0x{0:X})", task.LastTaskResult),
				task.Definition.RegistrationInfo.Author,
				string.Empty
				}, 0);
			return lvi;
		}

		/// <summary>
		/// Event args for when a task is selected.
		/// </summary>
		public class TaskSelectedEventArgs : EventArgs
		{
			/// <summary>
			/// Empty <see cref="TaskSelectedEventArgs"/> class.
			/// </summary>
			public static new readonly TaskSelectedEventArgs Empty = new TaskSelectedEventArgs();

			/// <summary>
			/// Gets or sets the task.
			/// </summary>
			/// <value>The task.</value>
			public Task Task { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="TaskSelectedEventArgs"/> class.
			/// </summary>
			/// <param name="task">The task.</param>
			public TaskSelectedEventArgs(Task task = null)
			{
				this.Task = task;
			}
		}
	}
}
