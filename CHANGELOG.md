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

## [0.2.5] - 2023-03-14
### Fixed
- fixed issue with change callback handler for property fields in settings file

## [0.2.6] - 2023-03-14
### Fixed
- Removed unnecessary frame delay when reloading a graph [#2](https://github.com/Gentlymad-Studios/NewGraph/issues/2)
- Removed Unbind call when clearing a graph
- PortListView entries will now update their name, if the referenced node name is changed

## [0.2.7] - 2023-03-15
### Added
- added new edge drop window based on searchwindow instead of GenericMenu
- added CustomContextMenu attribute to customize the default context menu [#3](https://github.com/Gentlymad-Studios/NewGraph/issues/3)
- added CustomEdgeDropMenu attribute to customize the default edge drop menu

## [0.2.8] - 2023-03-15
### Fixed
- Fixed possible ArgumentException caused by the new ContextMenu architecture

## [0.2.9] - 2023-03-16
### Added
- Added createInputPort to [Node] attribute. This allows to hide the input port for a node. [#6](https://github.com/Gentlymad-Studios/NewGraph/issues/6)
- Added comments to ContextMenu, EdgeDropMenu so it is easier to understand & extend

## [0.3.0] - 2023-03-17
### Added
- Added ShouldColorizeBackground that can be overridden in NodeEditors to prevent the background from being automatically colorized

## [0.3.1] - 2023-03-27
### Fixed
- Fixed double click on asset not opening graph, when window was already open. [#7](https://github.com/Gentlymad-Studios/NewGraph/issues/7)
- Some refactoring to opening a graph externally

## [0.3.2] - 2023-04-18
### Added
- Graphs can now be created in a scene context and use scene references. To create a graph add the MonoGraphModel component to a gameobject.
- Added ability to rename the GraphWindow [#11](https://github.com/Gentlymad-Studios/NewGraph/issues/11)
### Fixed
- Major refactoring and minor bugfixes

## [0.3.3] - 2023-04-18
### Added
- Added more stable method to retrieve all attributes for a serialized property. [#14](https://github.com/Gentlymad-Studios/NewGraph/issues/14)
### Fixed
- Fixed wrong window size for all SearchWindows [#10](https://github.com/Gentlymad-Studios/NewGraph/issues/10)
- wrapped UnityEditor in #if UNITY_EDITOR preprocessor directives in all graph models

## [0.3.4] - 2023-04-24
### Added
- Added integrated Examples (Samples~). To use: Copy contents from  Samples~ folder into you Assets project.
- Added ability to add new window/ graph types to the graph window lookup

## [0.3.5] - 2023-04-25
### Fixed
- Made it easier to derive custom ScriptableGraph models

## [0.3.6] - 2023-04-28
### Fixed
- Fixed issue with undoing changes
- Fixed port list connections not regenerating when adding a new item

## [0.3.7] - 2023-05-08
### Fixed
- Fixed StackOverflow errors for GroupCommentNodes when they are stacked into each other [#20](https://github.com/Gentlymad-Studios/NewGraph/issues/20)
- Fixed wrong positioning of Nodes that are part of a GroupCommentNode [#21](https://github.com/Gentlymad-Studios/NewGraph/issues/21)
### Added
- Nodes are now only included in a GroupCommentNode if they are fully contained and not just overlapping [#22](https://github.com/Gentlymad-Studios/NewGraph/issues/22)

## [0.3.8] - 2023-05-09
### Fixed
- Removed janky workaround for re-initializing a graph after playmode in exchange for a better solution. Fixes: [#23](https://github.com/Gentlymad-Studios/NewGraph/issues/23)

## [0.3.9] - 2023-05-10
### Fixed
- Fixed GroupCommentNode Headline cutting off text. Fixes: [#24](https://github.com/Gentlymad-Studios/NewGraph/issues/24)
- Fixed Frame action being triggerd while in an editable VisualElement. Fixes: [#27](https://github.com/Gentlymad-Studios/NewGraph/issues/27)
### Added
- Added customization of the hierarchical layer for GroupCommentNodes. Fixes: [#25](https://github.com/Gentlymad-Studios/NewGraph/issues/25)

## [0.4.0] - 2023-07-10
### Fixed
- Fixed removing entire nodes when hitting the delete key while an input field was focussed.

## [0.4.1] - 2023-08-21
### Fixed
- Fixed GraphDisplayAttribute createGroup behavior
