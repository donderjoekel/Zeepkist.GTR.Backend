namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Categories;

internal class ResponseModel
{
    internal class Category
    {
        public string DisplayName { get; set; } = null!;
        public int Value { get; set; }
    }
    
    public List<Category> Categories { get; set; } = null!;
}
