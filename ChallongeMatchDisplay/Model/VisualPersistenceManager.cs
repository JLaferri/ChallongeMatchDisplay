using System;
using System.IO;
using System.Runtime.Serialization;

namespace Fizzi.Applications.ChallongeVisualization.Model;

[DataContract]
internal class VisualPersistenceManager
{
	private static volatile VisualPersistenceManager instance;

	private static object syncRoot = new object();

	private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fizzi", "ChallongeMatchDisplay", "visual.xml");

	public static VisualPersistenceManager Instance
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						instance = new VisualPersistenceManager();
					}
				}
			}
			return instance;
		}
	}

	[DataMember]
	public double TextSizeRatio { get; set; }

	private VisualPersistenceManager()
	{
		initialize();
	}

	private void initialize()
	{
		LoadFromStorage();
	}

	public void LoadFromStorage()
	{
		VisualPersistenceManager visualPersistenceManager = null;
		try
		{
			DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(VisualPersistenceManager));
			using Stream stream = new FileStream(path, FileMode.Open);
			visualPersistenceManager = (VisualPersistenceManager)dataContractSerializer.ReadObject(stream);
		}
		catch (Exception)
		{
		}
		if (visualPersistenceManager != null)
		{
			TextSizeRatio = visualPersistenceManager.TextSizeRatio;
		}
		else
		{
			TextSizeRatio = 1.0;
		}
	}

	public void Save()
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(VisualPersistenceManager));
		using Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		dataContractSerializer.WriteObject(stream, this);
	}
}
