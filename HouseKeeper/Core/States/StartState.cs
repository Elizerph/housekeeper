using HouseKeeper.Contexts;

using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class StartState : State
{
    public StartState(BotDialog dialog, IApplicationContextFactory applicationContextFactory) 
        : base(dialog, applicationContextFactory)
    {
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.Send("Out of context");
        return this;
    }

    public override async Task<IState> InputDocument(string fileId)
    {
        await Dialog.Send("Out of context");
        return this;
    }

    public override async Task<IState> InputText(string text)
    {
        if (string.Equals(text, "/start"))
        {
            await using var context = await ApplicationContextFactory.Create();
            var user = await context.Users.Where(x => x.Id == Dialog.UserId)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                user = new Models.User
                {
                    Id = Dialog.UserId,
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
            await Dialog.Send("Welcome! Use /manage command to begin");
            return new DefaultState(this);
        }
        else
            return this;
    }
}
