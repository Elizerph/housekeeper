using HouseKeeper.Contexts;

namespace HouseKeeper.Core.States;
public abstract class State : IState
{
    public BotDialog Dialog { get; }
    public IApplicationContextFactory ApplicationContextFactory { get; }

    public State(BotDialog dialog, IApplicationContextFactory applicationContextFactory)
    {
        Dialog = dialog;
        ApplicationContextFactory = applicationContextFactory;
    }

    public State(State state)
    {
        Dialog = state.Dialog;
        ApplicationContextFactory = state.ApplicationContextFactory;
    }

    public abstract Task<IState> InputButton(int messageId, string buttonData);
    public abstract Task<IState> InputText(string text);
    public abstract Task<IState> InputDocument(string fileId);
}
