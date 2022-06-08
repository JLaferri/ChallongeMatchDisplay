namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class GlobalSettings
{
	private static volatile GlobalSettings instance;

	private static object syncRoot = new object();

	public static GlobalSettings Instance
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						instance = new GlobalSettings();
					}
				}
			}
			return instance;
		}
	}

	public NewMatchAction SelectedNewMatchAction { get; set; }

	private GlobalSettings()
	{
		initialize();
	}

	private void initialize()
	{
		SelectedNewMatchAction = NewMatchAction.None;
	}
}
