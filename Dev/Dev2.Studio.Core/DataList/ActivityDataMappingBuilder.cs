﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.DataList
{
    /// <summary>
    /// Used to perform fuzzy matching of sorts ;)
    /// NOTE : Commented out code for future extention on this piece
    /// </summary>
    class FuzzyMatchVO
    {
        internal IDictionary<string, string> _recordsetColumnsToName; 

        internal FuzzyMatchVO(IDictionary<string, string> matches)
        {
            _recordsetColumnsToName = matches;
        }

        /// <summary>
        /// Fetches the match.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        internal string FetchMatch(string token)
        {
            string result;

            _recordsetColumnsToName.TryGetValue(token, out result);

            return result;
        }
    }

    /// <summary>
    /// Used to return the mapping view models ;)
    /// </summary>
    public class MappingViewModelTO
    {
        public IList<IInputOutputViewModel> Inputs { get; private set; }

        public IList<IInputOutputViewModel> Outputs { get; private set; }

        public MappingViewModelTO(IList<IInputOutputViewModel> inputs, IList<IInputOutputViewModel> outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }

    /// <summary>
    /// Used to generate the data mapping view model data via the builder pattern ;)
    /// </summary>
    public class ActivityDataMappingBuilder
    {

        /// <summary>
        /// Gets or sets the data list.
        /// </summary>
        /// <value>
        /// The data list.
        /// </value>
        public string DataList { get; set; }

        /// <summary>
        /// Gets or sets the saved input mapping.
        /// </summary>
        /// <value>
        /// The saved input mapping.
        /// </value>
        public string SavedInputMapping { get; private set; }

        /// <summary>
        /// Gets or sets the saved output mapping.
        /// </summary>
        /// <value>
        /// The saved output mapping.
        /// </value>
        public string SavedOutputMapping { get; private set; }

        /// <summary>
        /// Gets the activity input definitions.
        /// </summary>
        /// <value>
        /// The activity input definitions.
        /// </value>
        public string ActivityInputDefinitions { get; private set; }

        /// <summary>
        /// Gets the activity output definitions.
        /// </summary>
        /// <value>
        /// The activity output definitions.
        /// </value>
        public string ActivityOutputDefinitions { get; private set; }

        /// <summary>
        /// Gets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public Type ActivityType { get; private set; }

        public bool IsWorkflow { get; private set; }

        /// <summary>
        /// Setups the activity data.
        /// </summary>
        /// <param name="activity">The activity.</param>
        public void SetupActivityData(IWebActivity activity)
        {
            if (activity != null)
            {
                SavedInputMapping = activity.SavedInputMapping;
                SavedOutputMapping = activity.SavedOutputMapping;
                ActivityType = activity.UnderlyingWebActivityObjectType;

                if (activity.ResourceModel != null)
                {
                IsWorkflow = activity.ResourceModel.ResourceType == ResourceType.WorkflowService;

                    // extract the IO data too ;)
                    string serviceDefinition = activity.ResourceModel.WorkflowXaml;
                    if (!string.IsNullOrEmpty(serviceDefinition))
                    {
                        string inputs = string.Empty;
                        string outputs = string.Empty;

                        // handle workflows differently ;)
                        if (IsWorkflow)
                        {
                            var datalist = activity.ResourceModel.DataList;
                            var compiler = DataListFactory.CreateDataListCompiler();

                            inputs = compiler.GenerateSerializableDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Input);
                            outputs = compiler.GenerateSerializableDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Output);
                        }
                        else
                        {
                            // handle services ;)
                            inputs = ExtractMappingData(serviceDefinition, "Inputs");
                            outputs = ExtractMappingData(serviceDefinition, "Outputs");
                        }

                        ActivityInputDefinitions = inputs;
                        ActivityOutputDefinitions = outputs;     
                    }
                }
            }
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        public MappingViewModelTO Generate()
        {

            MappingViewModelTO result = null;

            var outputParser = DataListFactory.CreateOutputParser();
            var inputParser = DataListFactory.CreateInputParser();

            var inputList = GenerateMapping(SavedInputMapping, ActivityInputDefinitions, false, inputParser);
            
            var outputList = GenerateMapping(SavedOutputMapping, ActivityOutputDefinitions, true, outputParser);

            result = new MappingViewModelTO(inputList, outputList);
           
            // and set the data to save?!
            if(string.IsNullOrEmpty(SavedInputMapping))
            {
                SavedInputMapping = GetInputString(inputList);
            }

            if(string.IsNullOrEmpty(SavedOutputMapping))
            {
                SavedOutputMapping = GetOutputString(outputList);
            }

            return result;
            }

        /// <summary>
        /// Gets the input string.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <returns></returns>
        private string GetInputString(IList<IInputOutputViewModel> inputData)
        {
            string inputString = string.Empty;
            IList<IDev2Definition> inputs = new List<IDev2Definition>();
            if(inputData.Count != 0)
            {
                foreach(IInputOutputViewModel itp in inputData)
                {
                    inputs.Add(itp.GetGenerationTO());
                }
                inputString = DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input);
            }
            return inputString;
        }
           
        /// <summary>
        /// Gets the output string.
        /// </summary>
        /// <param name="outputData">The output data.</param>
        /// <returns></returns>
        private string GetOutputString(IList<IInputOutputViewModel> outputData)
        {
            string outputString = string.Empty;
            IList<IDev2Definition> outputs = new List<IDev2Definition>();
            if(outputData.Count != 0)
            {
                foreach(IInputOutputViewModel otp in outputData)
                {
                    outputs.Add(otp.GetGenerationTO());
                }
                outputString = DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output);
            }
            return outputString;
        }

        /// <summary>
        /// Generates the mapping.
        /// </summary>
        /// <param name="savedMappingData">The mapping data.</param>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="isOutputMapping">if set to <c>true</c> [is output mapping].</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> GenerateMapping(string savedMappingData, string mappingDefinitions, bool isOutputMapping, IDev2LanguageParser parser)
        {
            IList<IInputOutputViewModel> result;

            if (string.IsNullOrEmpty(savedMappingData))
            {
                // TODO : Inject fuzzy matching logic here ;)
                var fuzzyMatchDefinitions = GenerateMatchFragmentsFromDataList();

                result = CreateMappingList(mappingDefinitions, parser, true, isOutputMapping, fuzzyMatchDefinitions);
            }
            else
            {
                
                // generate the master view ;)
                var masterView = CreateMappingList(mappingDefinitions, parser, true, isOutputMapping);

                // use existing data ;)
                var existingView = CreateMappingList(savedMappingData, parser, false, isOutputMapping);

                // Now adjust for the difference between the two views ;)
                result = ReconcileExistingAndMasterView(masterView, existingView);
            }

            return result;
        }

        /// <summary>
        /// Reconsiles the exiting and master view.
        /// </summary>
        /// <param name="masterView">The master view.</param>
        /// <param name="existingView">The existing view.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> ReconcileExistingAndMasterView(IList<IInputOutputViewModel> masterView, IList<IInputOutputViewModel> existingView)
        {
            IList<IInputOutputViewModel> result = null;

            var equalityCompareImpl = new InputOutputViewModelEqualityComparer();

            var intersectionResult = existingView.Intersect(masterView, equalityCompareImpl);

            foreach (var intersectionRowItem in intersectionResult)
            {
                //  Find a match in master list and tranfer properties ;)
                var match = masterView.FirstOrDefault(c => c.DisplayName == intersectionRowItem.DisplayName);

                if (match != null)
                {
                    intersectionRowItem.Required = match.Required;
                    intersectionRowItem.DefaultValue = match.DefaultValue;
                }
            }


            // ordering maters ;)
            result = intersectionResult.Union(masterView, equalityCompareImpl).ToList();

            return result;
        }

        /// <summary>
        /// Creates the mapping list.
        /// </summary>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="autoAddBrackets">if set to <c>true</c> [automatic add brackets].</param>
        /// <param name="isOutputMapping">if set to <c>true</c> [is output mapping].</param>
        /// <param name="fuzzyMatch">The fuzzy match.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> CreateMappingList(string mappingDefinitions, IDev2LanguageParser parser, bool autoAddBrackets, bool isOutputMapping, FuzzyMatchVO fuzzyMatch = null)
        {
            IList<IInputOutputViewModel> result = new List<IInputOutputViewModel>();
            IList<IDev2Definition> concreteDefinitions = parser.ParseAndAllowBlanks(mappingDefinitions);

            
            var masterRecordsetName = string.Empty;

            foreach(var def in concreteDefinitions)
            {
                var injectValue = def.RawValue;

                if(!string.IsNullOrEmpty(injectValue) || IsWorkflow)
                {
                    if (autoAddBrackets)
                    {
                        // When output mapping we need to replace the recordset name if present with MasterRecordset
                        // 
                        if(isOutputMapping && def.IsRecordSet && fuzzyMatch != null)
                        {
                            var field = DataListUtil.ExtractFieldNameFromValue(injectValue);

                            if (IsWorkflow)
                            {
                                field = def.Name;
                            }

                            string recordsetName = fuzzyMatch.FetchMatch(def.Name);
                            if (!string.IsNullOrEmpty(recordsetName))
                            {
                                masterRecordsetName = recordsetName;
                            }
                            else
                            {
                                // we have no match, use the current mapping value ;)
                                if (!IsWorkflow)
                                {
                                    masterRecordsetName = DataListUtil.ExtractRecordsetNameFromValue(injectValue);
                                }
                                else
                                {
                                    masterRecordsetName = def.RecordSetName;
                                }
                            }


                            injectValue = FormatString(masterRecordsetName, field);
                            
                        }
                        else
                        {
                            if (def.IsRecordSet)
                            {

                                string recordsetName = def.RecordSetName;
                                if (fuzzyMatch != null)
                                {
                                    recordsetName = fuzzyMatch.FetchMatch(def.Name);
                                    if (!String.IsNullOrEmpty(recordsetName))
                                    {
                                        masterRecordsetName = recordsetName;
                                    }
                                    else
                                    {
                                        masterRecordsetName = def.RecordSetName;
                                    }
                                }
                                else
                                {
                                    masterRecordsetName = def.RecordSetName;
                                }

                                injectValue = FormatString(masterRecordsetName, def.Name);
                            }
                            else
                            {
                                if (!IsWorkflow)
                                {
                                    injectValue = DataListUtil.AddBracketsToValueIfNotExist(injectValue);
                                }
                                else
                                {
                                    injectValue = DataListUtil.AddBracketsToValueIfNotExist(def.Name);
                                }
                            }
                            
                        }
                    }
                }
                else
                {
                    if (!def.IsRecordSet)
                    {
                        if(!String.IsNullOrEmpty(def.RawValue) && String.IsNullOrEmpty(SavedInputMapping))
                        {
                            injectValue = DataListUtil.AddBracketsToValueIfNotExist(def.Name);
                        }
                    }
                    else
                    {
                        if (!isOutputMapping)
                        {
                            var field = def.Name;

                            if (fuzzyMatch != null && def.IsRecordSet)
                            {
                                if (string.IsNullOrEmpty(masterRecordsetName))
                                {
                                    string recordsetName = fuzzyMatch.FetchMatch(def.Name);
                                    if (!string.IsNullOrEmpty(recordsetName))
                                    {
                                        masterRecordsetName = recordsetName;
                                    }
                                    else
                                    {
                                        // we have no match, use the current mapping value ;)
                                        masterRecordsetName = def.RecordSetName;
                                    }
                                }

                                injectValue = DataListUtil.ComposeIntoUserVisibleRecordset(masterRecordsetName,
                                                                                           string.Empty, field);
                                injectValue = DataListUtil.AddBracketsToValueIfNotExist(injectValue);
                            }
                            else
                            {
                                if(!String.IsNullOrEmpty(def.RawValue) && String.IsNullOrEmpty(SavedInputMapping))
                                {
                                    injectValue = FormatString(def.RecordSetName, def.Name);
                                }
                            }
                        }
                        else
                        {
                            if(!String.IsNullOrEmpty(def.RawValue) && String.IsNullOrEmpty(SavedOutputMapping))
                            {
                                injectValue = FormatString(def.RecordSetName, def.Name);
                            }
                        }
                    }
                }

                var injectMapsTo = def.MapsTo;

                // no saved mappings add brackets ;)
                if(!string.IsNullOrEmpty(injectMapsTo) && string.IsNullOrEmpty(SavedInputMapping))
                {
                    injectMapsTo = DataListUtil.AddBracketsToValueIfNotExist(injectMapsTo);
                }
                else
                {
                    if(def.IsRecordSet)
                    {
                        var tmp = injectValue.Replace("()", "(*)");
                        injectMapsTo = tmp; // tag it as the same ;)
                    }
                    else
                    {
                        injectMapsTo = injectValue; // tag it as the same ;)
                    }
                }

                // def.RecordSetName -> recordsetName
                var viewModel = new InputOutputViewModel(def.Name, injectValue, injectMapsTo, def.DefaultValue, def.IsRequired, def.RecordSetName, def.EmptyToNull);

                result.Add(viewModel);
            }

            return result;
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="recset">The recset.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private string FormatString(string recset, string field)
        {
            var tmp = DataListUtil.ComposeIntoUserVisibleRecordset(recset, string.Empty,field);
            return DataListUtil.AddBracketsToValueIfNotExist(tmp);
        }

        /// <summary>
        /// Generates the match fragments from data list.
        /// </summary>
        /// <returns></returns>
        private FuzzyMatchVO GenerateMatchFragmentsFromDataList()
        {
            FuzzyMatchVO result = null;

            if (!string.IsNullOrEmpty(DataList))
            {
                var compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO invokeErrors;
                var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, DataList, out invokeErrors);
                var dl = compiler.FetchBinaryDataList(dlID, out invokeErrors);
                IDictionary<string, string> tmp = new Dictionary<string, string>();

                if (dl != null)
                {
                    foreach (var rs in dl.FetchRecordsetEntries())
                    {
                        // build map for each column in a recordset ;)
                        foreach (var col in rs.Columns)
                        {
                            if (!tmp.Keys.Contains(col.ColumnName))
                            {
                                tmp[col.ColumnName] = rs.Namespace;
                            }
                        }
                    }
                }

                result = new FuzzyMatchVO(tmp);
                
            }

            return result;
        }

        /// <summary>
        /// Extracts the mapping data.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <returns></returns>
        private string ExtractMappingData(string serviceDefinition, string regionName)
        {
            string result = string.Empty;

            XElement xe = XElement.Parse(serviceDefinition);

            var region = xe.Element(regionName);

            if (region != null)
            {
                result = region.ToString(SaveOptions.DisableFormatting);
            }
            
            

            return result;
        }
    }
}
