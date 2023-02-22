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