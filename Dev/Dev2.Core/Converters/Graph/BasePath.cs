﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph
{
    public abstract class BasePath : IPath
    {
        #region Constructor

        public BasePath()
        {
            ActualPath = "";
            DisplayPath = "";
            SampleData = "";
            OutputExpression = "";
        }

        #endregion Constructor

        #region Properties

        [DataMember(Name = "ActualPath")]
        public string ActualPath { get; set; }

        [DataMember(Name = "DisplayPath")]
        public string DisplayPath { get; set; }

        [DataMember(Name = "SampleData")]
        public string SampleData { get; set; }

        [DataMember(Name = "OutputExpression")]
        public string OutputExpression { get; set; }

        #endregion Properties

        #region Override Methods

        public override string ToString()
        {
            return ActualPath;
        }

        #endregion Override Methods

        #region Abstract Methods

        public abstract IEnumerable<IPathSegment> GetSegements();
        public abstract IPathSegment CreatePathSegment(string pathSegmentString);

        #endregion Abstract Methods
    }
}
