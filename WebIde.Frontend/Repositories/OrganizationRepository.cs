using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class OrganizationRepository
{
    private readonly WebIdeDbContext _db;
    public OrganizationRepository(WebIdeDbContext db) => _db = db;

    public List<Organization> GetAll() =>
        _db.Organizations
            .Include(o => o.Members)
            .Include(o => o.ProblemSets)
            .OrderBy(o => o.Id)
            .ToList();

    public Organization? GetById(int id) =>
        _db.Organizations
            .Include(o => o.Members)
            .Include(o => o.ProblemSets).ThenInclude(ps => ps.Problems).ThenInclude(p => p.Tags)
            .FirstOrDefault(o => o.Id == id);
}
