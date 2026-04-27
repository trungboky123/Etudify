namespace back_end.Dto.Response;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }

    public CategoryResponse(int id, string name)
    {
        Id = id;
        Name = name;
    }
}