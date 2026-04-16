using WebIde.Model;

namespace WebIde.Web.Repositories;

public class SubmissionRepository
{
    public List<Submission> GetAll() => MockData.Submissions;
    public Submission? GetById(int id) => MockData.Submissions.FirstOrDefault(s => s.Id == id);
}
