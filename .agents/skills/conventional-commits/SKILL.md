---
name: conventional-commits
description: Generates, validates, and standardizes Git commit messages using the Conventional Commits 1.0.0 specification and commitlint rules. Use when the user wants semantic commits, commitlint configuration, automated commit validation, changelog-friendly commits, or standardized Git history. Do not use for branching strategies, GitFlow workflows, or repository administration tasks unrelated to commit messages.
---

# Conventional Commits

This skill standardizes Git commit messages using the Conventional Commits specification and commitlint ecosystem.

References:
- Conventional Commits 1.0.0
- commitlint configuration standards

See:
- `references/commit-types.md`
- `references/breaking-changes.md`
- `references/scopes-guide.md`
- `references/commitlint-rules.md`

Templates:
- `assets/commit-template.md`
- `assets/pull-request-template.md`
- `assets/examples.md`

Validation script:
- `scripts/validate-commit.sh`

## Primary Workflow

1. Identify the intent of the change.

2. Determine the correct commit type using `references/commit-types.md`.

3. Determine whether the commit requires:
   - scope
   - breaking change marker
   - footer metadata

4. Generate the commit message using the structure defined in `assets/commit-template.md`.

5. Validate the commit against commitlint rules from `references/commitlint-rules.md`.

6. If the commit introduces incompatible API or behavior changes:
   - Read `references/breaking-changes.md`
   - Add `!` after the type or scope
   - Add a `BREAKING CHANGE:` footer

7. If the user provides multiple changes:
   - Split unrelated changes into separate commits.
   - Preserve atomic commit structure.
