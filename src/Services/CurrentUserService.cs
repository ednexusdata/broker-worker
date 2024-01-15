using OregonNexus.Broker.SharedKernel;

namespace OregonNexus.Broker.Worker.Services;

public class CurrentUserService : ICurrentUser
{
    
    public CurrentUserService()
    {
    }

    public Guid? AuthenticatedUserId()
    {
        return null;
    }
}