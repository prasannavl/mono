//
// System.Runtime.Remoting.Messaging.MethodResponse.cs
//
// Author:	Duncan Mak (duncan@ximian.com)
//		Patrik Torstensson
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
	public class MethodResponse : IMethodReturnMessage, ISerializable
	{
		string _methodName;
		string _uri;
		string _typeName;
		MethodBase _methodBase;

		object _returnValue;
		Exception _exception;
		Type [] _methodSignature;
		ArgInfo _inArgInfo;
		InternalDictionary _properties;

		object []  _outArgs;

		IMethodCallMessage _callMsg;

		LogicalCallContext _callContext;

		public MethodResponse (Header[] headers, IMethodCallMessage mcm)
		{
		}

		internal MethodResponse (Exception e, IMethodCallMessage msg) {
			_callMsg = msg;

			if (null != msg)
				_uri = msg.Uri;
			else
				_uri = String.Empty;
			
			_exception = e;
			_returnValue = null;
			_outArgs = new object[0];	// .NET does this
		}

		internal MethodResponse (object returnValue, object [] outArgs, LogicalCallContext callCtx, IMethodCallMessage msg) {
			_callMsg = msg;

			_uri = msg.Uri;
			
			_exception = null;
			_returnValue = returnValue;
			_outArgs = outArgs;
		}

		internal MethodResponse (IMethodCallMessage msg, CADMethodReturnMessage retmsg) {
			_callMsg = msg;

			_methodBase = msg.MethodBase;
			//_typeName = msg.TypeName;
			_uri = msg.Uri;
			_methodName = msg.MethodName;
			
			// Get unmarshalled arguments
			ArrayList args = retmsg.GetArguments ();

			_exception = retmsg.GetException (args);
			_returnValue = retmsg.GetReturnValue (args);
			_outArgs = retmsg.GetArgs (args);

			if (retmsg.PropertiesCount > 0)
				CADMessageBase.UnmarshalProperties (Properties, retmsg.PropertiesCount, args);
		}

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public int ArgCount {
			get { 
				if (null == _outArgs)
					return 0;

				return _outArgs.Length;
			}
		}

		public object[] Args {
			get { 
				return _outArgs; 
			}
		}

		public Exception Exception {
			get { 
				return _exception; 
			}
		}
		
		public bool HasVarArgs {
			get { 
				return false;
			}
		}
		
		public LogicalCallContext LogicalCallContext {
			get { 
				return _callContext;
			}
		}
		
		public MethodBase MethodBase {
			get { 
				if (null == _methodBase && null != _callMsg)
					_methodBase = _callMsg.MethodBase;

				return _methodBase;
			}
		}

		public string MethodName {
			get { 
				if (null == _methodName && null != _callMsg)
					_methodName = _callMsg.MethodName;

				return _methodName;
			}
		}

		public object MethodSignature {
			get { 
				if (null == _methodSignature && null != _callMsg)
					_methodSignature = (Type []) _callMsg.MethodSignature;

				return _methodSignature;
			}
		}

		public int OutArgCount {
			get { 
				if (null == _methodBase)
					return 0;

				if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
				return _inArgInfo.GetInOutArgCount();
			}
		}

		public object[] OutArgs {
			get { 
				if (null == _methodBase)
					return new object[0];

				if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
				return _inArgInfo.GetInOutArgs (_outArgs);
			}
		}

		public virtual IDictionary Properties {
			get { 
				if (null == _properties) {
					_properties = new InternalDictionary (this);

					ExternalProperties = _properties;
					InternalProperties = _properties.GetInternalProperties();
				}
				
				return ExternalProperties;
			}
		}

		public object ReturnValue {
			get { 
				return _returnValue;
			}
		}

		public string TypeName {
			get { 
				if (null == _typeName && null != _callMsg)
					_typeName = _callMsg.TypeName;

				return _typeName;
			}
		}

		public string Uri {
			get { 
				if (null == _uri && null != _callMsg)
					_uri = _callMsg.Uri;
				
				return _uri;
			}

			set { 
				_uri = value;
			}
		}

		public object GetArg (int argNum)
		{
			if (null == _outArgs)
				return null;

			return _outArgs [argNum];
		}

		public string GetArgName (int index)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		public object GetOutArg (int argNum)
		{
			if (null == _methodBase)
				return null;

			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _outArgs [_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public string GetOutArgName (int index)
		{
			if (null == _methodBase)
				return "__method_" + index;

			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _inArgInfo.GetInOutArgName(index);
		}

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		class InternalDictionary : MethodReturnDictionary {
			public InternalDictionary(MethodResponse message) : base (message) { }

			protected override void SetMethodProperty (string key, object value) {
				if (key == "__Uri") ((MethodResponse) _message).Uri = (string)value;
				else base.SetMethodProperty (key, value);
			}
		}
	}
}
