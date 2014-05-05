using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Models;
using Dev2.Utils;
using Microsoft.CSharp.RuntimeBinder;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Sequence
{
    public class SequenceDesignerViewModel : ActivityDesignerViewModel
    {
        object _smallViewItem;

        public SequenceDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            AddTitleBarLargeToggle();
        }

        public object SmallViewItem
        {
            get
            {
                return _smallViewItem;
            }
            // ReSharper disable ValueParameterNotUsed
            set
            // ReSharper restore ValueParameterNotUsed
            {
                _smallViewItem = null;
            }
        }

        public List<String> ActivityNames
        {
            get
            {
                var property = ModelItem.GetProperty("Activities");
                var activityNames = property as Collection<Activity>;
                if(activityNames != null)
                {
                    var fullListOfNames = activityNames.Select(item => item.DisplayName).ToList();
                    if(fullListOfNames.Count() <= 4)
                    {
                        return fullListOfNames.ToList();
                    }
                    var limitedList = fullListOfNames.Take(4).ToList();
                    limitedList.Add("...");
                    return limitedList;
                }
                return new List<string>();
            }
        }

        public void SetModelItemForServiceTypes(IDataObject dataObject)
        {
            if(dataObject != null && dataObject.GetDataPresent(GlobalConstants.ResourceTreeViewModelFormat))
            {
                dynamic resourceTreeViewModel = dataObject.GetData(GlobalConstants.ResourceTreeViewModelFormat);
                try
                {
                    var resource = resourceTreeViewModel.DataContext as ResourceModel;
                    if(resource != null)
                    {
                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true);
                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                        d.IconPath = resource.IconPath;
                        WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource);
                        ModelItem modelItem = ModelItemUtils.CreateModelItem(d);
                        if(modelItem != null)
                        {
                            dataObject.SetData(DragDropHelper.ModelItemDataFormat, modelItem);
                        }
                    }
                }
                catch(RuntimeBinderException e)
                {
                    this.LogError(e);
                }
            }
        }


        public bool DoDrop(IDataObject dataObject)
        {
            var formats = dataObject.GetFormats();
            if(!formats.Any())
            {
                return false;
            }
            dynamic mi = ModelItem;
            ModelItemCollection activitiesCollection = mi.Activities;
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
            if(!String.IsNullOrEmpty(modelItemString))
            {
                var objectData = dataObject.GetData(modelItemString);
                var data = objectData as List<ModelItem>;
                if(data != null && data.Count > 1)
                {
                    foreach(var item in data)
                    {
                        activitiesCollection.Insert(activitiesCollection.Count, item);
                    }
                }
                return true;
            }
            modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat", StringComparison.Ordinal) >= 0);
            if(String.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);
                if(String.IsNullOrEmpty(modelItemString))
                {
                    return false;
                }
            }
            ModelItem modelItem = CreateModelItemFromDataObject(dataObject, modelItemString);
            if(modelItem != null)
            {
                activitiesCollection.Insert(activitiesCollection.Count, modelItem);
            }

            return true;
        }

        ModelItem CreateModelItemFromDataObject(IDataObject dataObject, string modelItemString)
        {
            var objectData = dataObject.GetData(modelItemString);
            var data = objectData as ModelItem;
            Type type = null;
            if(data != null)
            {
                return data;
            }
            var stringValue = objectData as string;
            if(stringValue != null)
            {
                type = Type.GetType(stringValue);
            }
            return type == null ? null : ModelItemUtils.CreateModelItem(Activator.CreateInstance(type) as Activity);
        }

        public override void Validate()
        {
        }
    }
}