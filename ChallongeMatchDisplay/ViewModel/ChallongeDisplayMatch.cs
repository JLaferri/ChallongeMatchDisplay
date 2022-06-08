using System;
using System.Linq;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel;

internal class ChallongeDisplayMatch
{
	public enum DisplayType
	{
		Assigned,
		Unassigned
	}

	public ChallongeObservableMatch Match { get; private set; }

	public DisplayType MatchDisplayType { get; private set; }

	public ICommand Player1Wins { get; private set; }

	public ICommand Player2Wins { get; private set; }

	public ICommand Player1WinsScored { get; private set; }

	public ICommand Player2WinsScored { get; private set; }

	public ICommand Player1ToggleMissing { get; private set; }

	public ICommand Player2ToggleMissing { get; private set; }

	public ICommand AssignStation { get; private set; }

	public ICommand CallMatchAnywhere { get; private set; }

	public ICommand CallMatch { get; private set; }

	public ICommand UncallMatch { get; private set; }

	public ChallongeDisplayMatch(ChallongeOrganizerViewModel ovm, ChallongeObservableMatch match, DisplayType displayType)
	{
		ChallongeDisplayMatch challongeDisplayMatch = this;
		Match = match;
		MatchDisplayType = displayType;
		Action startAction = delegate
		{
			ovm.ErrorMessage = null;
			ovm.IsBusy = true;
		};
		Action endAction = delegate
		{
			ovm.IsBusy = false;
		};
		Action<Exception> errorHandler = delegate(Exception ex)
		{
			if (ex.InnerException is ChallongeApiException)
			{
				ChallongeApiException ex2 = (ChallongeApiException)ex.InnerException;
				if (ex2.Errors != null)
				{
					ovm.ErrorMessage = ex2.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
				}
				else
				{
					ovm.ErrorMessage = $"Error with ResponseStatus \"{ex2.RestResponse.ResponseStatus}\" and StatusCode \"{ex2.RestResponse.StatusCode}\". {ex2.RestResponse.ErrorMessage}";
				}
			}
			else
			{
				ovm.ErrorMessage = ex.NewLineDelimitedMessages();
			}
			ovm.IsBusy = false;
		};
		Player1Wins = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.ReportPlayer1Victory(SetScore.Create(1, 0));
		}, startAction, endAction, errorHandler);
		Player2Wins = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.ReportPlayer2Victory(SetScore.Create(0, 1));
		}, startAction, endAction, errorHandler);
		Player1WinsScored = Command.CreateAsync((SetScore[] _) => true, delegate(SetScore[] scores)
		{
			challongeDisplayMatch.Match.ReportPlayer1Victory(scores);
		}, delegate
		{
			startAction();
		}, delegate
		{
			endAction();
		}, delegate(SetScore[] _, Exception ex)
		{
			errorHandler(ex);
		});
		Player2WinsScored = Command.CreateAsync((SetScore[] _) => true, delegate(SetScore[] scores)
		{
			challongeDisplayMatch.Match.ReportPlayer2Victory(scores);
		}, delegate
		{
			startAction();
		}, delegate
		{
			endAction();
		}, delegate(SetScore[] _, Exception ex)
		{
			errorHandler(ex);
		});
		Player1ToggleMissing = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.Player1.IsMissing = !challongeDisplayMatch.Match.Player1.IsMissing;
		}, startAction, endAction, errorHandler);
		Player2ToggleMissing = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.Player2.IsMissing = !challongeDisplayMatch.Match.Player2.IsMissing;
		}, startAction, endAction, errorHandler);
		AssignStation = Command.CreateAsync((ChallongeStationModel _) => true, delegate(ChallongeStationModel s)
		{
			challongeDisplayMatch.Match.AssignPlayersToStation(s.Name);
		}, delegate
		{
			startAction();
		}, delegate
		{
			endAction();
		}, delegate(ChallongeStationModel _, Exception ex)
		{
			errorHandler(ex);
		});
		CallMatchAnywhere = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.AssignPlayersToStation("Any");
		}, startAction, endAction, errorHandler);
		CallMatch = Command.CreateAsync((ChallongeStationModel _) => true, delegate(ChallongeStationModel s)
		{
			if (!match.IsMatchInProgress)
			{
				if (s != null)
				{
					challongeDisplayMatch.Match.AssignPlayersToStation(s.Name);
				}
				else
				{
					challongeDisplayMatch.Match.AssignPlayersToStation("Any");
				}
			}
		}, delegate
		{
			startAction();
		}, delegate
		{
			endAction();
		}, delegate(ChallongeStationModel _, Exception ex)
		{
			errorHandler(ex);
		});
		UncallMatch = Command.CreateAsync(() => true, delegate
		{
			challongeDisplayMatch.Match.ClearStationAssignment();
		}, startAction, endAction, errorHandler);
	}
}
