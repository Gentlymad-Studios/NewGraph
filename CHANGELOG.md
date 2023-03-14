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

## [0.1.5] - 2023-03-09
### Added
- Added ability to show & hide the inspector panel
- Added tooltips for command panel buttons
- Added "nodeName" field for NodeAttribute so you can customize the nodeName to your liking
### Removed
- Removed Save dialog as it is legacy and not needed

## [0.1.6] - 2023-03-09
### Added
- Added new way to handle the Settings file, that avoids the need to store it in the Assets folder (you can reach the graph settings via ProjectSettings)
- Settings can now also be reverted on a per property basis or alltogether
### Removed
- Removed helper scripts that are not longer needed
- Removed old way to traverse SerializedProperty (was faulty) and extended CreateGenericUI method

## [0.1.7] - 2023-03-09
### Added
- Added ability for EditableLabelFields to be exited via escape or return

## [0.1.8] - 2023-03-09
### Fixed
- Fixed settings file not serializing correctly.

## [0.1.9] - 2023-03-09
### Added
- Added workaround for bugged KeyDownEvent by implementing global key events
### Fixed
- Fixed bug in group creation logic that could lead to properties being part of groups because their names were similar
- Fixed error when using undo after a node was deleted
- Removed old keydown recognition system
- fixed a visibility related null ref in GraphModelEditor
- removed faulty isExpanded property from foldouts (they are just claused on default now)
- fixed HasSelectedEdges check that worked with the wrong field
- fixed long foldout label that prevented node dragging

## [0.2.0] - 2023-03-10
### Added
- implemented custom foldout states for groups/foldouts and indepedently from nodeView and inspector. This way a graph will always keep track over every expanded state for each node.
- implemented empty method in graphController to extend for new EdgeDrop action

## [0.2.1] - 2023-03-10
### Added
- Catered for special case where a field is a managedReference to avoid double headers
### Fixed
- fixed case where a group would not be drawn/ appear correctly
- fixed group label being cut off

## [0.2.2] - 2023-03-11
### Added
- added a context menu when dropping an edge into empty space
### Fixed
- Fixed port lists that could become unresponsive
- fixed possible null ref in managed references group handling

## [0.2.3] - 2023-03-14
### Added
- added renaming action as a shortcut and menu action (F2) for a selected node
### Fixed
- refactored key down detection system
- fixed GraphSettings not being created on first run

## [0.2.4] - 2023-03-14
### Fixed
- fixed selection info not updating properly [#1](https://github.com/Gentlymad-Studios/NewGraph/issues/1)

