using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemSetRepository
{
    public List<ProblemSet> GetAll() => MockData.ProblemSets;
    public ProblemSet? GetById(int id) => MockData.ProblemSets.FirstOrDefault(ps => ps.Id == id);
}
