using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace Infragistics
{
    #region ICollectionBase

    /// <summary>
    /// A base collection interface that provides hooks for derived classes to override base functionality.
    /// </summary>
    public interface ICollectionBase : IList, ICollection, IEnumerable, INotifyCollectionChanged, IDisposable
    {
        /// <summary>
        /// Adds the item at the specified index, without triggering any events. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        void AddItemSilently(int index, object item);

        /// <summary>
        /// Removes the item at the specified index, without triggering any events. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        bool RemoveItemSilently(int index);

        /// <summary>
        /// Removes all items from the collection without firing any events.
        /// </summary>
        void ResetItemsSilently();
    }

    /// <summary>
    /// A base collection interface that provides hooks for derived classes to override base functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionBase<T> : ICollectionBase, IList<T>, ICollection<T>, IEnumerable<T>
    {
        /// <summary>
        /// Adds the item at the specified index, without triggering any events. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        void AddItemSilently(int index, T item);
    }
    #endregion // ICollectionBase


}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved