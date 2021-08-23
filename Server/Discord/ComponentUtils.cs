using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;

public static class ComponentUtils
{
    #region DiscordMessage extensions

    // if action returns false interactivity will stop listening for new button interactions
    public static void OnButtonInteraction(this DiscordMessage msg, Func<ComponentInteractionCreateEventArgs, bool> predicate, Func<InteractivityResult<ComponentInteractionCreateEventArgs>, Task<bool>> action)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                var result = await msg.WaitForButtonAsync(predicate);

                if (result.TimedOut || await action(result) == false)
                    break;
            }
        });
    }

    // if action returns false interactivity will stop listening for new select interactions
    public static void OnSelectInteraction(this DiscordMessage msg, Func<ComponentInteractionCreateEventArgs, bool> predicate, Func<InteractivityResult<ComponentInteractionCreateEventArgs>, Task<bool>> action)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                var result = await msg.WaitForSelectAsync(predicate);

                if (result.TimedOut || await action(result) == false)
                    break;
            }
        });
    }
    #endregion

    #region Destroy button
    // generate red "destroy" button
    public static DiscordButtonComponent GetDestroyButton(string salt) => new DiscordButtonComponent(ButtonStyle.Danger, "destroy" + salt, "", emoji: new DiscordComponentEmoji("✖️"));

    // waits for a button with the id "destroy" and then deletes the message
    public static void OnDestroyButton(this DiscordMessage msg, string salt, DiscordUser user = null)
    {
        _ = Task.Run(async () =>
        {
            var result = await msg.WaitForButtonAsync(
                (args) => args.Id == "destroyButton" + salt && (user == null || args.User == user)
                , null);
            if (result.TimedOut)
                return;
            await msg.DeleteAsync();
        });
    }
    #endregion
}