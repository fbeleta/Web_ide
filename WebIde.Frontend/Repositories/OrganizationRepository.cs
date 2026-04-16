using WebIde.Model;

namespace WebIde.Web.Repositories;

public class OrganizationRepository
{
    public List<Organization> GetAll() => MockData.Organizations;
    public Organization? GetById(int id) => MockData.Organizations.FirstOrDefault(o => o.Id == id);
}
