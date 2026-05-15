namespace DevOpsAiHub.Application.Features.AI.UseCase;

using DevOpsAiHub.Application.Features.AI.DTOs;
using System.Text;

public static class ContextPromptBuilder
{
    public static (string SystemPrompt, string UserPrompt) Build(
        string question,
        RankedContextDto ranked)
    {
        var system = new StringBuilder();

        system.AppendLine("You are an expert DevOps engineer.");
        system.AppendLine();
        system.AppendLine("## HOW TO PROCESS EACH REQUEST");
        system.AppendLine("Before answering, follow this exact reading order:");
        system.AppendLine();
        system.AppendLine("STEP 1 — READ THE CHAT HISTORY (already provided above)");
        system.AppendLine("  - Review what has already been discussed.");
        system.AppendLine("  - Identify follow-up intent or references such as 'it', 'that', or 'the above'.");
        system.AppendLine("  - Use this to correctly interpret the user's current question.");
        system.AppendLine();
        system.AppendLine("STEP 2 — UNDERSTAND THE CURRENT QUESTION");
        system.AppendLine("  - Determine the user's exact intent, informed by the chat history.");
        system.AppendLine();
        system.AppendLine("STEP 3 — CONSULT THE RETRIEVED CONTEXT (provided in the user message)");
        system.AppendLine("  - Read each context block and judge its relevance.");
        system.AppendLine("  - Use only relevant blocks; IGNORE unrelated ones.");
        system.AppendLine("  - Cite each used block inline as [#1], [#2], etc.");
        system.AppendLine();
        system.AppendLine("## CRITICAL RULES");
        system.AppendLine("- NO GUESSING: If relevant context is insufficient, reply EXACTLY: 'I don't have enough information about that.'");
        system.AppendLine("- CITATION: Always cite the context blocks you rely on as [#N].");
        system.AppendLine("- STACKOVERFLOW FORMAT: When using a StackOverflow block, mention its title and relevant tags.");
        system.AppendLine("- TONE: Be concise, accurate, and focused on technical DevOps solutions.");

        var user = new StringBuilder();

        user.AppendLine($"CURRENT QUESTION: {question}");
        user.AppendLine();
        user.AppendLine("RETRIEVED CONTEXT (Step 3 — for reference only):");

        int ordinal = 1;
       
        if (ranked?.MergedRanked != null)
        {
            foreach (var ranked_hit in ranked.MergedRanked)
            {
                var h = ranked_hit.Hit;

                user.AppendLine($"[#{ordinal}]");

                if (h.CollectionType == VectorCollectionType.QA && h.QaPayload is { } qa)
                {
                    user.AppendLine($"[Source: StackOverflow] Title: {qa.QuestionTitle}");
                    user.AppendLine($"Tags: {string.Join(", ", qa.Tags ?? Array.Empty<string>())}");
                    user.AppendLine($"Content:\n{qa.TextContent}");
                }
                else if (h.ArticlePayload is { } art)
                {
                    user.AppendLine($"[Source: Document] Title: {art.Title}");
                    user.AppendLine($"File: {art.SourceFile}");
                    user.AppendLine($"Content:\n{art.Content}");
                }
                else
                {
                    user.AppendLine($"[Source: Text]\n{h.Text}");
                }

                user.AppendLine("---");
                ordinal++;
            }
        }
        else
        {
            user.AppendLine("No context retrieved.");
        }

        return (system.ToString(), user.ToString());
    }
}