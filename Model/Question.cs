
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Laboration_3.Model
{
    internal class Question
    {
        public ObjectId Id { get; set; }
        public string Query { get; set; }
        public string CorrectAnswer { get; set; }
        public string[] IncorrectAnswers { get; set; }       

        public Question(string query, string correctAnswer, 
            string incorrectAnswer1, string incorrectAnswer2, string incorrectAnswer3)
        {
            Query = query;
            CorrectAnswer = correctAnswer;
            IncorrectAnswers = new string[3] {incorrectAnswer1, incorrectAnswer2, incorrectAnswer3 };
            Id = new ObjectId();
        }

    }
}
