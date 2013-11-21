﻿using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.PathOperations;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To create an activity to create files on FTP, FTPS and file system
    /// </summary>
    public class DsfPathCreate : DsfAbstractFileActivity, IPathOutput, IPathOverwrite
    {

        public DsfPathCreate()
            : base("Create")
        {
            OutputPath = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO allErrors)
        {

            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dataObject.DataListID;
            IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

            //get all the possible paths for all the string variables
            IBinaryDataListEntry outputPathEntry = compiler.Evaluate(executionId, enActionType.User, OutputPath, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator outputItr = Dev2ValueObjectFactory.CreateEvaluateIterator(outputPathEntry);
            colItr.AddIterator(outputItr);

            IBinaryDataListEntry usernameEntry = compiler.Evaluate(executionId, enActionType.User, Username, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator unameItr = Dev2ValueObjectFactory.CreateEvaluateIterator(usernameEntry);
            colItr.AddIterator(unameItr);

            IBinaryDataListEntry passwordEntry = compiler.Evaluate(executionId, enActionType.User, Password, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator passItr = Dev2ValueObjectFactory.CreateEvaluateIterator(passwordEntry);
            colItr.AddIterator(passItr);

            if (dataObject.IsDebug || dataObject.RemoteInvoke)
            {
                AddDebugInputItem(OutputPath, "Output Path", outputPathEntry, executionId);
                AddDebugInputItemOverwrite(executionId, Overwrite);
                AddDebugInputItemUserNamePassword(executionId, usernameEntry);
            }

            while (colItr.HasMoreData())
            {

                string error = string.Empty;
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(Overwrite);

                try
                {
                    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(colItr.FetchNextRow(outputItr).TheValue,
                                                                                colItr.FetchNextRow(unameItr).TheValue,
                                                                                colItr.FetchNextRow(passItr).TheValue,
                                                                                IsNotCertVerifiable);

                    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                    string result = broker.Create(dstEndPoint, opTO, true);
                    outputs.Add(DataListFactory.CreateOutputTO(Result, result));
                }
                catch (Exception e)
                {
                    allErrors.AddError(e.Message);
                }
            }

            return outputs;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Inputs("Output Path")]
        [FindMissing]
        public string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfPathCreate" /> is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite
        {
            get;
            set;
        }

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                OutputPath = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(OutputPath);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
