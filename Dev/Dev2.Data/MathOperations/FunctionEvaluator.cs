﻿using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Calculations.Engine;
using Infragistics.Calculations.CalcManager;
using System.Parsing.Intellisense;
using System.Parsing;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.MathOperations {
    public class FunctionEvaluator : IFunctionEvaluator {

        #region Private Members
        private IDev2CalculationManager _manager;

        #endregion Private Members

        #region Ctor

        public FunctionEvaluator() {
            _manager = new Dev2CalculationManager();
        }

        #endregion Ctor

        #region Public Methods
        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="expressionTO">The expression TO.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string EvaluateFunction(IEvaluationFunction expressionTO, Guid curDLID, out ErrorResultTO errors) {
            string expression = expressionTO.Function;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            ErrorResultTO allErrors = new ErrorResultTO();
            

            // Travis.Frisinger : 31.01.2013 - Hi-jack this and evaluate all our internal 
            IBinaryDataListEntry bde = compiler.Evaluate(curDLID, enActionType.CalculateSubstitution, expression, false, out errors);
            allErrors.MergeErrors(errors);
            if (bde != null)
            {
                expression = bde.FetchScalar().TheValue;
            }

            Node[] nodes = builder.Build(expression);
            string result = string.Empty;

            if (builder.EventLog.HasEventLogs) {
                IList<string> err = EvaluateEventLogs(builder.EventLog.GetEventLogs(), expression);

                foreach (string e in err) {
                    allErrors.AddError(e);
                }
            } else {

                List<Node> allNodes = new List<Node>();
                nodes[0].CollectNodes(allNodes);
                
                IterationNodeValueSource valueSource = null;
                bool startedIteration = false;
                bool isIteration = false;
                bool pendingIterationRecordSet = false;
                int maxRecords = -1;
                int currentRecord = 0;

                do {
                    if (startedIteration)
                        for (int i = 0; i < allNodes.Count; i++)
                            allNodes[i].EvaluatedValue = null;

                    for (int i = allNodes.Count - 1; i >= 0; i--) {
                        if (allNodes[i] is IterationNode) {
                            IterationNode refNode = allNodes[i] as IterationNode;
                            if (valueSource == null) valueSource = new IterationNodeValueSource(1);
                            refNode.ValueSource = valueSource;
                            pendingIterationRecordSet = true;
                            isIteration = true;
                        } else if (allNodes[i] is DatalistRecordSetNode) {
                            DatalistRecordSetNode refNode = allNodes[i] as DatalistRecordSetNode;

                            if (refNode.Parameter != null) {
                                if ((refNode.Parameter.Items != null && refNode.Parameter.Items.Length != 0) || refNode.Parameter.Statement != null)
                                    refNode.Parameter.EvaluatedValue = InternalEval(refNode.Parameter.GetEvaluatedValue());
                            }


                            // this way we fetch the correct field with the data...
                            IBinaryDataListEntry e = compiler.Evaluate(curDLID, enActionType.User, refNode.GetRepresentationForEvaluation(), false, out errors);
                            allErrors.MergeErrors(errors);
                            string error = string.Empty;
                            refNode.EvaluatedValue = e.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;;
                            allErrors.AddError(error);

                            if (pendingIterationRecordSet) {
                                pendingIterationRecordSet = false;

                                if (refNode.NestedIdentifier != null) {
                                    allErrors.AddError("An error occured while parsing { " + expression + " } Iteration operator can not be used with nested recordset identifiers.");
                                    break;
                                }

                                string evaluateRecordLeft = refNode.GetRepresentationForEvaluation();
                                evaluateRecordLeft = evaluateRecordLeft.Substring(2, evaluateRecordLeft.IndexOf('(') - 2);

                                int totalRecords = 0;
                                IBinaryDataList bdl = compiler.FetchBinaryDataList(curDLID, out errors);
                                IBinaryDataListEntry entry;
                                if (bdl.TryGetEntry(evaluateRecordLeft, out entry, out error)) {
                                    totalRecords = entry.FetchLastRecordsetIndex();
                                }
                                allErrors.AddError(error);

                                maxRecords = Math.Max(totalRecords, maxRecords);

                            }
           
                            
                        } else if (allNodes[i] is DatalistReferenceNode) {
                            DatalistReferenceNode refNode = allNodes[i] as DatalistReferenceNode;
                            IBinaryDataListEntry entry = compiler.Evaluate(curDLID, enActionType.User, refNode.GetRepresentationForEvaluation(), false, out errors);
                            allErrors.MergeErrors(errors);
                            string error = string.Empty;

                            if (entry.IsRecordset)
                            {
                                refNode.EvaluatedValue = entry.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;
                                var testParse = new double();
                                if (!Double.TryParse(refNode.EvaluatedValue, out testParse)) refNode.EvaluatedValue = String.Concat("\"", refNode.EvaluatedValue, "\"");//Bug 6438
                            }
                            else
                            {
                                refNode.EvaluatedValue = entry.FetchScalar().TheValue;
                                var testParse = new double();
                                if (!Double.TryParse(refNode.EvaluatedValue, out testParse)) refNode.EvaluatedValue = String.Concat("\"", refNode.EvaluatedValue, "\"");//Bug 6438
                            }
                        } else if (allNodes[i] is BinaryOperatorNode && allNodes[i].Identifier.Start.Definition == TokenKind.Colon) {
                            BinaryOperatorNode biNode = (BinaryOperatorNode)allNodes[i];

                            if (!(biNode.Left is DatalistRecordSetFieldNode)) {
                                allErrors.AddError("An error occured while parsing { " + expression + " } Range operator can only be used with record set fields.");
                                break;
                            }

                            if (!(biNode.Right is DatalistRecordSetFieldNode)) {
                                allErrors.AddError("An error occured while parsing { " + expression + " } Range operator can only be used with record set fields.");
                                break;
                            }

                            DatalistRecordSetFieldNode fieldLeft = (DatalistRecordSetFieldNode)biNode.Left;
                            DatalistRecordSetFieldNode fieldRight = (DatalistRecordSetFieldNode)biNode.Right;

                            string evaluateFieldLeft = (fieldLeft.Field != null) ? fieldLeft.Field.GetEvaluatedValue() : fieldLeft.Identifier.Content;
                            string evaluateFieldRight = (fieldRight.Field != null) ? fieldRight.Field.GetEvaluatedValue() : fieldRight.Identifier.Content;

                            if (!String.Equals(evaluateFieldLeft, evaluateFieldRight, StringComparison.Ordinal)) {
                                allErrors.AddError("An error occured while parsing { " + expression + " } Range operator must be used with the same record set fields.");
                                break;
                            }

                            string evaluateRecordLeft = fieldLeft.RecordSet.GetRepresentationForEvaluation();
                            evaluateRecordLeft = evaluateRecordLeft.Substring(2, evaluateRecordLeft.IndexOf('(') - 2);
                            string evaluateRecordRight = fieldRight.RecordSet.GetRepresentationForEvaluation();
                            evaluateRecordRight = evaluateRecordRight.Substring(2, evaluateRecordRight.IndexOf('(') - 2);

                            if (!String.Equals(evaluateRecordLeft, evaluateRecordRight, StringComparison.Ordinal)) {
                                allErrors.AddError("An error occured while parsing { " + expression + " } Range operator must be used with the same record sets.");
                                break;
                            }

                            int totalRecords = 0;
                            IBinaryDataList bdl = compiler.FetchBinaryDataList(curDLID, out errors);
                            string error = string.Empty;
                            IBinaryDataListEntry entry;
                            if (bdl.TryGetEntry(evaluateRecordLeft, out entry, out error)) {
                                totalRecords = entry.FetchLastRecordsetIndex();
                            }

                            string rawParamLeft = fieldLeft.RecordSet.Parameter.GetEvaluatedValue();
                            rawParamLeft = rawParamLeft.Length == 2 ? "" : rawParamLeft.Substring(1, rawParamLeft.Length - 2);
                            string rawParamRight = fieldRight.RecordSet.Parameter.GetEvaluatedValue();
                            rawParamRight = rawParamRight.Length == 2 ? "" : rawParamRight.Substring(1, rawParamRight.Length - 2);

                            int startIndex = 0;
                            int endIndex = 0;

                            if (!String.IsNullOrEmpty(rawParamLeft)) {
                                if (!Int32.TryParse(rawParamLeft, out startIndex) || startIndex <= 0) {
                                    allErrors.AddError("An error occured while parsing { " + expression + " } Recordset index must be a positive whole number that is greater than zero.");
                                    break;
                                }
                            } else {
                                startIndex = 1;
                            }

                            if (!String.IsNullOrEmpty(rawParamRight)) {
                                if (!Int32.TryParse(rawParamRight, out endIndex) || endIndex <= 0) {
                                    allErrors.AddError("An error occured while parsing { " + expression + " } Recordset index must be a positive whole number that is greater than zero.");
                                    break;
                                }

                                if (endIndex > totalRecords) {
                                    allErrors.AddError("An error occured while parsing { " + expression + " } Recordset end index must be a positive whole number that is less than the number of entries in the recordset.");
                                    break;
                                }
                            } else {
                                endIndex = totalRecords;
                            }

                            endIndex++;

                            StringBuilder rangeBuilder = new StringBuilder();

                            for (int k = startIndex; k < endIndex; k++)
                            {
                                if (k != startIndex)
                                {
                                    rangeBuilder.Append("," + entry.TryFetchRecordsetColumnAtIndex(evaluateFieldLeft, k, out error).TheValue);
                                    allErrors.AddError(error);
                                }
                                else
                                {
                                    rangeBuilder.Append(entry.TryFetchRecordsetColumnAtIndex(evaluateFieldLeft, k, out error).TheValue);
                                    allErrors.AddError(error);
                                }
                            }
                            allNodes[i].EvaluatedValue = rangeBuilder.ToString();
                        }
                    }

                    string evaluatedValue = nodes[0].GetEvaluatedValue();
                    result = InternalEval(evaluatedValue);

                    if (startedIteration) {
                        currentRecord = valueSource.Index++;
                    }

                    if (isIteration && !startedIteration) {
                        startedIteration = true;
                        currentRecord = valueSource.Index++;
                    }
                }
                while (startedIteration && currentRecord < maxRecords);
            }

            errors = allErrors;

            return result;
        }

        private sealed class IterationNodeValueSource : INodeValueSource {
            private int _index;

            public int Index { get { return _index; } set { _index = value; } }

            public IterationNodeValueSource(int index) {
                _index = index;
            }

            public string GetEvaluatedValue(Node node) {
                return _index.ToString();
            }

            public string GetRepresentationForEvaluation(Node node) {
                return _index.ToString();
            }
        }

        private IList<string> EvaluateEventLogs(ParseEventLogEntry[] parseEventLogEntry, string expression) {
            IList<string> result = new List<string>();
            result.Add("An error occured while parsing { " + expression + " } It appears to be malformed");
            return result;
        }

        /// <summary>
        /// Evaluate the expression according to the operation specified and pass this to the CalculationManager
        //  If something goes wrong during the execution, the error field will be populated and the method will
        //  return false.
        /// <summary>
        /// Tries the evaluate function.
        /// </summary>
        /// <param name="expressionTO">The expression automatic.</param>
        /// <param name="evaluation">The evaluation.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryEvaluateFunction(IEvaluationFunction expressionTO, out string evaluation, out string error) {
            bool isSuccessfulEvaluation;
            error = string.Empty;
            evaluation = string.Empty;

            if (!(string.IsNullOrEmpty(expressionTO.Function))) {
                try {
                    CalculationValue calcVal = _manager.CalculateFormula(expressionTO.Function);
                    evaluation = calcVal.GetResolvedValue().ToString();
                    isSuccessfulEvaluation = true;
                    calcVal = null;
                } catch (Exception ex) {
                    error = ex.Message;
                    isSuccessfulEvaluation = false;
                }
            } else {
                error = "Unable to evaluate empty function";
                isSuccessfulEvaluation = false;
            }

            return isSuccessfulEvaluation;
        }


        /// <summary>
        /// Evaluate the expression according to the operation specified and pass this to the CalculationManager
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="evaluation">The evaluation.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryEvaluateFunction(string expression, out string evaluation, out string error) {
            bool evaluationState = false;
            error = String.Empty;
            evaluation = String.Empty;
            if (!(String.IsNullOrEmpty(expression))) {

                try {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    if (value.IsError) {
                        error = value.ToErrorValue().Message;
                        evaluationState = false;
                    } else {
                        evaluation = value.GetResolvedValue().ToString();
                        evaluationState = true;
                    }
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }

            return evaluationState;
        }


        /// <summary>
        /// This is to cater for range operations 
        // It expects a List of Type To (either strings or any type of object that is IComparable).
        // And evaluates the whole list against the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="expression"></param>
        /// <param name="evalution"></param>
        /// <param name="error"></param>
        /// <returns></returns>

        public bool TryEvaluateFunction<T>(List<T> value, string expression, out string evaluation, out string error) where T : IConvertible {
            bool evaluationState = false;
            string evalString = string.Concat(expression, "(");
            evaluation = string.Empty;
            error = string.Empty;
            if (value == null || value.Count == 0) {
                evaluationState = false;
                error = "Cannot evaluate empty value list";
            } else if (!string.IsNullOrEmpty(expression)) {
                foreach (T val in value) {
                    string eval = val.ToString();
                    if (!string.IsNullOrEmpty(eval)) {
                        evalString += string.Concat(eval, ",");
                    }
                }
                evalString = evalString.Remove(evalString.LastIndexOf(","), 1);
                evalString = string.Concat(evalString, ")");
                try {
                    CalculationValue calcValue = _manager.CalculateFormula(evalString);
                    evaluation = calcValue.GetResolvedValue().ToString();
                    evaluationState = true;
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }
            return evaluationState;
        }

        #endregion Public Methods

        #region Statics

        public bool TryEvaluateAtomicFunction(string expression, out string evaluation, out string error) {
            bool evaluationState = false;
            error = String.Empty;
            evaluation = String.Empty;
            if (!(String.IsNullOrEmpty(expression))) {

                try {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    evaluation = value.GetResolvedValue().ToString();
                    evaluationState = true;
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }

            return evaluationState;
        }

        #endregion Statics

        #region Private Methods
        private string InternalEval(string res) {
            string calcResult = string.Empty;
            string error = string.Empty;
            string result = string.Empty;

            TryEvaluateFunction(res, out calcResult, out error);

            if (error == string.Empty) {
                result = calcResult;
            } else {
                if (error.Contains("<")) error = error.Replace("<", "");
                if (error.Contains(">")) error = error.Replace(">", "");

                result = error;
            }

            return result;
        }

        #endregion Private Methods

    }
}
