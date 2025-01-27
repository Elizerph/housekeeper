namespace HouseKeeper.Core.States;
public interface IState
{
    BotDialog Dialog { get; }
    Task<IState> InputText(string text);
    Task<IState> InputButton(int messageId, string buttonData);
    Task<IState> InputDocument(string fileId);
}
