﻿using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ActivityDropWindowUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.FormatNumberUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServerWizardClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SwitchUIMapClasses;

namespace Dev2.Studio.UI.Tests
{
    // Use this class with all new UI test classes
    // Please add reference to any maps used to this class
    public abstract class UIMapBase
    {
        #region All UI Maps

        #region ActivityDropUIMap

        public ActivityDropWindowUIMap ActivityDropUIMap
        {
            get
            {
                if(_activityDropUIMap == null)
                {
                    _activityDropUIMap = new ActivityDropWindowUIMap();
                }
                return _activityDropUIMap;
            }

        }

        private ActivityDropWindowUIMap _activityDropUIMap;

        #endregion ActivityDropUIMap

        #region ConnectViewUIMap

        public ServerWizard ConnectViewUIMap
        {
            get
            {
                if (_connectViewUIMap == null)
                {
                    _connectViewUIMap = new ServerWizard();
                }
                return _connectViewUIMap;
            }

        }

        private ServerWizard _connectViewUIMap;

        #endregion ConnectViewUIMap

        #region Database Service Wizard UI Map

        public DatabaseServiceWizardUIMap DatabaseServiceWizardUIMap
        {
            get {
                if (_databaseServiceWizardUIMap == null) {
                    _databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return _databaseServiceWizardUIMap;
            }
        }


        private DatabaseServiceWizardUIMap _databaseServiceWizardUIMap;

        #endregion Database Service Wizard UI Map

        #region Database Source Wizard UI Map

        public DatabaseSourceUIMap DatabaseSourceUIMap
        {
            get
            {
                if(_databaseSourceWizardUIMap == null)
                {
                    _databaseSourceWizardUIMap = new DatabaseSourceUIMap();
                }

                return _databaseSourceWizardUIMap;
            }
        }


        private DatabaseSourceUIMap _databaseSourceWizardUIMap;

        #endregion Database Source Wizard UI Map

        #region Debug UI Map

        public DebugUIMap DebugUIMap
        {
            get
            {
                if (_debugUIMap == null)
                {
                    _debugUIMap = new DebugUIMap();
                }
                return _debugUIMap;
            }
        }

        private DebugUIMap _debugUIMap;

        #endregion Debug UI Map

        #region DecisionWizard UI Map

        public DecisionWizardUIMap DecisionWizardUIMap
        {
            get
            {
                if(_decisionWizardUIMap == null)
                {
                    _decisionWizardUIMap = new DecisionWizardUIMap();
                }
                return _decisionWizardUIMap;
            }
        }

        private DecisionWizardUIMap _decisionWizardUIMap;

        #endregion DecisionWizard UI Map

        #region Dependancy Graph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if (_dependancyGraphUIMap == null)
                {
                    _dependancyGraphUIMap = new DependencyGraph();
                }
                return _dependancyGraphUIMap;
            }
        }

        private DependencyGraph _dependancyGraphUIMap;

        #endregion Dependancy Graph UI Map

        #region Deploy UI Map

        public DeployViewUIMap DeployUIMap
        {
            get
            {
                if (_deployUIMap == null)
                {
                    _deployUIMap = new DeployViewUIMap();
                }
                return _deployUIMap;
            }
        }


        private DeployViewUIMap _deployUIMap;

        #endregion Deploy UI Map

        #region Dock Manager UI Map

        public DocManagerUIMap DockManagerUIMap
        {
            get
            {
                if (_dockManagerUIMap == null)
                {
                    _dockManagerUIMap = new DocManagerUIMap();
                }
                return _dockManagerUIMap;
            }
        }

        private DocManagerUIMap _dockManagerUIMap;

        #endregion Dock Manager UI Map

        #region Email Source Wizard UI Map

        public EmailSourceWizardUIMap EmailSourceWizardUIMap
        {
            get
            {
                if(_emailSourceWizardUIMap == null)
                {
                    _emailSourceWizardUIMap = new EmailSourceWizardUIMap();
                }
                return _emailSourceWizardUIMap;
            }
        }

        private EmailSourceWizardUIMap _emailSourceWizardUIMap;

        #endregion EmailSourceWizard UI Map

