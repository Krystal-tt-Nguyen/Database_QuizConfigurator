
using MongoDB.Bson;

namespace Laboration_3.Model;

enum Difficulty { Easy, Medium, Hard }

internal class QuestionPack
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public Difficulty Difficulty { get; set; }
    public int TimeLimitInSeconds { get; set; }
    public List<Question> Questions { get; set; }
    public string? Category { get; set; }

    public QuestionPack(string name = "<PackName>", string? category = null, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 30)
    {
        Id = new ObjectId();
        Name = name;
        Category = category;
        Difficulty = difficulty;
        TimeLimitInSeconds = timeLimitInSeconds;
        Questions = new List<Question>();
    }

   /* public QuestionPack(QuestionPack pack)
    {
        Id = pack.Id;
        Name = pack.Name;
        Category = pack.Category;
        TimeLimitInSeconds = pack.TimeLimitInSeconds;
        Questions = pack.Questions;
    }*/
}
