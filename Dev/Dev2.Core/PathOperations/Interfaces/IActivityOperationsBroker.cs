﻿using System.Collections.Generic;

namespace Dev2.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an interface for brokering IO operations between different endpoints
    /// </summary>
    public interface IActivityOperationsBroker
    {
        /// <summary>
        /// Get the contents of a path as a string
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="deferredRead">if set to <c>true</c> [deferred read].</param>
        /// <returns></returns>
        string Get(IActivityIOOperationsEndPoint path, bool deferredRead = false);

        /// <summary>
        /// Dump a payload to a location as per the value of args
        /// </summary>
        /// <param name="dst">The DST.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args);


        /// <summary>
        /// Delete a file/folder 
        /// </summary>
        /// <param name="src"></param>
        string Delete(IActivityIOOperationsEndPoint src);

        /// <summary>
        /// List the contents of a folder
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src);

        /// <summary>
        /// Create a directory or file per the value of args
        /// </summary>
        /// <param name="dst">The DST.</param>
        /// <param name="args">The args.</param>
        /// <param name="createToFile">if set to <c>true</c> [create to file].</param>
        /// <returns></returns>
        string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile);

        /// <summary>
        /// Copy a file from src to dst as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args);

        /// <summary>
        /// Move a file from src to dst as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args);

        /// <summary>
        /// Zip a file/folder as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args);

        /// <summary>
        /// Unzip an archive as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2UnZipOperationTO args);
    }
}
