# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.1] - 2023-02-18
### Added
- Initial commit

## [0.0.2] - 2023-02-18
### Added
- Added GraphSettings copying procedure
- Refactored SettingsPRovider code to be more generic

## [0.0.3] - 2023-02-19
### Removed
- removed partial classes for our main models (GraphModel & NodeModel) as this caused serialization errors
### Added
- re-added editor specific code back to GraphModel and NodeModel and added platform dependent compilation tags (#IF UNITY_EDITOR) where needed
- added CreateAssetMenu attribute to GraphModel so no graph items can be create by right clicking in the project window.

## [0.0.4] - 2023-02-22
### Removed
- removed extraction of [Space] and [Header] properties as this is no longer needed since UIElements/UIToolkti now fully supports PropertyDrawers

## [0.0.5] - 2023-02-23
### Fixed
- fixed missing references/connections when copy & pasting nodes
- fixed selected nodes text appearing even on a new graph
### Changed
- renamed [Output] to [Port] attribute to avoid misleading naming.
-renamed former [Port] attribute to [PortBase]

## [0.0.6] - 2023-02-24
### Added
- added a GroupCommentNode that allows to frame/group a section of nodes that can also be moved together
- made some API changes to give utility nodes even more flexibility
- added ShouldColorizeBackground method to IUtilityNode interface

## [0.0.7] - 2023-02-27
### Added
- added Undo capabilities when changing node dimensions for the GroupCommentNode