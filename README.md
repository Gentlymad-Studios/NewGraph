# NewGraph
![UnityVersion](https://img.shields.io/static/v1?label=unity&message=2022.2%2B&color=blue&style=flat&logo=Unity)
![Version](https://img.shields.io/github/package-json/v/Gentlymad-Studios/NewGraph)
![GitHub last commit](https://img.shields.io/github/last-commit/Gentlymad-Studios/NewGraph)
![GitHub](https://img.shields.io/github/license/Gentlymad-Studios/NewGraph)
[![GitHub issues](https://img.shields.io/github/issues-raw/Gentlymad-Studios/NewGraph)](https://github.com/Gentlymad-Studios/NewGraph/blob/main/CHANGELOG.md)
![image](https://user-images.githubusercontent.com/530629/219878506-2a12f872-cf5b-468e-8982-066c742bb8e7.png)
NewGraph is a data-oriented node graph solution for **Unity** powered by **UIToolkit (UIElements)**. This is based on the idea to visualize complex data structures as graph networks without having to modify already established data classes, except adding `[Node]`, `[Port]`, `[PortList]` and `[SerializeReference]` attributes to all classes that should show in the Graph View.

_This is planned to receive long term support as it is an integral part of our internal suite of tools at Gentlymad Studios._

### Changelog 📢
See the [Changelog](https://github.com/Gentlymad-Studios/NewGraph/blob/main/CHANGELOG.md) for current updates!

### Requirements 🌵
1. [GraphViewBase](https://github.com/Gentlymad-Studios/GraphViewBase)
2. [OdinSerializer](https://github.com/Gentlymad-Studios/OdinSerializer) (Original OdinSerializer repository: https://github.com/TeamSirenix/odin-serializer)
3. This project requires ![UnityVersion](https://img.shields.io/static/v1?label=unity&message=2022.2%2B&color=blue&style=flat&logo=Unity) (or later) as the Unity version.

### Features 🍒
* `WYSIWYG.` A port connecting to a node in the graph is a `real reference` to the object in your `graph .asset`
* Good performance even with many nodes as this is built upon  the new retained `UIToolkit` UI system.
* create visual data oriented networks based on your custom `[Serializable]` data classes simply by `adding attributes`.
* `Fully serialized dynamic lists` of ports with the [`[PortList]`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage-%F0%9F%91%88#portlist) attribute.
* Customize data visualization with [`[GraphDisplay]`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage-%F0%9F%91%88#graphdisplay): Display a field inside the graph, side inspector or both views.
* Full support of Unity's native `Undo/Redo` stack.
* Support for `Utility Nodes` that can be created to help organize your specific graph business.
* `Copy & Paste` even across graphs.
* No child `ScriptableObject` mayhem! A graph asset will hold all your data in `one single scriptable object`.
* `Cyclic references` and `complex reference chains` are natively supported with the use of [SerializeReference] attribute.
* `No reliance` on the now deprecated `Experimental.GraphView`. Everything was written from scratch or is based on maintainable code.
* `Keyboard shortcuts`, a `searchable context menu`, default UtilityNodes like [`CommentNode`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage-%F0%9F%91%88#commentnode), [`GroupCommentNode`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage-%F0%9F%91%88#groupcommentnode), a extensively commented code base and more...

### Wiki 📒
See the [Wiki](https://github.com/Gentlymad-Studios/NewGraph/wiki) for installation & usage instructions.

~ Use at your own risk. ~
