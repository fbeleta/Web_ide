using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemRepository
{
    public List<Problem> GetAll() => MockData.Problems;
    public Problem? GetById(int id) => MockData.Problems.FirstOrDefault(p => p.Id == id);
}
