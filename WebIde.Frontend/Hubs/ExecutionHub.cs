using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebIde.Web.Repositories;

namespace WebIde.Web.Hubs;

[Authorize]
public class ExecutionHub(SubmissionRepository submissionRepo) : Hub
{
    public async Task SubscribeToSubmission(int submissionId)
    {
        var userId = int.Parse(Context.User!.FindFirst("webide:userId")!.Value);
        var owns = await submissionRepo.IsOwnedByAsync(submissionId, userId);
        if (!owns)
            throw new HubException("forbidden");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"submission:{submissionId}");
    }
}
