using WebIde.Model;

namespace WebIde.Web.Repositories;

public class TagRepository
{
    public List<Tag> GetAll() => MockData.Tags;
    public Tag? GetById(int id) => MockData.Tags.FirstOrDefault(t => t.Id == id);
}
