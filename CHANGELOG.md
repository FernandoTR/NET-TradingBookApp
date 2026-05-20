# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-05-15

### Added
- User management module with account settings for email and password change.
- Two-factor authentication via authenticator app and email.
- Catalog modules for instruments, frames, account types, categories, and figures.
- Home dashboard with effectiveness chart, block statistics, and trigger statistics.
- Analysis modules for triggers, direction, days, figures, scenarios, stage, and time.
- Orders module with create, edit, delete, close, and table listing functionality.
- Order close automation: default comment assignment on TP/SL selection.
- Direction-based filtering across Orders, Home, and Analysis modules.
- My accounts section with balance tracking on the home page.
- Risk management calculator.
- Automated trade scoring service with dashboard scoring statistics.
- Trade grimoire with scenario type query and external visualization link.
- LoggingDbContext for centralized error log storage.
- Automatic account balance update on order creation.

### Fixed
- Two-factor authentication flow.
- Login error handling.
- Sidebar state persistence via cookie.
- Order table pagination.
- Manual date and time editing on order registration.

### Changed
- Solution renamed to TradingBookApp.
- Primary template color changed to yellow.
- Dashboard card widgets resized.
- Orders module visible record limit increased.
- Account views redesigned.

### Refactored
- Modals moved from individual views to shared layout.
- Dashboard redesigned with new statistics panels.

### Security
- Two-factor authentication enabled with app and email support.
