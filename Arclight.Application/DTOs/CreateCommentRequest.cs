public record CreateCommentRequest(string Text);
public record CommentResponse(Guid Id, string Text, string AuthorName, DateTimeOffset CreatedAt, Guid UserId);