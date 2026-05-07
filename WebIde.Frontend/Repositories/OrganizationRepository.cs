using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class OrganizationRepository(WebIdeDbContext db)
{
    public List<Organization> GetAll() => db.Organizations
        .Include(o => o.Members)
        .OrderBy(o => o.Id)
        .ToList();

    public Organization? GetById(int id) => db.Organizations
        .Include(o => o.Members)
        .Include(o => o.ProblemSets).ThenInclude(ps => ps.Problems)
        .FirstOrDefault(o => o.Id == id);
}
