namespace StateDesignPattern.Models;

/// <summary>
/// Who is asking. The lifecycle reads only these two capabilities, so the state classes stay
/// free of authentication, database or framework concerns.
/// </summary>
public sealed record ExamUser(bool MayAuthor, bool MayManage)
{
    public static ExamUser Author { get; } = new(MayAuthor: true, MayManage: false);

    public static ExamUser Admin { get; } = new(MayAuthor: true, MayManage: true);

    public static ExamUser Employee { get; } = new(MayAuthor: false, MayManage: false);

    public override string ToString() =>
        MayManage ? "Exams admin" : MayAuthor ? "Exam author" : "Employee";
}
