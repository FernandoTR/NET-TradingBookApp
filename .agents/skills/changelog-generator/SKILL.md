---
name: changelog-generator
description: Generates and updates concise but understandable CHANGELOG.md files using semantic versioning and categorized entries. Use when the user wants to document project changes, releases, fixes, features, refactors, or breaking changes. Do not use for full project documentation, commit history dumps, or release note marketing copy.
---

# Changelog Generator Skill

## Objective

Generate or update a `CHANGELOG.md` file with compact, readable, and categorized entries.

## Instructions

1. Detect whether the repository already contains a `CHANGELOG.md`.

2. If the file does not exist:
   - Create a new `CHANGELOG.md`.
   - Read `assets/changelog-template.md`.
   - Use that structure exactly.

3. If the file already exists:
   - Preserve previous versions and formatting.
   - Insert the new version at the top below `# Changelog`.

4. Group changes using these categories only:
   - Added
   - Changed
   - Fixed
   - Removed
   - Refactored
   - Security

5. Keep every changelog entry:
   - Maximum 1 sentence.
   - Action-oriented.
   - Technically specific.
   - Compact but understandable.

6. Do not:
   - Copy raw commit messages.
   - Include implementation details irrelevant to users.
   - Add empty categories.
   - Write paragraphs.

7. Normalize wording:
   - Start with verbs.
   - Use present or past consistent tense.
   - Avoid vague terms like "improved stuff" or "various fixes".

8. If the user provides commits:
   - Summarize duplicated work into a single entry.
   - Merge related changes when possible.

9. Read `references/changelog-rules.md` before generating entries if:
   - The repository contains multiple modules.
   - The changes include breaking changes.
   - The commits are noisy or ambiguous.

10. Output only the final markdown content for the changelog section being added or updated.