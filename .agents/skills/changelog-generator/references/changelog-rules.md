# Changelog Rules

## Entry Style

Good:
- Added scenario comparison chart for trading strategies.
- Fixed duplicated trade calculation on portfolio refresh.
- Refactored Tailwind layout for mobile responsiveness.

Bad:
- Did many UI improvements.
- Fixed bugs.
- Updated code.

## Compactness Rules

- Prefer 8-16 words per entry.
- Remove filler words.
- Merge related UI changes into one entry.
- Mention affected module when relevant.

## Breaking Changes

Use:
- Changed authentication flow to require token refresh.
- Removed Bootstrap dependency from dashboard components.

## Multi-Module Repositories

Prefix entries when necessary:
- TradingBook: Added risk/reward visualization panel.
- API: Fixed JWT expiration validation.