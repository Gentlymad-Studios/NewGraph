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

## [0.0.8] - 2023-03-01
### Fixed
- potential fix for wrong window size of the context menu

## [0.0.9] - 2023-03-01
### Fixed
- created a workaround for node references losing their "live connection" after being added as a reference
- fixed possible null ref in GroupCommentNode
### Added
- added ability to pan the view with alt/or options key + left click
- added ability to rename ports via the [Node] or [Port] attribute

## [0.1.0] - 2023-03-02
### Fixed
- added a more graceful fallback if the template GraphSettings file could not be retrieved
### Added
- refactored create dialog in the GraphView to use EditorUtility.SaveFilePanel so the user can decide & customize the location of new graphs
- Added a custom stylesheet option in the GraphSettings so all visuals can be customized using a custom .uss file

## [0.1.1] - 2023-03-03
### Fixed
- Fixed losing right click & key down events after docking the window
- Added Auto reloading the graph after exiting playmode, so we don't lose our VectorImage contents

## [0.1.2] - 2023-03-03
### Added
- Refactored property and field visibility
- class object fields can now utilize the GraphDisplay property correctly
- added createGroup option to GraphDisplay attribute to prevent creating foldouts if this is not desireable
### Fixed
- Fixed wrong property visibility
- groups for fields are now only created if they have properties inside them that should be drawn

## [0.1.3] - 2023-03-03
### Added
- Added initial NodeEditor functionality. Add CustomNodeEditorAttribute to a class inheriting from NodeEditor and set the nodeType. Works very similar to CustomEditor for mono behaviors.

## [0.1.4] - 2023-03-04
### Added
- Extended NodeEditor functionality, so that derived nodetypes can also receive the same inspector. Set the bool property in CustomNodeEditorAttribute to true to enable this.
