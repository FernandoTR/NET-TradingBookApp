# Changelog

All notable changes to this project will be documented in this file.

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
