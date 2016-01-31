using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class ResourceNotFoundException : Exception {

	private string path;

	protected ResourceNotFoundException() : base() {}

	// IMPROVE: support Type systemTypeInstance argument
	public ResourceNotFoundException(string path) :
	   base(string.Format("Resource \"{0}\" not found.", path))
	{
	   this.path = path;
	}

	public ResourceNotFoundException(string path, string message)
	   : base(message)
	{
	   this.path = path;
	}

	public ResourceNotFoundException(string path, string message, Exception innerException) :
	   base(message, innerException)
	{
	   this.path = path;
	}

	protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public string Path { get { return path; } }

}

public class UnassignedReferenceException : Exception {

	private MonoBehaviour script;
	private string referenceName;

	protected UnassignedReferenceException() : base() {}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName) :
	   base(string.Format("Attribute {1} of script {0} has not been assigned. Please assign it in the inspector.", script, referenceName))
	{
	   this.script = script;
	   this.referenceName = referenceName;
	}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName, string message)
	   : base(message)
	{
		this.script = script;
		this.referenceName = referenceName;
	}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName, string message, Exception innerException) :
	   base(message, innerException)
	{
		this.script = script;
		this.referenceName = referenceName;
	}

	protected UnassignedReferenceException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public MonoBehaviour Script { get { return script; } }
	public string ReferenceName { get { return referenceName; } }

}

public class UninitializedSingletonException : Exception {

	private string singletonName;

	protected UninitializedSingletonException() : base() {}

	public UninitializedSingletonException(string singletonName) :
	   base(string.Format("Singleton {0} has no instance initialized. Ensure that the game object with the {0} component has an Awake() method with \"Instance = this\".", singletonName))
	{
	   this.singletonName = singletonName;
	}

	public UninitializedSingletonException(string singletonName, string message)
	   : base(message)
	{
		this.singletonName = singletonName;
	}

	public UninitializedSingletonException(string singletonName, string message, Exception innerException) :
	   base(message, innerException)
	{
		this.singletonName = singletonName;
	}

	protected UninitializedSingletonException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public string SingletonName { get { return singletonName; } }

}

public class ReinitializeSingletonException : Exception {

	private string singletonName;

	protected ReinitializeSingletonException() : base() {}

	public ReinitializeSingletonException(string singletonName) :
	   base(string.Format("Singleton {0} has already an instance initialized. Please make sure there is only one game object with a {0} component.", singletonName))
	{
	   this.singletonName = singletonName;
	}

	public ReinitializeSingletonException(string singletonName, string message)
	   : base(message)
	{
		this.singletonName = singletonName;
	}

	public ReinitializeSingletonException(string singletonName, string message, Exception innerException) :
	   base(message, innerException)
	{
		this.singletonName = singletonName;
	}

	protected ReinitializeSingletonException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public string SingletonName { get { return singletonName; } }

}
