﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an interface for Path Operations EndPoints
    /// </summary>
    public interface IActivityIOOperationsEndPoint {

        /// <summary>
        /// Returns the path this end point is operating on
        /// </summary>
        IActivityIOPath IOPath { get; set; }

        /// <summary>
        /// Return the contents of a file as a stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Stream Get(IActivityIOPath path);

        /// <summary>
        /// Put a stream into a location as per dst based upon the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="args"></param>
        int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args);

        /// <summary>
        /// Delete a file/folder at a location
        /// </summary>
        /// <param name="src"></param>
        bool Delete(IActivityIOPath src);

        /// <summary>
        /// List the contents of a directory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        IList<IActivityIOPath> ListDirectory(IActivityIOPath src);

        /// <summary>
        /// Create a directory as per the value of args
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args);


        /// <summary>
        /// Detect if a path exist already or not
        /// </summary>
        /// <param name="dst"></param>
        /// <returns></returns>
        bool PathExist(IActivityIOPath dst);

        /// <summary>
        /// Does the provider require local cache access to operate
        /// </summary>
        /// <returns></returns>
        bool RequiresLocalTmpStorage();

        /// <summary>
        /// Return the type of protocol this concrete handles ;)
        /// </summary>
        /// <returns></returns>
        bool HandlesType(enActivityIOPathType type);

        /// <summary>
        /// Returns the type of path as per this end-point
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        enPathType PathIs(IActivityIOPath path);


        /// <summary>
        /// Return the native path seperator
        /// </summary>
        /// <returns></returns>
        string PathSeperator();

    }
}
