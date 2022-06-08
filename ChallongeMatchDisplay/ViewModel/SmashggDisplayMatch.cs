using System;
using System.Linq;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel;

internal class SmashggDisplayMatch
{
	public enum DisplayType
	{
		Assigned,
		Unassigned
	}

	public SmashggObservableMatch Match { get; private set; }

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

	public SmashggDisplayMatch(SmashggOrganizerViewModel ovm, SmashggObservableMatch match, DisplayType displayType)
	{
		SmashggDisplayMatch smashggDisplayMatch = this;
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
			if (ex.InnerException is SmashggApiException)
			{
				SmashggApiException ex2 = (SmashggApiException)ex.InnerException;
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
			smashggDisplayMatch.Match.ReportEntrant1Victory(SetScore.Create(1, 0));
		}, startAction, endAction, errorHandler);
		Player2Wins = Command.CreateAsync(() => true, delegate
		{
			smashggDisplayMatch.Match.ReportEntrant2Victory(SetScore.Create(0, 1));
		}, startAction, endAction, errorHandler);
		Player1WinsScored = Command.CreateAsync((SetScore[] _) => true, delegate(SetScore[] scores)
		{
			smashggDisplayMatch.Match.ReportEntrant1Victory(scores);
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
			smashggDisplayMatch.Match.ReportEntrant2Victory(scores);
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
	}
}
