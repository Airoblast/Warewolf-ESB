﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for SequenceActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SequenceActivityFindMissingStrategyTests
    {


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceActivityFindMissingStrategy_GetActivityFields")]
        public void SequenceActivityFindMissingStrategy_GetActivityFields_WithAssignAndDataMerge_ReturnsAllVariables()
        {
            //------------Setup for test--------------------------
            DsfMultiAssignActivity multiAssignActivity = new DsfMultiAssignActivity();
            multiAssignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[AssignRight1]]", "[[AssignLeft1]]", 1), new ActivityDTO("[[AssignRight2]]", "[[AssignLeft2]]", 2) };

            DsfDataMergeActivity dataMergeActivity = new DsfDataMergeActivity();
            dataMergeActivity.Result = "[[Result]]";
            dataMergeActivity.MergeCollection.Add(new DataMergeDTO("[[rec().a]]", "Index", "6", 1, "[[b]]", "Left"));

            DsfActivity dsfActivity = new DsfActivity();
            dsfActivity.InputMapping = @"<Inputs><Input Name=""reg"" Source=""NUD2347"" DefaultValue=""NUD2347""><Validator Type=""Required"" /></Input><Input Name=""asdfsad"" Source=""registration223"" DefaultValue=""w3rt24324""><Validator Type=""Required"" /></Input><Input Name=""number"" Source=""[[number]]"" /></Inputs>";
            dsfActivity.OutputMapping = @"<Outputs><Output Name=""vehicleVin"" MapsTo=""VIN"" Value="""" /><Output Name=""vehicleColor"" MapsTo=""VehicleColor"" Value="""" /><Output Name=""speed"" MapsTo=""speed"" Value="""" Recordset=""Fines"" /><Output Name=""date"" MapsTo=""date"" Value=""Fines.Date"" Recordset=""Fines"" /><Output Name=""location"" MapsTo=""location"" Value="""" Recordset=""Fines"" /></Outputs>";
            DsfForEachActivity forEachActivity = new DsfForEachActivity();
            forEachActivity.ForEachElementName = "5";
            forEachActivity.DataFunc.Handler = dsfActivity;

            DsfSequenceActivity activity = new DsfSequenceActivity();
            activity.Activities.Add(multiAssignActivity);
            activity.Activities.Add(dataMergeActivity);
            activity.Activities.Add(forEachActivity);

            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.Sequence);
            //------------Execute Test---------------------------
            List<string> actual = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            List<string> expected = new List<string> { "[[AssignRight1]]", "[[AssignLeft1]]", "[[AssignRight2]]", "[[AssignLeft2]]", "[[b]]", "[[rec().a]]", "6", "[[Result]]", "NUD2347", "registration223", "[[number]]", "Fines.Date", "5" };
            CollectionAssert.AreEqual(expected, actual);
        }

    }
}