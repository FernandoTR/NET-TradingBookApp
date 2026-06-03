# Changelog

All notable changes to this project will be documented in this file.

## [1.9.0] - 2026-06-02

### Added
- Created AnalyticsConvergence page to cross up to 5 variables (Trigger, Scenery, Direction, Frame, Figure) and identify best-performing combinations ranked by a compound Score.
- Added stored procedure `usp_GetTBAnalyticsConvergence` with dynamic GROUP BY and Score formula `(TP1Rate*10 + TP2Rate*20 + TP3Rate*70) * MIN(Trades/50, 1)`.
- Added permission `AnalyticsConvergence = 18`, DTOs, repository, service, controller, and Metronic Tailwind view with toggle-based convergence panel and DataTable server-side rendering.

## [1.8.0] - 2026-05-29

### Changed
- Migrated solution from .NET 8.0 to .NET 10.0.300 with all Microsoft packages updated to 10.0.8.
- Upgraded EntityFrameworkCore, Identity, Hosting, Diagnostics, and CodeGeneration packages to 10.0.x versions.
- Updated third-party packages: MailKit 4.17.0, Newtonsoft.Json 13.0.4, QRCoder 1.8.0, Selenium 4.44.0.

### Security
- All NuGet packages audited; no critical vulnerabilities introduced by the migration.

## [1.7.0] - 2026-05-28

### Security
- Hardcoded credentials removed; sensitive data moved to config/secrets.json excluded from git.
- Rate limiting added to Account endpoints with AccountPolicy (10 requests per 5 seconds per IP).
- ModelState validation enabled in AccountController.Login (previously commented out).
- ForgotPassword confirmed to protect against user enumeration by always redirecting to confirmation.

### Changed
- Connection strings updated to use Encrypt=true instead of TrustServerCertificate=True.
- TrustServerCertificate and Persist Security Info removed from all connection strings.

## [1.6.0] - 2026-05-28

### Changed
- Manage/AddCash and WithdrawCash views migrated to Tailwind CSS with InputMaskHelper.decimal and KTModal API.
- Manage/Index modal launch updated to use KTModal API instead of jQuery modal.
- Account views (ConfirmEmail, ForgotPasswordConfirmation, NewPassword, NewPasswordConfirmation, ResetPassword, Send2FACode) migrated to Tailwind CSS.

## [1.5.0] - 2026-05-27

### Changed
- Manage/Index tab navigation migrated from Bootstrap tabs to kt-tabs with Tailwind styling.
- Manage/_Overview profile details migrated to kt-card with kt-table-auto layout.
- Manage/_Balance account cards, date range picker, and DataTable migrated to Metronic Tailwind with KTFlatpickr and updated DataTables 2.x configuration.
- Manage/_Settings email and password change forms migrated to kt-input with Tailwind layout; 2FA section migrated to kt-card-group.
- Manage/EnableAuthenticator migrated to Metronic Tailwind with kt-card structure.
- Two-factor authentication modal switched from Bootstrap modal to KTModal API.
- Badge render functions in Utilities.js updated to kt-badge-light kt-badge-{color} and balance display classes converted to Tailwind utilities.

## [1.4.0] - 2026-05-25

### Changed
- Orders/Index migrated to Metronic Tailwind with header container, kt-drawer filters using kt-select, DataTables 2.x configuration, and KTModal API.
- Orders/New migrated to Metronic Tailwind with KTScrollspy sidebar, kt-card section layout, kt-select dropdowns, unified FormValidation, and InputMaskHelper.decimal.
- Orders/Edit migrated to Metronic Tailwind with KTScrollspy sidebar, kt-card section layout, kt-select dropdowns, and FormValidation following CatFigure pattern.
- Orders/Close migrated to Metronic Tailwind with KTScrollspy sidebar, kt-card section layout, and kt-checkbox replacing Bootstrap checkboxes.
- Non-open order action column restricted to View only when direction data is present.
- Shared layout kt-modal body padding removed for cleaner rendering.

## [1.3.0] - 2026-05-24

### Added
- AnalyticsDay Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.
- AnalyticsDirection Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.
- AnalyticsStage Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.
- AnalyticsTime Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.

### Changed
- Columnar percentage values (SL%, TP1%, TP2%, TP3%) consolidated into main columns (SL, TP1, TP2, TP3) using progress bar rendering.
- DataTables configuration updated to 2.x standard with modern layout and search controls.

## [1.2.0] - 2026-05-21

### Changed
- Employees views (Index, New, Edit) migrated to Metronic Tailwind with kt-card, kt-input, kt-btn, and kt-form-label.
- Row action menus replaced with Tailwind kt-menu dropdown across all modules via ActionButtonHelper.
- Shared layout modals updated to kt-modal structure with scrollable body and simplified close button.
- DataTables filter inputs and selects converted from Bootstrap form-control to Metronic Tailwind kt-input.
- CatCategory views (Index, New, Edit) migrated to Metronic Tailwind following the same pattern as Employees.
- CatFigure views (Index, New, Edit) migrated to Metronic Tailwind with kt-badge status column and KTModal dialogs.
- CatAccountType views (Index, New, Edit) migrated to Metronic Tailwind following the same pattern as CatFigure.
- CatFrame views (Index, New, Edit) migrated to Metronic Tailwind following the same pattern as CatCategory.
- Shared badge render functions (renderStatus, renderStatusEmployee, renderAccountType, renderStatusAnalytics) in Utilities.js converted from Bootstrap badges to kt-badge.
- CatInstruments views (Index, New, Edit) migrated to Metronic Tailwind following the same pattern as CatFigure.
- renderIconCoin in Utilities.js migrated from Bootstrap symbol classes to Tailwind rounded-full size-9.
- Roles views (Index, New, Edit) migrated to Metronic Tailwind following the same pattern as CatFigure; jsTree permission tree wrapped in Tailwind kt-form-label container.
- Users views (Index, Edit) migrated to Metronic Tailwind with kt-card table, KTModal dialogs, and CatFigure-style FormValidation for role assignment checkboxes.
- renderTrueFalse and renderFlag icon functions in Utilities.js converted from Bootstrap sizing/color classes to Tailwind text-green-600 text-2xl and text-destructive.
- Logs Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer date range filter.
- AnalyticsTrigger Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.
- AnalyticsScenery Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select.
- renderProgressBar in Utilities.js migrated from Bootstrap progress bars to Tailwind flex h-1.5 rounded-full with mapped color classes.
- AnalyticsFigure Index view migrated to Metronic Tailwind with kt-card, kt-table, and kt-drawer filter using kt-select, removing percentage columns (SL%, TP1%, TP2%, TP3%).

### Added
- Flatpickr date picker integrated with dedicated dark theme CSS and KTFlatpickr helper utility.

## [1.1.0] - 2026-05-20

### Added
- Metronic Tailwind template framework with core CSS/JS bundles.
- Vendor libraries for ApexCharts, TinyMCE v6, Leaflet, Dropzone, and KeenIcons.
- Updated template media assets, illustrations, and avatars.

### Changed
- Layout upgraded to load Tailwind CSS/JS alongside existing Bootstrap assets.
- Home/Index and SignIn views adapted for hybrid Bootstrap/Tailwind coexistence.
- Dashboard partials, menu helper, and DI registration updated for new framework.

### Removed
- Old Metronic Bootstrap plugins: CKEditor, DataTables, Cropper, jKanban, jstree, FlotCharts, FullCalendar, Draggable, and FS Lightbox.
- Legacy FontAwesome, Line-Awesome, and KeenIcons icon font files.
- Deprecated template stock images, SVG shapes, and illustration sets.

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
