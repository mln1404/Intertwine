using Intertwine.Services.DTOs.Categories;

namespace Intertwine.Services.DTOs.Questions;

public class QuestionListDto
{
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string FullQuestion { get; set; } = string.Empty;

    public List<CategoryDto> Categories { get; set; } = [];
}