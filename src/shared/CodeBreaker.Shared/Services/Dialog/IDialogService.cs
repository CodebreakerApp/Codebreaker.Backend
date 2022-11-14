using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBreaker.Shared.Services.Dialog;

public record CodeBreakerDialogContext(Type ComponentType, Dictionary<string, object> Parameters, string DialogTitle);

public interface ICodeBreakerDialogService
{
    EventHandler<CodeBreakerDialogContext> ShowDialogHandler { get; set; }
    void ShowDialog(CodeBreakerDialogContext context);
}

public class CodeBreakerDialogService : ICodeBreakerDialogService
{
    public EventHandler<CodeBreakerDialogContext> ShowDialogHandler { get; set; } = default!;

    public void ShowDialog(CodeBreakerDialogContext context)
    {
        ShowDialogHandler.Invoke(this, context);
    }
}
