
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Tamir.SharpSsh.jsch
{
	/// <summary>
	/// Summary description for JSchException.
	/// </summary>
	public class JSchPartialAuthException : JSchException
	{
		string methods;
		public JSchPartialAuthException() : base()
		{
			methods = null;
		}

		public JSchPartialAuthException(string msg) : base (msg)
		{
			methods = msg;
		}

		public String getMethods()
		{
			return methods;
		}
	}
}
