using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Views.ResourceManagement;
using System;
using System.Collections.Generic;

namespace Dev2.Utils
{
    public class ResourceChangeHandler
    {
        readonly IEventAggregator _eventPublisher;
        public ResourceChangeHandler(IEventAggregator eventPublisher)
        {
            if(eventPublisher == null)
            {
                throw new ArgumentNullException("eventPublisher");
            }
            _eventPublisher = eventPublisher;
        }

        public void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants, IResourceChangedDialog resourceChangedDialog = null)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            if(numberOfDependants == null)
            {
                throw new ArgumentNullException("numberOfDependants");
            }
            if(resourceChangedDialog == null)
            {
                resourceChangedDialog = new ResourceChangedDialog(resource, numberOfDependants.Count, StringResources.MappingChangedWarningDialogTitle);
            }
            resourceChangedDialog.ShowDialog();
            if(resourceChangedDialog.OpenDependencyGraph)
            {
                if(numberOfDependants.Count == 1)
                {
                    var resourceModel = resource.Environment.ResourceRepository.FindSingle(model => model.ResourceName == numberOfDependants[0]);
                    if(resourceModel != null)
                    {
                        WorkflowDesignerUtils.EditResource(resourceModel, _eventPublisher);
                    }
                }
                else
                {
                    Logger.TraceInfo("Publish message of type - " + typeof(ShowReverseDependencyVisualizer));
                    _eventPublisher.Publish(new ShowReverseDependencyVisualizer(resource));
                }
            }
        }
    }
}