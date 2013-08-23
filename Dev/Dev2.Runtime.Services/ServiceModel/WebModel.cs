﻿using System;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Runtime.Diagnostics;

namespace Dev2.Runtime.ServiceModel
{
    /// <summary>
    /// A model class for pushing and pulling JSON serialized objects to and from Web into the Server
    /// </summary>
    public class WebModel : ExceptionManager
    {

        #region Model Save

        /// <summary>
        /// Saves the JSON model for decisions from the wizard ;)
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string SaveModel(string args, Guid workspaceID, Guid dataListID)
        {
            string result = "{ \"message\" : \"Error Saving Model\"} ";

            if(dataListID != GlobalConstants.NullDataListID)
            {
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;

                // remove the silly Choose... from the string
                args = Dev2DecisionStack.RemoveDummyOptionsFromModel(args);
                // remove [[]], &, !
                args = Dev2DecisionStack.RemoveNaughtyCharsFromModel(args);
                
                compiler.UpsertSystemTag(dataListID, enSystemTag.SystemModel, args, out errors);

                result = "{  \"message\" : \"Saved Model\"} ";
            }

            return result;
        }

        #endregion


        #region Model Fetch
        /// <summary>
        /// Fetches the decision model.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string FetchDecisionModel(string args, Guid workspaceID, Guid dataListID)
        {

            if(dataListID != GlobalConstants.NullDataListID)
            {
                var compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors;

                var result = compiler.FetchSystemModelAsWebModel<Dev2DecisionStack>(dataListID, out errors);

                if(errors.HasErrors())
                {
                    compiler.UpsertSystemTag(dataListID, enSystemTag.Dev2Error, errors.MakeDataListReady(), out errors);
                }

                return result;
            }

            return "{}"; // empty model
        }

        /// <summary>
        /// Fetches the switch expression.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string FetchSwitchExpression(string args, Guid workspaceID, Guid dataListID)
        {
            if(dataListID != GlobalConstants.NullDataListID)
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors;

                var result = compiler.FetchSystemModelAsWebModel<Dev2Switch>(dataListID, out errors);

                if(errors.HasErrors())
                {
                    compiler.UpsertSystemTag(dataListID, enSystemTag.Dev2Error, errors.MakeDataListReady(), out errors);
                }

                return result;
            }

            return "{}"; // empty model
        }

        #endregion
    }
}