        #region Explorer UI Map

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_explorerUIMap == null)
                {
                    _explorerUIMap = new ExplorerUIMap();
                }
                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

        #endregion Explorer UI Map

        #region External UI Map

        public ExternalUIMap ExternalUIMap
        {
            get
            {
                if (_externalUIMap == null)
                {
                    _externalUIMap = new ExternalUIMap();
                }
                return _externalUIMap;
            }
        }

        private ExternalUIMap _externalUIMap;

        #endregion External UI Map

        #region Feedback UI Map

        public FeedbackUIMap FeedbackUIMap
        {
            get
            {
                if (_feedbackUIMap == null)
                {
                    _feedbackUIMap = new FeedbackUIMap();
                }
                return _feedbackUIMap;
            }
        }

        private FeedbackUIMap _feedbackUIMap;

        #endregion Feedback UI Map

        #region Tools UI Maps

        #region Format Number UI Map

        public FormatNumberUIMap FormatNumberUIMap
        {
            get
            {
                if (_formatNumberUIMap == null)
                {
                    _formatNumberUIMap = new FormatNumberUIMap();
                }

                return _formatNumberUIMap;
            }
        }

        private FormatNumberUIMap _formatNumberUIMap;

        #endregion Format Number UI Map

        #endregion Tools UI Maps

        #region New Server UI Map

        public NewServerUIMap NewServerUIMap
        {
            get {
                if (_newServerUIMap == null)
                    _newServerUIMap = new NewServerUIMap();
                return _newServerUIMap;
            }
        }

        private NewServerUIMap _newServerUIMap;

        #endregion New Server UI Map

        #region Output UI Map

        public OutputUIMap OutputUIMap
        {
            get
            {
                if(_outputUIMap == null)
                    _outputUIMap = new OutputUIMap();
                return _outputUIMap;
            }
        }

        private OutputUIMap _outputUIMap;

        #endregion New Server UI Map

        #region Plugin Service Wizard UI Map

        public PluginServiceWizardUIMap PluginServiceWizardUIMap
        {
            get
            {
                if (_pluginServiceWizardUIMap == null)
                    _pluginServiceWizardUIMap = new PluginServiceWizardUIMap();
                return _pluginServiceWizardUIMap;
            }
        }

        private PluginServiceWizardUIMap _pluginServiceWizardUIMap;

        #endregion Plugin Service Wizard UI Map

        #region Plugin Source Wizard UI Map

        public PluginSourceMap PluginSourceMap
        {
            get
            {
                if(_pluginSourceWizardUIMap == null)
                    _pluginSourceWizardUIMap = new PluginSourceMap();
                return _pluginSourceWizardUIMap;
            }
        }

        private PluginSourceMap _pluginSourceWizardUIMap;

        #endregion Plugin Source Wizard UI Map

        #region ResourceChangedPopUp UI Map

        public ResourceChangedPopUpUIMap ResourceChangedPopUpUIMap
        {
            get
            {
                if(_resourceChangedPopUpUIMap == null)
                    _resourceChangedPopUpUIMap = new ResourceChangedPopUpUIMap();
                return _resourceChangedPopUpUIMap;
            }


        }

        private ResourceChangedPopUpUIMap _resourceChangedPopUpUIMap;

        #endregion ResourceChangedPopUp UI Map

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get {
                if (_ribbonUIMap == null)
                    _ribbonUIMap = new RibbonUIMap();
                return _ribbonUIMap;
            }
            
                
        }

        private RibbonUIMap _ribbonUIMap;

        #endregion Ribbon UI Map

        #region Save Dialog UI Map

        public SaveDialogUIMap SaveDialogUIMap
        {
            get
            {
                if(_saveDialogWizardUIMap == null)
                    _saveDialogWizardUIMap = new SaveDialogUIMap();
                return _saveDialogWizardUIMap;
            }

        }

        private SaveDialogUIMap _saveDialogWizardUIMap;

        #endregion Service Details UI Map

        #region Service Details UI Map

        public ServiceDetailsUIMap ServiceDetailsWizardUIMap
        {
            get
            {
                if (_serviceDetailsWizardUIMap == null)
                    _serviceDetailsWizardUIMap = new ServiceDetailsUIMap();
                return _serviceDetailsWizardUIMap;
            }

        }

        private ServiceDetailsUIMap _serviceDetailsWizardUIMap;

        #endregion Service Details UI Map

        #region Studio Window UI Map

        public UIBusinessDesignStudioWindow StudioWindow
        {
            get
            {
                if(_studioWindowWizardUIMap == null)
                    _studioWindowWizardUIMap = new UIBusinessDesignStudioWindow();
                return _studioWindowWizardUIMap;
            }

        }

        private UIBusinessDesignStudioWindow _studioWindowWizardUIMap;

        #endregion Service Details UI Map

        #region Switch Wizard UI Map

        public SwitchWizardUIMap SwitchWizardUIMap
        {
            get
            {
                if(_switchWizardWizardUIMap == null)
                    _switchWizardWizardUIMap = new SwitchWizardUIMap();
                return _switchWizardWizardUIMap;
            }

        }

        private SwitchWizardUIMap _switchWizardWizardUIMap;

        #endregion Service Details UI Map

        #region Tab Manager UI Map

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (_tabManagerUIMap == null)
                    _tabManagerUIMap = new TabManagerUIMap();
                return _tabManagerUIMap;
            }
        }

        private TabManagerUIMap _tabManagerUIMap;

        #endregion Tab Manager UI Map

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if (_toolboxUIMap == null)
                    _toolboxUIMap = new ToolboxUIMap();
                return _toolboxUIMap;
            }

        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion Toolbox UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
        {
            get
            {
                if (_variablesUIMap == null)
                    _variablesUIMap = new VariablesUIMap();
                return _variablesUIMap;
            }
        }

        public VariablesUIMap _variablesUIMap;

        #endregion Variables UI Map

        #region Webpage Service Wizard UI Map

        public WebpageServiceWizardUIMap WebpageServiceWizardUIMap
        {
            get {
                if (_webpageServiceWizardUIMap == null)
                    _webpageServiceWizardUIMap = new WebpageServiceWizardUIMap();
                return _webpageServiceWizardUIMap;
            }
        }

        private WebpageServiceWizardUIMap _webpageServiceWizardUIMap;

        #endregion Webpage Service Wizard UI Map

        #region Workflow Designer UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (_workflowDesignerUIMap == null)
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion Workflow Designer UI Map

        #region Workflow Wizard UI Map

        public WorkflowWizardUIMap WorkflowWizardUIMap
        {
            get
            {
                if (_workflowWizardUIMap == null)
                    _workflowWizardUIMap = new WorkflowWizardUIMap();
                return _workflowWizardUIMap;
            }
        }

        private WorkflowWizardUIMap _workflowWizardUIMap;

        #endregion Workflow Wizard UI Map

        #endregion All UI Maps
    }
}
